using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using GameCore.Events;
using GameCore.Resources;
using GameCore.Turn;
using UnityEngine;

namespace Catan.Tests
{
    public class CatanTurnManagerTests
    {
        private class TestResource : IResource
        {
            public string ResourceId { get; }
            public string DisplayName { get; }
            public TestResource(string resourceId) { ResourceId = resourceId; DisplayName = resourceId; }
        }

        private GameObject _gameObject;
        private CatanTurnManager _turnManager;
        private CatanBoard _board;
        private CatanPlayer _playerRed;
        private CatanPlayer _playerBlue;

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            CatanResources.Initialize(
                new TestResource("wood"),
                new TestResource("brick"),
                new TestResource("sheep"),
                new TestResource("wheat"),
                new TestResource("ore"));

            var generator = new CatanBoardGenerator(new System.Random(1));
            _board = generator.GenerateBoard();

            _playerRed = new CatanPlayer("red", "Red", Color.red);
            _playerBlue = new CatanPlayer("blue", "Blue", Color.blue);

            _gameObject = new GameObject("TurnManagerTest");
            _turnManager = _gameObject.AddComponent<CatanTurnManager>();
            _turnManager.Actors.Add(_playerRed);
            _turnManager.Actors.Add(_playerBlue);

            var allPlayers = new List<GameCore.Player.IPlayer> { _playerRed, _playerBlue };
            var robberSystem = new RobberSystem(new System.Random(0));
            robberSystem.Initialize(_board, allPlayers);
            _turnManager.Initialize(_board, new DiceManager(CreateFixedRoller(3, 4)), robberSystem);
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
            Object.DestroyImmediate(_gameObject);
        }

        // ── HandleDiceRoll ─────────────────────────────────────────────────────

        [Test]
        public void HandleDiceRoll_NonSeven_TransitionsToTradingPhase()
        {
            _turnManager.HandleDiceRoll(6);
            Assert.AreEqual(CatanTurnPhase.Trading, _turnManager.CurrentCatanPhase);
        }

        [Test]
        public void HandleDiceRoll_Seven_TransitionsToRobberPhase()
        {
            _turnManager.HandleDiceRoll(7);
            Assert.AreEqual(CatanTurnPhase.Robber, _turnManager.CurrentCatanPhase);
        }

        [Test]
        public void HandleDiceRoll_NonSeven_ProducesResources()
        {
            var producingTile = _board.Grid.Tiles.Values
                .FirstOrDefault(tile => tile.DiceNumber != 0 && tile.ResourceType.HasValue);
            if (producingTile == null) return;

            var vertex = _board.Grid.GetVertices(producingTile.Coord)[0];
            var settlement = new Settlement(_playerRed, vertex);
            _board.Settlements[vertex] = settlement;
            _playerRed.Settlements.Add(settlement);

            _turnManager.HandleDiceRoll(producingTile.DiceNumber);

            Assert.Greater(CountCards(_playerRed), 0);
        }

        [Test]
        public void HandleDiceRoll_Seven_ActivatesRobberSystem()
        {
            _turnManager.HandleDiceRoll(7);
            Assert.IsTrue(_turnManager.RobberSystem.MustMove);
        }

        [Test]
        public void HandleDiceRoll_Seven_PublishesSevenRolledEvent()
        {
            bool sevenRolledFired = false;
            EventBus.Subscribe<SevenRolledEvent>(_ => sevenRolledFired = true);

            _turnManager.HandleDiceRoll(7);

            Assert.IsTrue(sevenRolledFired);
        }

        [Test]
        public void HandleDiceRoll_NonSeven_PublishesCatanPhaseChangedEvent()
        {
            CatanPhaseChangedEvent? receivedEvent = null;
            EventBus.Subscribe<CatanPhaseChangedEvent>(gameEvent => receivedEvent = gameEvent);

            _turnManager.HandleDiceRoll(6);

            Assert.IsNotNull(receivedEvent);
            Assert.AreEqual(CatanTurnPhase.Trading, receivedEvent!.Value.To);
        }

        // ── NextTurn ───────────────────────────────────────────────────────────

        [Test]
        public void StartGame_SetsFirstActorAsCurrent()
        {
            _turnManager.StartGame();
            Assert.AreEqual(_playerRed, _turnManager.CurrentActor);
        }

        [Test]
        public void StartGame_SetsTurnNumberToOne()
        {
            _turnManager.StartGame();
            Assert.AreEqual(1, _turnManager.TurnNumber);
        }

        [Test]
        public void StartGame_SetsPhaseToSetupPlacement()
        {
            _turnManager.StartGame();
            Assert.AreEqual(CatanTurnPhase.SetupPlacement, _turnManager.CurrentCatanPhase);
        }

        [Test]
        public void NextTurn_DuringSetup_AdvancesToNextPlayer()
        {
            _turnManager.StartGame();
            _turnManager.NextTurn();
            Assert.AreEqual(_playerBlue, _turnManager.CurrentActor);
        }

        [Test]
        public void NextTurn_IncrementsTurnNumber()
        {
            _turnManager.StartGame();
            _turnManager.NextTurn();
            Assert.AreEqual(2, _turnManager.TurnNumber);
        }

        [Test]
        public void NextTurn_PublishesTurnEndedEvent()
        {
            _turnManager.StartGame();

            TurnEndedEvent? receivedEvent = null;
            EventBus.Subscribe<TurnEndedEvent>(gameEvent => receivedEvent = gameEvent);

            _turnManager.NextTurn();

            Assert.IsNotNull(receivedEvent);
            Assert.AreEqual(_playerRed, receivedEvent!.Value.Actor);
        }

        [Test]
        public void NextTurn_PublishesTurnStartedEvent()
        {
            _turnManager.StartGame();

            TurnStartedEvent? receivedEvent = null;
            EventBus.Subscribe<TurnStartedEvent>(gameEvent => receivedEvent = gameEvent);

            _turnManager.NextTurn();

            Assert.IsNotNull(receivedEvent);
            Assert.AreEqual(_playerBlue, receivedEvent!.Value.Actor);
        }

        [Test]
        public void NextTurn_AfterSetupComplete_SetsPhasToRollDice()
        {
            _turnManager.StartGame(); // red: setup 1

            // Advance through all 4 setup turns (2 players × 2 rounds = 4 turns)
            for (int turnIndex = 0; turnIndex < 4; turnIndex++)
                _turnManager.NextTurn();

            Assert.AreEqual(CatanTurnPhase.RollDice, _turnManager.CurrentCatanPhase);
        }

        [Test]
        public void NextTurn_DuringRegularPlay_CyclesToNextActor()
        {
            _turnManager.StartGame();

            // Complete all setup turns
            for (int turnIndex = 0; turnIndex < 4; turnIndex++)
                _turnManager.NextTurn();

            // Now in regular play — first player is red, next should be blue
            _turnManager.NextTurn();
            Assert.AreEqual(_playerBlue, _turnManager.CurrentActor);
        }

        // ── AdvancePhase ───────────────────────────────────────────────────────

        [Test]
        public void AdvancePhase_FromTrading_TransitionsToBuilding()
        {
            _turnManager.HandleDiceRoll(6); // → Trading
            _turnManager.AdvancePhase();
            Assert.AreEqual(CatanTurnPhase.Building, _turnManager.CurrentCatanPhase);
        }

        [Test]
        public void AdvancePhase_FromRobber_TransitionsToBuilding()
        {
            _turnManager.HandleDiceRoll(7); // → Robber
            _turnManager.AdvancePhase();
            Assert.AreEqual(CatanTurnPhase.Building, _turnManager.CurrentCatanPhase);
        }

        [Test]
        public void AdvancePhase_FromBuilding_TransitionsToEndTurn()
        {
            _turnManager.HandleDiceRoll(6); // → Trading
            _turnManager.AdvancePhase();    // → Building
            _turnManager.AdvancePhase();    // → EndTurn
            Assert.AreEqual(CatanTurnPhase.EndTurn, _turnManager.CurrentCatanPhase);
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static int CountCards(CatanPlayer player)
        {
            int total = 0;
            foreach (var pair in player.Resources.Current.Amounts)
                total += pair.Value;
            return total;
        }

        private static System.Func<int, int, int> CreateFixedRoller(int firstDie, int secondDie)
        {
            int rollIndex = 0;
            int[] results = { firstDie, secondDie };
            return (min, max) => results[rollIndex++ % results.Length];
        }
    }
}
