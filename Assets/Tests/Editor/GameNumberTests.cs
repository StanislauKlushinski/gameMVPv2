using System;
using Game.Core;
using NUnit.Framework;

namespace Game.Tests.Editor
{
    public class GameNumberTests
    {
        [Test]
        public void ArithmeticOperators_ReturnExpectedValues()
        {
            var left = GameNumber.FromDouble(12d);
            var right = GameNumber.FromDouble(3d);

            Assert.That((double)(left + right), Is.EqualTo(15d));
            Assert.That((double)(left - right), Is.EqualTo(9d));
            Assert.That((double)(left * right), Is.EqualTo(36d));
            Assert.That((double)(left / right), Is.EqualTo(4d));
        }

        [Test]
        public void ComparisonOperators_WorkCorrectly()
        {
            var smaller = GameNumber.FromDouble(5d);
            var larger = GameNumber.FromDouble(10d);

            Assert.That(smaller < larger, Is.True);
            Assert.That(smaller <= larger, Is.True);
            Assert.That(larger > smaller, Is.True);
            Assert.That(larger >= smaller, Is.True);
            Assert.That(smaller == GameNumber.FromDouble(5d), Is.True);
            Assert.That(smaller != larger, Is.True);
        }

        [Test]
        public void ZeroAndOne_ReturnExpectedValues()
        {
            Assert.That((double)GameNumber.Zero, Is.EqualTo(0d));
            Assert.That((double)GameNumber.One, Is.EqualTo(1d));
        }

        [Test]
        public void JsonSerialization_RoundTripsScientificString()
        {
            var value = GameNumber.FromDouble(123000d);

            Assert.That(value.ToString(), Is.EqualTo("1.23e5"));
            Assert.That(value.ToJsonString(), Is.EqualTo("\"1.23e5\""));
            Assert.That(GameNumber.ParseJsonString("\"1.23e5\""), Is.EqualTo(value));
        }

        [Test]
        public void FromDouble_RejectsNonFiniteValues()
        {
            Assert.That(() => GameNumber.FromDouble(double.NaN), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => GameNumber.FromDouble(double.PositiveInfinity), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => GameNumber.FromDouble(double.NegativeInfinity), Throws.TypeOf<ArgumentOutOfRangeException>());
        }
    }
}
