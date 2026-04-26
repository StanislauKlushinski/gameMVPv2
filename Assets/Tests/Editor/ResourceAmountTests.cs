using System;
using Game.Core;
using NUnit.Framework;

namespace Game.Tests.Editor
{
    public class ResourceAmountTests
    {
        [Test]
        public void Constructor_AssignsProperties()
        {
            var amount = new ResourceAmount("wood", GameNumber.FromDouble(12.5d));

            Assert.That(amount.ResourceId, Is.EqualTo("wood"));
            Assert.That(amount.Amount, Is.EqualTo(GameNumber.FromDouble(12.5d)));
        }

        [Test]
        public void Constructor_RejectsBlankResourceId()
        {
            Assert.That(() => new ResourceAmount(string.Empty, GameNumber.One), Throws.TypeOf<ArgumentException>());
            Assert.That(() => new ResourceAmount("   ", GameNumber.One), Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void Constructor_RejectsNegativeAmount()
        {
            Assert.That(
                () => new ResourceAmount("wood", GameNumber.FromDouble(-1d)),
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void Equality_UsesResourceIdAndAmount()
        {
            var baseline = new ResourceAmount("stone", GameNumber.FromDouble(3d));
            var same = new ResourceAmount("stone", GameNumber.FromDouble(3d));
            var differentResource = new ResourceAmount("wood", GameNumber.FromDouble(3d));
            var differentAmount = new ResourceAmount("stone", GameNumber.FromDouble(4d));

            Assert.That(baseline == same, Is.True);
            Assert.That(baseline.Equals(same), Is.True);
            Assert.That(baseline != differentResource, Is.True);
            Assert.That(baseline != differentAmount, Is.True);
        }
    }
}
