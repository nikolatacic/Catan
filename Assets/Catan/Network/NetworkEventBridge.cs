using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using GameCore.Events;
using GameCore.Board;
using GameCore.Resources;
using Catan.UI;

namespace Catan.Network
{
    // ── Host → clients event bridge ───────────────────────────────────────────
    // Sits on the same GameObject as NetworkCommandBridge in GameHotseat.
    //
    // Host side:
    //   - On spawn: broadcasts BoardSeed so clients can build the same board
    //   - Subscribes to local EventBus events; re-emits via ClientRpc
    //
    // Client side:
    //   - Receives BoardSeed → calls GameManager.CompleteInitialization(seed)
    //   - Receives event ClientRpcs → republishes locally via EventBus so
    //     the existing UI subscribers react identically to hotseat
    //
    // Hidden-state filtering (5d): ResourceProduced / ResourceAdded /
    // ResourceRemoved / ResourceStolen currently fan out in full. They will
    // be redacted per-client in a later phase so opponents see counts only.
    // ──────────────────────────────────────────────────────────────────────────

    public class NetworkEventBridge : NetworkBehaviour
    {
        public static NetworkEventBridge Instance { get; private set; }

        // Server-side: clientId → player index (host gets 0, first joiner 1, …).
        private readonly Dictionary<ulong, int> _clientToPlayerIndex = new();
        private int _nextPlayerIndex;

        public override void OnNetworkSpawn()
        {
            Instance = this;

            if (IsServer)
            {
                Debug.Log("[NetworkEventBridge] Host OnNetworkSpawn — subscribing to events and assigning host index.");
                SubscribeToHostEvents();
                // Host: claim index 0 locally — no RPC needed (host IS the server).
                AssignAndAnnounceLocally(NetworkManager.ServerClientId);
            }
            else
            {
                Debug.Log("[NetworkEventBridge] Client OnNetworkSpawn — requesting initial state from host.");
                // Client: ask the server for our initial state once our bridge is
                // spawned. This is more reliable than the server pushing right at
                // OnClientConnected — at that moment the client may not yet have
                // the in-scene NetworkObject ready to receive the RPC.
                RequestInitialStateServerRpc();
            }
        }

        public override void OnNetworkDespawn()
        {
            if (Instance == this) Instance = null;
            if (IsServer) UnsubscribeFromHostEvents();
        }

        // ── Player index assignment ────────────────────────────────────────────

        // Server-only: pick and remember an index for this client, set the local
        // session (server-side, used by host UI), publish the local event.
        private void AssignAndAnnounceLocally(ulong clientId)
        {
            if (_clientToPlayerIndex.ContainsKey(clientId)) return;
            int index = _nextPlayerIndex++;
            _clientToPlayerIndex[clientId] = index;
            NetworkSession.SetLocalPlayerIndex(index);
            EventBus.Publish(new LocalPlayerAssignedEvent { Index = index });
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestInitialStateServerRpc(ServerRpcParams rpc = default)
        {
            ulong clientId = rpc.Receive.SenderClientId;
            if (!_clientToPlayerIndex.TryGetValue(clientId, out int index))
            {
                index = _nextPlayerIndex++;
                _clientToPlayerIndex[clientId] = index;
            }

            var manager = GameManager.Instance;
            int seed = manager != null ? manager.BoardSeed : 0;
            Debug.Log($"[NetworkEventBridge] Server received RequestInitialState from clientId={clientId} — assigning index={index} seed={seed} managerExists={manager != null}");

            int currentActorIndex = -1;
            int currentTurnNumber = 0;
            int currentPhase = (int)CatanTurnPhase.SetupPlacement;
            if (manager != null && manager.TurnManager != null)
            {
                var current = manager.ActivePlayer;
                if (current != null)
                    currentActorIndex = manager.Players.IndexOf(current);
                currentTurnNumber = manager.TurnManager.TurnNumber;
                currentPhase = (int)manager.TurnManager.CurrentCatanPhase;
            }

            int playerCount = NetworkManager.ConnectedClientsIds.Count;

            var targetOnly = new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } }
            };
            SendInitialStateClientRpc(seed, index, playerCount, currentActorIndex, currentTurnNumber, currentPhase, targetOnly);
        }

        [ClientRpc]
        private void SendInitialStateClientRpc(int seed, int index, int playerCount, int currentActorIndex,
            int turnNumber, int phase, ClientRpcParams rpc = default)
        {
            // Targeted RPC, but NGO still delivers locally on the host with the
            // ClientRpc plumbing. Host already initialized itself, so guard.
            if (IsServer) return;

            Debug.Log($"[NetworkEventBridge] Client received initial state — seed={seed} index={index} playerCount={playerCount} actorIndex={currentActorIndex} turn={turnNumber} phase={phase}");

            var manager = GameManager.Instance;
            if (manager == null)
            {
                Debug.LogWarning("[NetworkEventBridge] Initial state arrived before GameManager existed.");
                return;
            }

            // Initialize the board FIRST so Players list is populated before
            // LocalPlayerAssignedEvent fires and RefreshButtons reads Players.Count.
            if (manager.Board == null)
            {
                Debug.Log($"[NetworkEventBridge] Client calling CompleteInitialization with seed={seed} playerCount={playerCount}");
                GameSession.SetNetworkedPlayers(playerCount);
                manager.CompleteInitialization(seed);
                Debug.Log($"[NetworkEventBridge] Client CompleteInitialization done. Players={manager.Players.Count} Board={(manager.Board != null ? "OK" : "NULL")}");
            }
            else
            {
                Debug.Log("[NetworkEventBridge] Board already initialized on client, skipping CompleteInitialization.");
            }

            // Set local player index and notify UI AFTER players exist.
            NetworkSession.SetLocalPlayerIndex(index);
            EventBus.Publish(new LocalPlayerAssignedEvent { Index = index });

            // Mirror the host's current turn state so client UI shows the right
            // active player and phase before any further events arrive.
            if (currentActorIndex >= 0 && currentActorIndex < manager.Players.Count)
            {
                var actor = manager.Players[currentActorIndex];
                manager.TurnManager.MirrorActor(actor, turnNumber);
                manager.TurnManager.MirrorPhase((CatanTurnPhase)phase);
                EventBus.Publish(new GameCore.Turn.TurnStartedEvent
                {
                    Actor = actor,
                    TurnNumber = turnNumber,
                });
                EventBus.Publish(new CatanPhaseChangedEvent
                {
                    From = (CatanTurnPhase)phase,
                    To = (CatanTurnPhase)phase,
                });
            }
            else
            {
                Debug.LogWarning($"[NetworkEventBridge] Could not mirror turn state — actorIndex={currentActorIndex} playerCount={manager.Players.Count}");
            }
        }

        // ── Host: subscribe to local events and fan out ────────────────────────

        private void SubscribeToHostEvents()
        {
            EventBus.Subscribe<DiceRolledEvent>(OnHostDiceRolled);
            EventBus.Subscribe<RobberMovedEvent>(OnHostRobberMoved);
            EventBus.Subscribe<KnightPlayedEvent>(OnHostKnightPlayed);
            EventBus.Subscribe<DevCardPurchasedEvent>(OnHostDevCardPurchased);
            EventBus.Subscribe<CatanPhaseChangedEvent>(OnHostPhaseChanged);
            EventBus.Subscribe<GameStateChangedEvent>(OnHostGameStateChanged);
            EventBus.Subscribe<GameCore.Turn.TurnStartedEvent>(OnHostTurnStarted);
            EventBus.Subscribe<GameCore.Build.BuildSucceededEvent>(OnHostBuildSucceeded);
            EventBus.Subscribe<GameCore.Score.ScoreChangedEvent>(OnHostScoreChanged);
            EventBus.Subscribe<GameCore.Score.VictoryAchievedEvent>(OnHostVictoryAchieved);
            EventBus.Subscribe<ResourceAddedEvent>(OnHostResourceAdded);
            EventBus.Subscribe<ResourceRemovedEvent>(OnHostResourceRemoved);
            EventBus.Subscribe<GameCore.Trade.TradeProposedEvent>(OnHostTradeProposed);
            EventBus.Subscribe<GameCore.Trade.TradeAcceptedEvent>(OnHostTradeAccepted);
            EventBus.Subscribe<GameCore.Trade.TradeRejectedEvent>(OnHostTradeRejected);
            EventBus.Subscribe<GameCore.Trade.TradeCancelledEvent>(OnHostTradeCancelled);
        }

        private void UnsubscribeFromHostEvents()
        {
            EventBus.Unsubscribe<DiceRolledEvent>(OnHostDiceRolled);
            EventBus.Unsubscribe<RobberMovedEvent>(OnHostRobberMoved);
            EventBus.Unsubscribe<KnightPlayedEvent>(OnHostKnightPlayed);
            EventBus.Unsubscribe<DevCardPurchasedEvent>(OnHostDevCardPurchased);
            EventBus.Unsubscribe<CatanPhaseChangedEvent>(OnHostPhaseChanged);
            EventBus.Unsubscribe<GameStateChangedEvent>(OnHostGameStateChanged);
            EventBus.Unsubscribe<GameCore.Turn.TurnStartedEvent>(OnHostTurnStarted);
            EventBus.Unsubscribe<GameCore.Build.BuildSucceededEvent>(OnHostBuildSucceeded);
            EventBus.Unsubscribe<GameCore.Score.ScoreChangedEvent>(OnHostScoreChanged);
            EventBus.Unsubscribe<GameCore.Score.VictoryAchievedEvent>(OnHostVictoryAchieved);
            EventBus.Unsubscribe<ResourceAddedEvent>(OnHostResourceAdded);
            EventBus.Unsubscribe<ResourceRemovedEvent>(OnHostResourceRemoved);
            EventBus.Unsubscribe<GameCore.Trade.TradeProposedEvent>(OnHostTradeProposed);
            EventBus.Unsubscribe<GameCore.Trade.TradeAcceptedEvent>(OnHostTradeAccepted);
            EventBus.Unsubscribe<GameCore.Trade.TradeRejectedEvent>(OnHostTradeRejected);
            EventBus.Unsubscribe<GameCore.Trade.TradeCancelledEvent>(OnHostTradeCancelled);
        }

        // ── Host handlers → ClientRpc ──────────────────────────────────────────

        private void OnHostDiceRolled(DiceRolledEvent e)
            => DiceRolledClientRpc(e.D1, e.D2, e.Total);

        private void OnHostRobberMoved(RobberMovedEvent e)
            => RobberMovedClientRpc(e.From.Q, e.From.R, e.To.Q, e.To.R, PlayerIndex(e.Mover));

        private void OnHostKnightPlayed(KnightPlayedEvent e)
            => KnightPlayedClientRpc(PlayerIndex(e.Player));

        private void OnHostDevCardPurchased(DevCardPurchasedEvent e)
            => DevCardPurchasedClientRpc(PlayerIndex(e.Player));

        private void OnHostPhaseChanged(CatanPhaseChangedEvent e)
            => PhaseChangedClientRpc((int)e.From, (int)e.To);

        private void OnHostGameStateChanged(GameStateChangedEvent _)
            => GameStateChangedClientRpc();

        private void OnHostTurnStarted(GameCore.Turn.TurnStartedEvent e)
            => TurnStartedClientRpc(PlayerIndex(e.Actor as GameCore.Player.IPlayer), e.TurnNumber);

        private void OnHostBuildSucceeded(GameCore.Build.BuildSucceededEvent e)
        {
            BuildPieceKind kind = e.Piece switch
            {
                Catan.Road        => BuildPieceKind.Road,
                Catan.CityUpgrade => BuildPieceKind.City,
                Catan.Settlement  => BuildPieceKind.Settlement,
                _ => BuildPieceKind.Unknown,
            };

            int q = 0, r = 0;
            byte index = 0;
            switch (e.Location)
            {
                case Catan.Road road:
                    if (BoardKeys.TryGetEdgeKey(road.Location, GameManager.Instance, out var et, out var ei))
                    { q = et.Q; r = et.R; index = ei; }
                    break;
                case Catan.Settlement s:
                    if (BoardKeys.TryGetVertexKey(s.Location, GameManager.Instance, out var st, out var sc))
                    { q = st.Q; r = st.R; index = sc; }
                    break;
                case Catan.CityUpgrade cu:
                    if (BoardKeys.TryGetVertexKey(cu.Location, GameManager.Instance, out var ct, out var cc))
                    { q = ct.Q; r = ct.R; index = cc; }
                    break;
            }

            BuildSucceededClientRpc(PlayerIndex(e.Player), (byte)kind, q, r, index);
        }

        private void OnHostScoreChanged(GameCore.Score.ScoreChangedEvent e)
            => ScoreChangedClientRpc(PlayerIndex(e.Player), e.OldScore, e.NewScore);

        private void OnHostVictoryAchieved(GameCore.Score.VictoryAchievedEvent e)
            => VictoryAchievedClientRpc(PlayerIndex(e.Winner));

        private void OnHostResourceAdded(ResourceAddedEvent e)
        {
            var bundle = AsBundleBytes(e.Added);
            ResourceAddedClientRpc(PlayerIndex(e.Player),
                bundle[0], bundle[1], bundle[2], bundle[3], bundle[4]);
        }

        private void OnHostResourceRemoved(ResourceRemovedEvent e)
        {
            var bundle = AsBundleBytes(e.Removed);
            ResourceRemovedClientRpc(PlayerIndex(e.Player),
                bundle[0], bundle[1], bundle[2], bundle[3], bundle[4]);
        }

        private void OnHostTradeProposed(GameCore.Trade.TradeProposedEvent e)
        {
            if (e.Offer.Target == null) return; // bank trade
            var offerBytes = AsBundleBytes(e.Offer.Offering);
            var wantBytes  = AsBundleBytes(e.Offer.Requesting);
            PlayerTradeProposedClientRpc(
                PlayerIndex(e.Offer.Proposer), PlayerIndex(e.Offer.Target),
                offerBytes[0], offerBytes[1], offerBytes[2], offerBytes[3], offerBytes[4],
                wantBytes[0],  wantBytes[1],  wantBytes[2],  wantBytes[3],  wantBytes[4]);
        }

        private void OnHostTradeAccepted(GameCore.Trade.TradeAcceptedEvent e)
            => PlayerTradeAcceptedClientRpc(PlayerIndex(e.Responder), PlayerIndex(e.Offer.Proposer));

        private void OnHostTradeRejected(GameCore.Trade.TradeRejectedEvent e)
            => PlayerTradeDeclinedClientRpc(PlayerIndex(e.Responder), PlayerIndex(e.Offer.Proposer));

        private void OnHostTradeCancelled(GameCore.Trade.TradeCancelledEvent e)
        {
            if (e.Offer.Target == null) return; // bank trade
            PlayerTradeCancelledClientRpc(PlayerIndex(e.Offer.Proposer));
        }

        // ── ClientRpcs → republish on every client ─────────────────────────────

        [ClientRpc]
        private void DiceRolledClientRpc(int d1, int d2, int total)
        {
            if (IsServer) return;
            EventBus.Publish(new DiceRolledEvent { D1 = d1, D2 = d2, Total = total });
        }

        [ClientRpc]
        private void RobberMovedClientRpc(int fromQ, int fromR, int toQ, int toR, int moverIndex)
        {
            if (IsServer) return;
            EventBus.Publish(new RobberMovedEvent
            {
                From = new HexCoord(fromQ, fromR),
                To = new HexCoord(toQ, toR),
                Mover = PlayerAt(moverIndex),
            });
        }

        [ClientRpc]
        private void KnightPlayedClientRpc(int playerIndex)
        {
            if (IsServer) return;
            EventBus.Publish(new KnightPlayedEvent { Player = PlayerAt(playerIndex) });
        }

        [ClientRpc]
        private void DevCardPurchasedClientRpc(int playerIndex)
        {
            if (IsServer) return;
            EventBus.Publish(new DevCardPurchasedEvent { Player = PlayerAt(playerIndex) });
        }

        [ClientRpc]
        private void PhaseChangedClientRpc(int from, int to)
        {
            if (IsServer) return;
            GameManager.Instance?.TurnManager?.MirrorPhase((CatanTurnPhase)to);
            EventBus.Publish(new CatanPhaseChangedEvent
            {
                From = (CatanTurnPhase)from,
                To = (CatanTurnPhase)to,
            });
        }

        [ClientRpc]
        private void GameStateChangedClientRpc()
        {
            if (IsServer) return;
            EventBus.Publish(new GameStateChangedEvent());
        }

        [ClientRpc]
        private void TurnStartedClientRpc(int actorIndex, int turnNumber)
        {
            if (IsServer) return;
            var actor = PlayerAt(actorIndex) as GameCore.Turn.ITurnActor;
            if (actor == null) return;
            GameManager.Instance?.TurnManager?.MirrorActor(actor, turnNumber);
            EventBus.Publish(new GameCore.Turn.TurnStartedEvent
            {
                Actor = actor,
                TurnNumber = turnNumber,
            });
        }

        [ClientRpc]
        private void BuildSucceededClientRpc(int playerIndex, byte kind, int q, int r, byte index)
        {
            if (IsServer) return;
            var player = PlayerAt(playerIndex);
            if (player == null) return;

            var manager = GameManager.Instance;
            if (manager == null) return;

            GameCore.Build.IPlaceable piece     = null;
            GameCore.Build.IBuildLocation loc   = null;

            switch ((BuildPieceKind)kind)
            {
                case BuildPieceKind.Road:
                {
                    var edges = manager.Board.Grid.GetEdges(new HexCoord(q, r));
                    if (index >= edges.Count) return;
                    var road = new Catan.Road(player, edges[index]);
                    manager.Board.Roads[edges[index]] = road;
                    (player as CatanPlayer)?.Roads.Add(road);
                    piece = road;
                    loc   = road;
                    break;
                }
                case BuildPieceKind.Settlement:
                {
                    var vertices = manager.Board.Grid.GetVertices(new HexCoord(q, r));
                    if (index >= vertices.Count) return;
                    var settlement = new Catan.Settlement(player, vertices[index]);
                    manager.Board.Settlements[vertices[index]] = settlement;
                    (player as CatanPlayer)?.Settlements.Add(settlement);
                    piece = settlement;
                    loc   = settlement;
                    break;
                }
                case BuildPieceKind.City:
                {
                    var vertices = manager.Board.Grid.GetVertices(new HexCoord(q, r));
                    if (index >= vertices.Count) return;
                    if (manager.Board.Settlements.TryGetValue(vertices[index], out var existing))
                        existing.UpgradeToCity();
                    piece = new Catan.CityUpgrade(player, vertices[index]);
                    loc   = (Catan.CityUpgrade)piece;
                    break;
                }
            }

            EventBus.Publish(new GameCore.Build.BuildSucceededEvent
            {
                Player = player, Piece = piece, Location = loc,
            });
        }

        [ClientRpc]
        private void ScoreChangedClientRpc(int playerIndex, int oldScore, int newScore)
        {
            if (IsServer) return;
            EventBus.Publish(new GameCore.Score.ScoreChangedEvent
            {
                Player = PlayerAt(playerIndex),
                OldScore = oldScore,
                NewScore = newScore,
            });
        }

        [ClientRpc]
        private void VictoryAchievedClientRpc(int playerIndex)
        {
            if (IsServer) return;
            EventBus.Publish(new GameCore.Score.VictoryAchievedEvent
            {
                Winner = PlayerAt(playerIndex),
            });
        }

        [ClientRpc]
        private void ResourceAddedClientRpc(int playerIndex, byte wood, byte brick,
            byte sheep, byte wheat, byte ore)
        {
            if (IsServer) return;
            var player = PlayerAt(playerIndex) as CatanPlayer;
            if (player == null) return;
            // ResourceInventory.TryAdd publishes ResourceAddedEvent internally,
            // so client UI subscribers fire from that — no manual publish needed.
            player.Resources.TryAdd(BundleFromBytes(wood, brick, sheep, wheat, ore));
        }

        [ClientRpc]
        private void ResourceRemovedClientRpc(int playerIndex, byte wood, byte brick,
            byte sheep, byte wheat, byte ore)
        {
            if (IsServer) return;
            var player = PlayerAt(playerIndex) as CatanPlayer;
            if (player == null) return;
            player.Resources.TryRemove(BundleFromBytes(wood, brick, sheep, wheat, ore));
        }

        [ClientRpc]
        private void PlayerTradeProposedClientRpc(
            int proposerIndex, int targetIndex,
            byte offerWood, byte offerBrick, byte offerSheep, byte offerWheat, byte offerOre,
            byte wantWood,  byte wantBrick,  byte wantSheep,  byte wantWheat,  byte wantOre)
        {
            if (IsServer) return;
            var proposer   = PlayerAt(proposerIndex);
            var target     = PlayerAt(targetIndex);
            if (proposer == null || target == null) return;
            var offering   = BundleFromBytes(offerWood, offerBrick, offerSheep, offerWheat, offerOre);
            var requesting = BundleFromBytes(wantWood,  wantBrick,  wantSheep,  wantWheat,  wantOre);
            EventBus.Publish(new GameCore.Trade.TradeProposedEvent
            {
                Offer = new GameCore.Trade.TradeOffer(proposer, target, offering, requesting)
            });
        }

        [ClientRpc]
        private void PlayerTradeAcceptedClientRpc(int responderIndex, int proposerIndex)
        {
            if (IsServer) return;
            EventBus.Publish(new GameCore.Trade.TradeAcceptedEvent
            {
                Responder = PlayerAt(responderIndex),
                Offer = new GameCore.Trade.TradeOffer(
                    PlayerAt(proposerIndex), PlayerAt(responderIndex),
                    new ResourceBundle(), new ResourceBundle()),
            });
        }

        [ClientRpc]
        private void PlayerTradeDeclinedClientRpc(int responderIndex, int proposerIndex)
        {
            if (IsServer) return;
            EventBus.Publish(new GameCore.Trade.TradeRejectedEvent
            {
                Responder = PlayerAt(responderIndex),
                Offer = new GameCore.Trade.TradeOffer(
                    PlayerAt(proposerIndex), PlayerAt(responderIndex),
                    new ResourceBundle(), new ResourceBundle()),
            });
        }

        [ClientRpc]
        private void PlayerTradeCancelledClientRpc(int proposerIndex)
        {
            if (IsServer) return;
            var proposer = PlayerAt(proposerIndex);
            EventBus.Publish(new GameCore.Trade.TradeCancelledEvent
            {
                // Target is set to proposer to pass the bank-trade null filter in UI subscribers.
                Offer = new GameCore.Trade.TradeOffer(proposer, proposer, new ResourceBundle(), new ResourceBundle()),
            });
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private enum BuildPieceKind : byte { Unknown = 0, Settlement = 1, City = 2, Road = 3 }

        private static int PlayerIndex(GameCore.Player.IPlayer player)
        {
            var manager = GameManager.Instance;
            if (player == null || manager == null) return -1;
            return manager.Players.IndexOf(player as CatanPlayer);
        }

        private static GameCore.Player.IPlayer PlayerAt(int index)
        {
            var manager = GameManager.Instance;
            if (manager == null || index < 0 || index >= manager.Players.Count) return null;
            return manager.Players[index];
        }

        private static byte[] AsBundleBytes(ResourceBundle bundle)
        {
            return new[]
            {
                (byte)bundle.Get(CatanResources.Wood),
                (byte)bundle.Get(CatanResources.Brick),
                (byte)bundle.Get(CatanResources.Sheep),
                (byte)bundle.Get(CatanResources.Wheat),
                (byte)bundle.Get(CatanResources.Ore),
            };
        }

        private static ResourceBundle BundleFromBytes(byte w, byte b, byte s, byte wh, byte o)
        {
            var bundle = new ResourceBundle();
            if (w  > 0) bundle = bundle.Add(CatanResources.Wood,  w);
            if (b  > 0) bundle = bundle.Add(CatanResources.Brick, b);
            if (s  > 0) bundle = bundle.Add(CatanResources.Sheep, s);
            if (wh > 0) bundle = bundle.Add(CatanResources.Wheat, wh);
            if (o  > 0) bundle = bundle.Add(CatanResources.Ore,   o);
            return bundle;
        }
    }
}
