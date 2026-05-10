using NUnit.Framework;
using UnityEngine;

namespace GameCore.Resources.Tests
{
    public class ResourceBundleTests
    {
        private class TestResource : IResource
        {
            public string ResourceId { get; }
            public string DisplayName { get; }
            public TestResource(string resourceId) { ResourceId = resourceId; DisplayName = resourceId; }
        }

        private readonly TestResource _wood = new("wood");
        private readonly TestResource _brick = new("brick");
        private readonly TestResource _ore = new("ore");

        // ── Get ────────────────────────────────────────────────────────────────

        [Test]
        public void Get_ResourceNotAdded_ReturnsZero()
        {
            var bundle = new ResourceBundle();
            Assert.AreEqual(0, bundle.Get(_wood));
        }

        [Test]
        public void Get_AfterAdd_ReturnsCorrectAmount()
        {
            var bundle = new ResourceBundle().Add(_wood, 3);
            Assert.AreEqual(3, bundle.Get(_wood));
        }

        // ── Add ────────────────────────────────────────────────────────────────

        [Test]
        public void Add_ReturnsNewInstance_OriginalUnchanged()
        {
            var original = new ResourceBundle();
            var modified = original.Add(_wood, 5);

            Assert.AreEqual(0, original.Get(_wood));
            Assert.AreEqual(5, modified.Get(_wood));
        }

        [Test]
        public void Add_TwiceToSameResource_AccumulatesCorrectly()
        {
            var bundle = new ResourceBundle()
                .Add(_wood, 2)
                .Add(_wood, 3);

            Assert.AreEqual(5, bundle.Get(_wood));
        }

        [Test]
        public void Add_MultipleResources_AllStoredIndependently()
        {
            var bundle = new ResourceBundle()
                .Add(_wood, 1)
                .Add(_brick, 2)
                .Add(_ore, 3);

            Assert.AreEqual(1, bundle.Get(_wood));
            Assert.AreEqual(2, bundle.Get(_brick));
            Assert.AreEqual(3, bundle.Get(_ore));
        }

        // ── Remove ─────────────────────────────────────────────────────────────

        [Test]
        public void Remove_ReturnsNewInstance_OriginalUnchanged()
        {
            var original = new ResourceBundle().Add(_wood, 5);
            var modified = original.Remove(_wood, 2);

            Assert.AreEqual(5, original.Get(_wood));
            Assert.AreEqual(3, modified.Get(_wood));
        }

        [Test]
        public void Remove_ExactAmount_ResultsInZero()
        {
            var bundle = new ResourceBundle().Add(_wood, 3).Remove(_wood, 3);
            Assert.AreEqual(0, bundle.Get(_wood));
        }

        [Test]
        public void Remove_MoreThanAvailable_ClampsToZero()
        {
            var bundle = new ResourceBundle().Add(_wood, 2).Remove(_wood, 10);
            Assert.AreEqual(0, bundle.Get(_wood));
        }

        // ── CanAfford ──────────────────────────────────────────────────────────

        [Test]
        public void CanAfford_ExactAmount_ReturnsTrue()
        {
            var inventory = new ResourceBundle().Add(_wood, 3);
            var cost = new ResourceBundle().Add(_wood, 3);
            Assert.IsTrue(inventory.CanAfford(cost));
        }

        [Test]
        public void CanAfford_MoreThanCost_ReturnsTrue()
        {
            var inventory = new ResourceBundle().Add(_wood, 5);
            var cost = new ResourceBundle().Add(_wood, 3);
            Assert.IsTrue(inventory.CanAfford(cost));
        }

        [Test]
        public void CanAfford_LessThanCost_ReturnsFalse()
        {
            var inventory = new ResourceBundle().Add(_wood, 2);
            var cost = new ResourceBundle().Add(_wood, 3);
            Assert.IsFalse(inventory.CanAfford(cost));
        }

        [Test]
        public void CanAfford_EmptyCost_ReturnsTrue()
        {
            var inventory = new ResourceBundle();
            var cost = new ResourceBundle();
            Assert.IsTrue(inventory.CanAfford(cost));
        }

        [Test]
        public void CanAfford_MultipleResources_AllMustBeSufficient()
        {
            var inventory = new ResourceBundle().Add(_wood, 2).Add(_brick, 1);
            var cost = new ResourceBundle().Add(_wood, 2).Add(_brick, 2);
            Assert.IsFalse(inventory.CanAfford(cost));
        }

        [Test]
        public void CanAfford_ResourceMissing_ReturnsFalse()
        {
            var inventory = new ResourceBundle().Add(_wood, 5);
            var cost = new ResourceBundle().Add(_brick, 1);
            Assert.IsFalse(inventory.CanAfford(cost));
        }

        // ── Operator + ─────────────────────────────────────────────────────────

        [Test]
        public void AddOperator_CombinesTwoBundles()
        {
            var bundleA = new ResourceBundle().Add(_wood, 2).Add(_brick, 1);
            var bundleB = new ResourceBundle().Add(_wood, 1).Add(_ore, 3);
            var combined = bundleA + bundleB;

            Assert.AreEqual(3, combined.Get(_wood));
            Assert.AreEqual(1, combined.Get(_brick));
            Assert.AreEqual(3, combined.Get(_ore));
        }

        [Test]
        public void AddOperator_DoesNotMutateOperands()
        {
            var bundleA = new ResourceBundle().Add(_wood, 2);
            var bundleB = new ResourceBundle().Add(_wood, 3);
            var _ = bundleA + bundleB;

            Assert.AreEqual(2, bundleA.Get(_wood));
            Assert.AreEqual(3, bundleB.Get(_wood));
        }
    }
}
