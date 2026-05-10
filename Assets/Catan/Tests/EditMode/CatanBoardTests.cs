using System.Linq;
using NUnit.Framework;
using GameCore.Events;
using GameCore.Resources;
using UnityEngine;

namespace Catan.Tests
{
    public class CatanBoardTests
    {
        private class TestResource : IResource
        {
            public string ResourceId { get; }
            public string DisplayName { get; }
            public TestResource(string id) { ResourceId = id; DisplayName = id; }
        }

        private class TestPlayer : GameCore.Player.IPlayer, GameCore.Turn.ITurnActor
        {
            public string Id { get; }
            public string DisplayName => Id;
            public Color Color => Color.white;
            public bool CanAct { get; set; }
            public ResourceInventory Resources { get; }
            public System.Collections.Generic.List<Settlement> Settlements { get; } = new();
            public System.Collections.Generic.List<Road> Roads { get; } = new();
            public int KnightsPlayed { get; internal set; }
            public bool HasLargestArmy { get; internal set; }
            public bool HasLongestRoad { get; internal set; }
            public GameCore.Cards.CardHand<DevelopmentCard> DevelopmentCards { get; } = new();
            public TestPlayer(string playerId)
            {
                Id = playerId;
                Resources = new ResourceInventory(this);
            }
        }

        private CatanBoard _board;
        private TestPlayer _playerRed;
        private TestPlayer _playerBlue;

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
            _playerRed = new TestPlayer("red");
            _playerBlue = new TestPlayer("blue");
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
        }

        // ── MoveRobber ─────────────────────────────────────────────────────────

        [Test]
        public void MoveRobber_UpdatesRobberPosition()
        {
            var targetCoord = new GameCore.Board.HexCoord(1, 0);
            _board.MoveRobber(targetCoord);
            Assert.AreEqual(targetCoord, _board.RobberPosition);
        }

        [Test]
        public void MoveRobber_NewTileHasRobber()
        {
            var targetCoord = new GameCore.Board.HexCoord(1, 0);
            _board.MoveRobber(targetCoord);
            Assert.IsTrue(_board.Grid.GetTile(targetCoord).HasRobber);
        }

        [Test]
        public void MoveRobber_PreviousTileNoLongerHasRobber()
        {
            var firstCoord = new GameCore.Board.HexCoord(1, 0);
            var secondCoord = new GameCore.Board.HexCoord(-1, 0);
            _board.MoveRobber(firstCoord);
            _board.MoveRobber(secondCoord);
            Assert.IsFalse(_board.Grid.GetTile(firstCoord).HasRobber);
        }

        // ── GetTilesForNumber ──────────────────────────────────────────────────

        [Test]
        public void GetTilesForNumber_ReturnsTilesWithMatchingDiceNumber()
        {
            var tilesForFive = _board.GetTilesForNumber(5);
            Assert.IsTrue(tilesForFive.All(tile => tile.DiceNumber == 5));
        }

        [Test]
        public void GetTilesForNumber_ExcludesRobberBlockedTile()
        {
            var tileWithFive = _board.Grid.Tiles.Values.FirstOrDefault(tile => tile.DiceNumber == 5);
            if (tileWithFive == null) return; // skip if board seed has no 5

            _board.MoveRobber(tileWithFive.Coord);
            var tilesForFive = _board.GetTilesForNumber(5);

            Assert.IsFalse(tilesForFive.Contains(tileWithFive));
        }

        [Test]
        public void GetTilesForNumber_DesertNeverProduces()
        {
            Assert.IsEmpty(_board.GetTilesForNumber(0));
        }

        // ── GetPlayersOnTile ───────────────────────────────────────────────────

        [Test]
        public void GetPlayersOnTile_NoSettlements_ReturnsEmpty()
        {
            var coord = new GameCore.Board.HexCoord(0, 0);
            var players = _board.GetPlayersOnTile(coord);
            Assert.IsEmpty(players);
        }

        [Test]
        public void GetPlayersOnTile_SettlementOnAdjacentVertex_ReturnsOwner()
        {
            var coord = new GameCore.Board.HexCoord(0, 0);
            var vertex = _board.Grid.GetVertices(coord)[0];
            var settlement = new Settlement(_playerRed, vertex);
            _board.Settlements[vertex] = settlement;
            _playerRed.Settlements.Add(settlement);

            var playersOnTile = _board.GetPlayersOnTile(coord);

            Assert.Contains(_playerRed, (System.Collections.ICollection)playersOnTile);
        }

        [Test]
        public void GetPlayersOnTile_TwoSettlementsOnSameTile_ReturnsBothOwners()
        {
            var coord = new GameCore.Board.HexCoord(0, 0);
            var vertices = _board.Grid.GetVertices(coord);

            var settlementRed = new Settlement(_playerRed, vertices[0]);
            var settlementBlue = new Settlement(_playerBlue, vertices[1]);
            _board.Settlements[vertices[0]] = settlementRed;
            _board.Settlements[vertices[1]] = settlementBlue;

            var playersOnTile = _board.GetPlayersOnTile(coord);

            Assert.AreEqual(2, playersOnTile.Count);
        }

        [Test]
        public void GetPlayersOnTile_PlayerHasTwoSettlementsOnSameTile_ReturnedOnce()
        {
            var coord = new GameCore.Board.HexCoord(0, 0);
            var vertices = _board.Grid.GetVertices(coord);

            _board.Settlements[vertices[0]] = new Settlement(_playerRed, vertices[0]);
            _board.Settlements[vertices[1]] = new Settlement(_playerRed, vertices[1]);

            var playersOnTile = _board.GetPlayersOnTile(coord);

            Assert.AreEqual(1, playersOnTile.Count);
        }

        // ── ProduceResourcesForNumber ──────────────────────────────────────────

        [Test]
        public void ProduceResourcesForNumber_SettlementOnMatchingTile_PlayerReceivesResource()
        {
            // Find a non-desert tile with a specific number.
            var producingTile = _board.Grid.Tiles.Values
                .FirstOrDefault(tile => tile.DiceNumber == 6 || tile.DiceNumber == 9);
            if (producingTile == null) return;

            var vertex = _board.Grid.GetVertices(producingTile.Coord)[0];
            var settlement = new Settlement(_playerRed, vertex);
            _board.Settlements[vertex] = settlement;
            _playerRed.Settlements.Add(settlement);

            _board.ProduceResourcesForNumber(producingTile.DiceNumber);

            Assert.AreEqual(1, _playerRed.Resources.Current.Get(producingTile.Resource));
        }

        [Test]
        public void ProduceResourcesForNumber_CityOnMatchingTile_PlayerReceivesTwoResources()
        {
            var producingTile = _board.Grid.Tiles.Values
                .FirstOrDefault(tile => tile.DiceNumber == 6 || tile.DiceNumber == 9);
            if (producingTile == null) return;

            var vertex = _board.Grid.GetVertices(producingTile.Coord)[0];
            var settlement = new Settlement(_playerRed, vertex);
            settlement.UpgradeToCity();
            _board.Settlements[vertex] = settlement;
            _playerRed.Settlements.Add(settlement);

            _board.ProduceResourcesForNumber(producingTile.DiceNumber);

            Assert.AreEqual(2, _playerRed.Resources.Current.Get(producingTile.Resource));
        }

        [Test]
        public void ProduceResourcesForNumber_RobberOnTile_NothingProduced()
        {
            var producingTile = _board.Grid.Tiles.Values
                .FirstOrDefault(tile => tile.DiceNumber == 6);
            if (producingTile == null) return;

            var vertex = _board.Grid.GetVertices(producingTile.Coord)[0];
            _board.Settlements[vertex] = new Settlement(_playerRed, vertex);
            _board.MoveRobber(producingTile.Coord);

            _board.ProduceResourcesForNumber(6);

            Assert.AreEqual(0, _playerRed.Resources.Current.Get(producingTile.Resource));
        }

        [Test]
        public void ProduceResourcesForNumber_PublishesResourceProducedEvent()
        {
            var producingTile = _board.Grid.Tiles.Values
                .FirstOrDefault(tile => tile.DiceNumber == 5);
            if (producingTile == null) return;

            ResourceProducedEvent? receivedEvent = null;
            EventBus.Subscribe<ResourceProducedEvent>(gameEvent => receivedEvent = gameEvent);

            var vertex = _board.Grid.GetVertices(producingTile.Coord)[0];
            _board.Settlements[vertex] = new Settlement(_playerRed, vertex);
            _board.ProduceResourcesForNumber(5);

            Assert.IsNotNull(receivedEvent);
        }
    }
}
