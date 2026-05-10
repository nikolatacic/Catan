using NUnit.Framework;
using UnityEngine;

namespace GameCore.Board.Tests
{
    public class HexCoordTests
    {
        private const float HexSize = 1f;
        private const float Tolerance = 0.001f;

        // ── Neighbors ──────────────────────────────────────────────────────────

        [Test]
        public void Neighbors_ReturnsExactlySixCoords()
        {
            var center = new HexCoord(0, 0);
            Assert.AreEqual(6, center.Neighbors().Length);
        }

        [Test]
        public void Neighbors_EachNeighborIsDistanceOneFromCenter()
        {
            var center = new HexCoord(2, -1);
            foreach (var neighbor in center.Neighbors())
                Assert.AreEqual(1, center.Distance(neighbor),
                    $"Neighbor {neighbor} should be distance 1 from {center}");
        }

        [Test]
        public void Neighbors_OriginNeighbors_MatchKnownDirections()
        {
            var origin = new HexCoord(0, 0);
            var neighbors = origin.Neighbors();

            Assert.Contains(new HexCoord(1,  0), neighbors);
            Assert.Contains(new HexCoord(1, -1), neighbors);
            Assert.Contains(new HexCoord(0, -1), neighbors);
            Assert.Contains(new HexCoord(-1, 0), neighbors);
            Assert.Contains(new HexCoord(-1, 1), neighbors);
            Assert.Contains(new HexCoord(0,  1), neighbors);
        }

        [Test]
        public void Neighbors_OffsetCoord_AllAtDistanceOne()
        {
            var coord = new HexCoord(3, -2);
            foreach (var neighbor in coord.Neighbors())
                Assert.AreEqual(1, coord.Distance(neighbor));
        }

        // ── Distance ───────────────────────────────────────────────────────────

        [Test]
        public void Distance_SameCoord_IsZero()
        {
            var coord = new HexCoord(4, -3);
            Assert.AreEqual(0, coord.Distance(coord));
        }

        [Test]
        public void Distance_IsSymmetric()
        {
            var coordA = new HexCoord(0, 0);
            var coordB = new HexCoord(3, -2);
            Assert.AreEqual(coordA.Distance(coordB), coordB.Distance(coordA));
        }

        [Test]
        [TestCase(0, 0, 1, 0, 1)]
        [TestCase(0, 0, 2, 0, 2)]
        [TestCase(0, 0, -2, 2, 2)]
        [TestCase(1, -1, 3, -3, 2)]
        [TestCase(0, 0, 2, -2, 2)]
        public void Distance_KnownPairs_ReturnsExpectedDistance(
            int fromQ, int fromR, int toQ, int toR, int expectedDistance)
        {
            var from = new HexCoord(fromQ, fromR);
            var to = new HexCoord(toQ, toR);
            Assert.AreEqual(expectedDistance, from.Distance(to));
        }

        // ── World position round-trip ───────────────────────────────────────────

        [Test]
        public void ToWorldPosition_Origin_ReturnsZeroVector()
        {
            var origin = new HexCoord(0, 0);
            var worldPosition = origin.ToWorldPosition(HexSize);
            Assert.AreEqual(Vector3.zero, worldPosition);
        }

        [Test]
        public void FromWorldPosition_ThenToWorldPosition_RoundTripsCleanly(
            [Values(0, 1, -1, 2, -2)] int coordQ,
            [Values(0, 1, -1)] int coordR)
        {
            var original = new HexCoord(coordQ, coordR);
            var worldPosition = original.ToWorldPosition(HexSize);
            var recovered = HexCoord.FromWorldPosition(worldPosition, HexSize);

            Assert.AreEqual(original, recovered,
                $"Round-trip failed for ({coordQ},{coordR}): got ({recovered.Q},{recovered.R})");
        }

        [Test]
        public void FromWorldPosition_PointInsideTile_SnapsToCorrectCoord()
        {
            var expectedCoord = new HexCoord(1, 0);
            var centerWorldPosition = expectedCoord.ToWorldPosition(HexSize);
            var slightlyOffCenter = centerWorldPosition + new Vector3(0.1f, 0, 0.1f);

            var snappedCoord = HexCoord.FromWorldPosition(slightlyOffCenter, HexSize);

            Assert.AreEqual(expectedCoord, snappedCoord);
        }

        [Test]
        public void ToWorldPosition_AdjacentHexes_AreCorrectDistanceApart()
        {
            var hexA = new HexCoord(0, 0);
            var hexB = new HexCoord(1, 0);
            var worldA = hexA.ToWorldPosition(HexSize);
            var worldB = hexB.ToWorldPosition(HexSize);
            var actualDistance = Vector3.Distance(worldA, worldB);

            // For flat-top hex with size 1, horizontal neighbors are 1.5 units apart on X
            Assert.AreEqual(1.5f, actualDistance, Tolerance);
        }

        // ── Equality ───────────────────────────────────────────────────────────

        [Test]
        public void Equality_SameCoords_AreEqual()
        {
            Assert.AreEqual(new HexCoord(3, -2), new HexCoord(3, -2));
        }

        [Test]
        public void Equality_DifferentCoords_AreNotEqual()
        {
            Assert.AreNotEqual(new HexCoord(1, 0), new HexCoord(0, 1));
        }

        [Test]
        public void EqualityOperator_SameCoords_ReturnsTrue()
        {
            Assert.IsTrue(new HexCoord(1, -1) == new HexCoord(1, -1));
        }

        [Test]
        public void InequalityOperator_DifferentCoords_ReturnsTrue()
        {
            Assert.IsTrue(new HexCoord(1, 0) != new HexCoord(0, 1));
        }

        [Test]
        public void GetHashCode_SameCoords_ReturnSameHash()
        {
            Assert.AreEqual(
                new HexCoord(2, -3).GetHashCode(),
                new HexCoord(2, -3).GetHashCode());
        }
    }
}
