using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using GameCore.Resources;

namespace Catan.Tests
{
    public class CatanBoardGeneratorTests
    {
        private class TestResource : IResource
        {
            public string ResourceId { get; }
            public string DisplayName { get; }
            public TestResource(string resourceId) { ResourceId = resourceId; DisplayName = resourceId; }
        }

        private CatanBoardGenerator _generator;

        [SetUp]
        public void SetUp()
        {
            CatanResources.Initialize(
                new TestResource("wood"),
                new TestResource("brick"),
                new TestResource("sheep"),
                new TestResource("wheat"),
                new TestResource("ore"));

            _generator = new CatanBoardGenerator(new System.Random(42));
        }

        // ── Tile count and distribution ────────────────────────────────────────

        [Test]
        public void GenerateBoard_Creates19Tiles()
        {
            var board = _generator.GenerateBoard();
            Assert.AreEqual(19, board.Grid.Tiles.Count);
        }

        [Test]
        public void GenerateBoard_TileTypeDistribution_IsCorrect()
        {
            var board = _generator.GenerateBoard();
            var allTiles = board.Grid.Tiles.Values.ToList();

            Assert.AreEqual(4, allTiles.Count(tile => tile.ResourceType == CatanResourceType.Wood));
            Assert.AreEqual(4, allTiles.Count(tile => tile.ResourceType == CatanResourceType.Sheep));
            Assert.AreEqual(4, allTiles.Count(tile => tile.ResourceType == CatanResourceType.Wheat));
            Assert.AreEqual(3, allTiles.Count(tile => tile.ResourceType == CatanResourceType.Ore));
            Assert.AreEqual(3, allTiles.Count(tile => tile.ResourceType == CatanResourceType.Brick));
            Assert.AreEqual(1, allTiles.Count(tile => tile.ResourceType == null));
        }

        [Test]
        public void GenerateBoard_DesertTile_HasNoDiceNumber()
        {
            var board = _generator.GenerateBoard();
            var desertTile = board.Grid.Tiles.Values.Single(tile => tile.ResourceType == null);
            Assert.AreEqual(0, desertTile.DiceNumber);
        }

        [Test]
        public void GenerateBoard_NonDesertTiles_AllHaveDiceNumbers()
        {
            var board = _generator.GenerateBoard();
            var nonDesertTiles = board.Grid.Tiles.Values.Where(tile => tile.ResourceType.HasValue);
            foreach (var tile in nonDesertTiles)
                Assert.AreNotEqual(0, tile.DiceNumber, $"Non-desert tile at {tile.Coord} has no dice number.");
        }

        [Test]
        public void GenerateBoard_NoTileHasDiceNumberSeven()
        {
            var board = _generator.GenerateBoard();
            Assert.IsFalse(board.Grid.Tiles.Values.Any(tile => tile.DiceNumber == 7));
        }

        [Test]
        public void GenerateBoard_DiceNumberDistribution_IsCorrect()
        {
            var board = _generator.GenerateBoard();
            var allNumbers = board.Grid.Tiles.Values
                .Where(tile => tile.DiceNumber != 0)
                .Select(tile => tile.DiceNumber)
                .OrderBy(number => number)
                .ToList();

            var expectedNumbers = new[] { 2, 3, 3, 4, 4, 5, 5, 6, 6, 8, 8, 9, 9, 10, 10, 11, 11, 12 };
            CollectionAssert.AreEquivalent(expectedNumbers, allNumbers);
        }

        // ── 6/8 adjacency constraint ───────────────────────────────────────────

        [Test]
        public void GenerateBoard_NoTwoAdjacentTiles_BothHaveSixOrEight()
        {
            // Run multiple times because generation is random.
            for (int runIndex = 0; runIndex < 20; runIndex++)
            {
                var generator = new CatanBoardGenerator(new System.Random(runIndex));
                var board = generator.GenerateBoard();

                var highProbabilityTiles = board.Grid.Tiles.Values
                    .Where(tile => tile.DiceNumber == 6 || tile.DiceNumber == 8)
                    .ToList();

                foreach (var firstTile in highProbabilityTiles)
                {
                    foreach (var secondTile in highProbabilityTiles)
                    {
                        if (firstTile == secondTile) continue;
                        Assert.AreNotEqual(1, firstTile.Coord.Distance(secondTile.Coord),
                            $"Tiles at {firstTile.Coord} (#{firstTile.DiceNumber}) and " +
                            $"{secondTile.Coord} (#{secondTile.DiceNumber}) are adjacent — 6/8 constraint violated.");
                    }
                }
            }
        }

        // ── Ports ──────────────────────────────────────────────────────────────

        [Test]
        public void GenerateBoard_Creates9Ports()
        {
            var board = _generator.GenerateBoard();
            Assert.AreEqual(9, board.Ports.Ports.Count);
        }

        [Test]
        public void GenerateBoard_PortDistribution_FiveSpecificFourGeneric()
        {
            var board = _generator.GenerateBoard();
            var specificPortCount = board.Ports.Ports.Count(port => port.SpecificResource.HasValue);
            var genericPortCount = board.Ports.Ports.Count(port => !port.SpecificResource.HasValue);

            Assert.AreEqual(5, specificPortCount);
            Assert.AreEqual(4, genericPortCount);
        }

        [Test]
        public void GenerateBoard_SpecificPorts_OnePerResourceType()
        {
            var board = _generator.GenerateBoard();
            var specificPorts = board.Ports.Ports.Where(port => port.SpecificResource.HasValue).ToList();

            Assert.AreEqual(1, specificPorts.Count(port => port.SpecificResource == CatanResourceType.Wood));
            Assert.AreEqual(1, specificPorts.Count(port => port.SpecificResource == CatanResourceType.Brick));
            Assert.AreEqual(1, specificPorts.Count(port => port.SpecificResource == CatanResourceType.Sheep));
            Assert.AreEqual(1, specificPorts.Count(port => port.SpecificResource == CatanResourceType.Wheat));
            Assert.AreEqual(1, specificPorts.Count(port => port.SpecificResource == CatanResourceType.Ore));
        }

        [Test]
        public void GenerateBoard_SpecificPorts_HaveTradeRatioTwo()
        {
            var board = _generator.GenerateBoard();
            foreach (var port in board.Ports.Ports.Where(port => port.SpecificResource.HasValue))
                Assert.AreEqual(2, port.TradeRatio);
        }

        [Test]
        public void GenerateBoard_GenericPorts_HaveTradeRatioThree()
        {
            var board = _generator.GenerateBoard();
            foreach (var port in board.Ports.Ports.Where(port => !port.SpecificResource.HasValue))
                Assert.AreEqual(3, port.TradeRatio);
        }

        [Test]
        public void GenerateBoard_EachPort_HasTwoAccessVertices()
        {
            var board = _generator.GenerateBoard();
            foreach (var port in board.Ports.Ports)
                Assert.AreEqual(2, port.AccessVertices.Length,
                    $"Port for {port.SpecificResource} has wrong number of access vertices.");
        }

        // ── Robber ─────────────────────────────────────────────────────────────

        [Test]
        public void GenerateBoard_RobberStartsOnDesertTile()
        {
            var board = _generator.GenerateBoard();
            var desertTile = board.Grid.Tiles.Values.Single(tile => tile.ResourceType == null);

            Assert.AreEqual(desertTile.Coord, board.RobberPosition);
            Assert.IsTrue(desertTile.HasRobber);
        }

        // ── Reproducibility ────────────────────────────────────────────────────

        [Test]
        public void GenerateBoard_SameSeed_ProducesSameLayout()
        {
            var firstBoard = new CatanBoardGenerator(new System.Random(999)).GenerateBoard();
            var secondBoard = new CatanBoardGenerator(new System.Random(999)).GenerateBoard();

            var firstTileNumbers = firstBoard.Grid.Tiles
                .OrderBy(pair => pair.Key.Q).ThenBy(pair => pair.Key.R)
                .Select(pair => pair.Value.DiceNumber);
            var secondTileNumbers = secondBoard.Grid.Tiles
                .OrderBy(pair => pair.Key.Q).ThenBy(pair => pair.Key.R)
                .Select(pair => pair.Value.DiceNumber);

            CollectionAssert.AreEqual(firstTileNumbers, secondTileNumbers);
        }
    }
}
