using System.Collections.Generic;
using System.Linq;
using GameCore.Events;
using GameCore.Player;
using GameCore.Turn;

namespace Catan
{
    public class CatanTurnManager : TurnManager
    {
        public CatanTurnPhase CurrentCatanPhase { get; private set; }
        public DiceManager DiceManager { get; private set; }
        public RobberSystem RobberSystem { get; private set; }

        private CatanBoard _board;
        private bool _inSetupPhase;
        private Queue<ITurnActor> _setupQueue;

        // Call before StartGame. Optional overrides allow injecting test doubles.
        // An injected RobberSystem must already be initialized with the board and player list.
        public void Initialize(CatanBoard board, DiceManager diceManager = null, RobberSystem robberSystem = null)
        {
            _board = board;
            DiceManager = diceManager ?? new DiceManager();

            if (robberSystem != null)
            {
                RobberSystem = robberSystem;
            }
            else
            {
                var playerList = Actors.OfType<IPlayer>().ToList();
                RobberSystem = new RobberSystem();
                RobberSystem.Initialize(board, playerList);
            }

            EventBus.Subscribe<DiceRolledEvent>(OnDiceRolledEvent);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<DiceRolledEvent>(OnDiceRolledEvent);
        }

        // Begins the game: setup phase in forward then reverse player order.
        public void StartGame()
        {
            _inSetupPhase = true;
            _setupQueue = new Queue<ITurnActor>();

            foreach (var actor in Actors)
                _setupQueue.Enqueue(actor);

            var reversedActors = new List<ITurnActor>(Actors);
            reversedActors.Reverse();
            foreach (var actor in reversedActors)
                _setupQueue.Enqueue(actor);

            TurnNumber = 1;
            CurrentActor = _setupQueue.Count > 0 ? _setupQueue.Dequeue() : null;
            SetCatanPhase(CatanTurnPhase.SetupPlacement);

            if (CurrentActor != null)
                EventBus.Publish(new TurnStartedEvent { Actor = CurrentActor, TurnNumber = TurnNumber });
        }

        // Triggers a dice roll. Only valid during the RollDice phase.
        public void RequestRoll()
        {
            if (CurrentCatanPhase != CatanTurnPhase.RollDice) return;
            DiceManager.Roll();
        }

        // Processes the dice result. Called automatically via DiceRolledEvent subscription,
        // but also available directly for testing.
        public void HandleDiceRoll(int total)
        {
            if (total == 7)
            {
                SetCatanPhase(CatanTurnPhase.Robber);
                RobberSystem.Activate(CurrentActor as IPlayer);
            }
            else
            {
                _board.ProduceResourcesForNumber(total);
                SetCatanPhase(CatanTurnPhase.Trading);
            }
        }

        public override void NextTurn()
        {
            if (CurrentActor != null)
                EventBus.Publish(new TurnEndedEvent { Actor = CurrentActor });

            if (_inSetupPhase)
            {
                if (_setupQueue != null && _setupQueue.Count > 0)
                {
                    CurrentActor = _setupQueue.Dequeue();
                    TurnNumber++;
                    SetCatanPhase(CatanTurnPhase.SetupPlacement);
                }
                else
                {
                    _inSetupPhase = false;
                    CurrentActor = Actors.Count > 0 ? Actors[0] : null;
                    TurnNumber++;
                    SetCatanPhase(CatanTurnPhase.RollDice);
                }
            }
            else
            {
                int currentIndex = CurrentActor != null ? Actors.IndexOf(CurrentActor) : -1;
                int nextIndex = Actors.Count > 0 ? (currentIndex + 1) % Actors.Count : 0;
                CurrentActor = Actors.Count > 0 ? Actors[nextIndex] : null;
                TurnNumber++;
                SetCatanPhase(CatanTurnPhase.RollDice);
            }

            if (CurrentActor != null)
                EventBus.Publish(new TurnStartedEvent { Actor = CurrentActor, TurnNumber = TurnNumber });
        }

        // Advances to the next logical phase in the Catan turn sequence.
        public override void AdvancePhase()
        {
            CatanTurnPhase nextPhase = CurrentCatanPhase switch
            {
                CatanTurnPhase.RollDice => CatanTurnPhase.Trading,
                CatanTurnPhase.Robber => CatanTurnPhase.Building,
                CatanTurnPhase.Trading => CatanTurnPhase.Building,
                CatanTurnPhase.Building => CatanTurnPhase.EndTurn,
                CatanTurnPhase.EndTurn => CatanTurnPhase.RollDice,
                _ => CatanTurnPhase.RollDice
            };
            SetCatanPhase(nextPhase);
        }

        // Override to suppress the base TurnPhase cycle — Catan uses CatanTurnPhase.
        public override void SkipActor(ITurnActor actor, string reason)
        {
            base.SkipActor(actor, reason);
        }

        // Hook called every time the Catan phase changes.
        protected virtual void OnPhaseStart(CatanTurnPhase phase) { }

        // ── Private helpers ────────────────────────────────────────────────────

        private void SetCatanPhase(CatanTurnPhase newPhase)
        {
            var previousPhase = CurrentCatanPhase;
            CurrentCatanPhase = newPhase;
            EventBus.Publish(new CatanPhaseChangedEvent { From = previousPhase, To = newPhase });
            OnPhaseStart(newPhase);
        }

        private void OnDiceRolledEvent(DiceRolledEvent gameEvent)
        {
            HandleDiceRoll(gameEvent.Total);
        }
    }
}
