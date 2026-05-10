using System.Collections.Generic;
using NUnit.Framework;
using GameCore.Events;
using GameCore.Resources;
using UnityEngine;

namespace Catan.Tests
{
    public class DevCardTests
    {
        private class TestResource : IResource
        {
            public string ResourceId { get; }
            public string DisplayName { get; }
            public TestResource(string resourceId) { ResourceId = resourceId; DisplayName = resourceId; }
        }

        private CatanBoard _board;
        private CatanPlayer _activePlayer;
        private CatanPlayer _otherPlayer;
        private GameObject _gameObject;
        private CatanTurnManager _turnManager;
        private CatanGameContext _context;

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            CatanResources.Initialize(
                new TestResource("wood"), new TestResource("brick"),
                new TestResource("sheep"), new TestResource("wheat"),
                new TestResource("ore"));

            var generator = new CatanBoardGenerator(new System.Random(1));
            _board = generator.GenerateBoard();

            _activePlayer = new CatanPlayer("active", "Active", Color.red);
            _otherPlayer = new CatanPlayer("other", "Other", Color.blue);

            _gameObject = new GameObject("TurnManagerTest");
            _turnManager = _gameObject.AddComponent<CatanTurnManager>();
            _turnManager.Actors.Add(_activePlayer);
            _turnManager.Actors.Add(_otherPlayer);

            var allPlayers = new List<GameCore.Player.IPlayer> { _activePlayer, _otherPlayer };
            var robberSystem = new RobberSystem(new System.Random(0));
            robberSystem.Initialize(_board, allPlayers);
            _turnManager.Initialize(_board, new DiceManager(CreateFixedRoller(3, 4)), robberSystem);

            _context = new CatanGameContext(_activePlayer, _turnManager, _board, allPlayers);
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
            Object.DestroyImmediate(_gameObject);
        }

        // ── IsPlayable: same-turn restriction ─────────────────────────────────

        [Test]
        public void KnightCard_IsPlayable_SameTurnPurchased_ReturnsFalse()
        {
            var card = new KnightCard { TurnPurchased = _turnManager.TurnNumber };
            _turnManager.HandleDiceRoll(6); // puts us in Trading phase
            Assert.IsFalse(card.IsPlayable(_context));
        }

        [Test]
        public void KnightCard_IsPlayable_DifferentTurn_TradingPhase_ReturnsTrue()
        {
            var card = new KnightCard { TurnPurchased = _turnManager.TurnNumber - 1 };
            _turnManager.HandleDiceRoll(6); // → Trading
            Assert.IsTrue(card.IsPlayable(_context));
        }

        [Test]
        public void KnightCard_IsPlayable_RollDicePhase_ReturnsTrue()
        {
            var card = new KnightCard { TurnPurchased = _turnManager.TurnNumber - 1 };
            // Default phase is RollDice (no HandleDiceRoll called)
            Assert.IsTrue(card.IsPlayable(_context));
        }

        [Test]
        public void MonopolyCard_IsPlayable_SetupPhase_ReturnsFalse()
        {
            var card = new MonopolyCard { TurnPurchased = _turnManager.TurnNumber - 1 };
            _turnManager.StartGame(); // → SetupPlacement
            Assert.IsFalse(card.IsPlayable(_context));
        }

        [Test]
        public void RoadBuildingCard_IsPlayable_TradingPhase_ReturnsTrue()
        {
            var card = new RoadBuildingCard { TurnPurchased = _turnManager.TurnNumber - 1 };
            _turnManager.HandleDiceRoll(6); // → Trading
            Assert.IsTrue(card.IsPlayable(_context));
        }

        // ── KnightCard.OnPlay ──────────────────────────────────────────────────

        [Test]
        public void KnightCard_OnPlay_IncrementsKnightsPlayed()
        {
            var card = new KnightCard();
            card.OnPlay(_context);
            Assert.AreEqual(1, _activePlayer.KnightsPlayed);
        }

        [Test]
        public void KnightCard_OnPlay_ActivatesRobberSystem()
        {
            var card = new KnightCard();
            card.OnPlay(_context);
            Assert.IsTrue(_turnManager.RobberSystem.MustMove);
        }

        [Test]
        public void KnightCard_OnPlay_PublishesKnightPlayedEvent()
        {
            bool eventFired = false;
            EventBus.Subscribe<KnightPlayedEvent>(_ => eventFired = true);

            var card = new KnightCard();
            card.OnPlay(_context);

            Assert.IsTrue(eventFired);
        }

        // ── MonopolyCard.OnPlay ────────────────────────────────────────────────

        [Test]
        public void MonopolyCard_OnPlay_TransfersAllChosenResourceFromOtherPlayer()
        {
            _otherPlayer.Resources.TryAdd(new ResourceBundle().Add(CatanResources.Wood, 5));

            var card = new MonopolyCard { ChosenResourceType = CatanResourceType.Wood };
            card.OnPlay(_context);

            Assert.AreEqual(5, _activePlayer.Resources.Current.Get(CatanResources.Wood));
            Assert.AreEqual(0, _otherPlayer.Resources.Current.Get(CatanResources.Wood));
        }

        [Test]
        public void MonopolyCard_OnPlay_DoesNotAffectActivePlayer()
        {
            _activePlayer.Resources.TryAdd(new ResourceBundle().Add(CatanResources.Wood, 3));
            _otherPlayer.Resources.TryAdd(new ResourceBundle().Add(CatanResources.Wood, 2));

            var card = new MonopolyCard { ChosenResourceType = CatanResourceType.Wood };
            card.OnPlay(_context);

            // Active player should have their 3 + the 2 stolen = 5
            Assert.AreEqual(5, _activePlayer.Resources.Current.Get(CatanResources.Wood));
        }

        [Test]
        public void MonopolyCard_OnPlay_PublishesMonopolyPlayedEvent()
        {
            bool eventFired = false;
            EventBus.Subscribe<MonopolyPlayedEvent>(_ => eventFired = true);

            var card = new MonopolyCard { ChosenResourceType = CatanResourceType.Brick };
            card.OnPlay(_context);

            Assert.IsTrue(eventFired);
        }

        // ── YearOfPlentyCard.OnPlay ────────────────────────────────────────────

        [Test]
        public void YearOfPlentyCard_OnPlay_GrantsTwoChosenResources()
        {
            var card = new YearOfPlentyCard
            {
                FirstChosenResource = CatanResourceType.Wood,
                SecondChosenResource = CatanResourceType.Ore
            };
            card.OnPlay(_context);

            Assert.AreEqual(1, _activePlayer.Resources.Current.Get(CatanResources.Wood));
            Assert.AreEqual(1, _activePlayer.Resources.Current.Get(CatanResources.Ore));
        }

        [Test]
        public void YearOfPlentyCard_OnPlay_CanGrantSameResourceTwice()
        {
            var card = new YearOfPlentyCard
            {
                FirstChosenResource = CatanResourceType.Wood,
                SecondChosenResource = CatanResourceType.Wood
            };
            card.OnPlay(_context);

            Assert.AreEqual(2, _activePlayer.Resources.Current.Get(CatanResources.Wood));
        }

        [Test]
        public void YearOfPlentyCard_OnPlay_PublishesYearOfPlentyPlayedEvent()
        {
            bool eventFired = false;
            EventBus.Subscribe<YearOfPlentyPlayedEvent>(_ => eventFired = true);

            var card = new YearOfPlentyCard
            {
                FirstChosenResource = CatanResourceType.Wheat,
                SecondChosenResource = CatanResourceType.Sheep
            };
            card.OnPlay(_context);

            Assert.IsTrue(eventFired);
        }

        // ── RoadBuildingCard.OnPlay ────────────────────────────────────────────

        [Test]
        public void RoadBuildingCard_OnPlay_PublishesFreeRoadsGrantedEvent()
        {
            FreeRoadsGrantedEvent? receivedEvent = null;
            EventBus.Subscribe<FreeRoadsGrantedEvent>(gameEvent => receivedEvent = gameEvent);

            var card = new RoadBuildingCard();
            card.OnPlay(_context);

            Assert.IsNotNull(receivedEvent);
            Assert.AreEqual(_activePlayer, receivedEvent!.Value.Player);
            Assert.AreEqual(2, receivedEvent!.Value.Count);
        }

        // ── VictoryPointCard ───────────────────────────────────────────────────

        [Test]
        public void VictoryPointCard_IsPlayable_AlwaysReturnsFalse()
        {
            var card = new VictoryPointCard { TurnPurchased = 0 };
            _turnManager.HandleDiceRoll(6);
            Assert.IsFalse(card.IsPlayable(_context));
        }

        // ── Helper ─────────────────────────────────────────────────────────────

        private static System.Func<int, int, int> CreateFixedRoller(int firstDie, int secondDie)
        {
            int rollIndex = 0;
            int[] results = { firstDie, secondDie };
            return (min, max) => results[rollIndex++ % results.Length];
        }
    }
}
