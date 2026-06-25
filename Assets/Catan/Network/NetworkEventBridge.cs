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

        public override void OnNetworkSpawn()
        {
            Instance = this;

            if (IsServer)
            {
                SubscribeToHostEvents();
                if (GameManager.Instance != null)
                    BroadcastSeedClientRpc(GameManager.Instance.BoardSeed);
            }
        }

        public override void OnNetworkDespawn()
        {
            if (Instance == this) Instance = null;
            if (IsServer) UnsubscribeFromHostEvents();
        }

        // ── Seed sync ──────────────────────────────────────────────────────────

        [ClientRpc]
        private void BroadcastSeedClientRpc(int seed)
        {
            if (IsServer) return; // host already completed init at scene start
            var manager = GameManager.Instance;
            if (manager == null)
            {
                Debug.LogWarning("[NetworkEventBridge] Seed arrived before GameManager existed.");
                return;
            }
            // Match host's default 2-player roster until the lobby learns to send
            // a real player list (Phase 5d). Resources are already initialized in
            // GameManager.Start before the defer-on-client branch.
            GameSession.SetDefault2PlayerHotseat();
            manager.CompleteInitialization(seed);
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
