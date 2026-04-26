#nullable enable

using System;

namespace Game.Core
{
    /// <summary>
    /// Represents a resource identifier paired with an economy-safe amount.
    /// </summary>
    public readonly struct ResourceAmount : IEquatable<ResourceAmount>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceAmount"/> struct.
        /// </summary>
        /// <param name="resourceId">The resource identifier.</param>
        /// <param name="amount">The amount associated with the resource.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="resourceId"/> is null, empty, or whitespace.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="amount"/> is negative.
        /// </exception>
        public ResourceAmount(string resourceId, GameNumber amount)
        {
            if (string.IsNullOrWhiteSpace(resourceId))
            {
                throw new ArgumentException("Resource id cannot be null, empty, or whitespace.", nameof(resourceId));
            }

            if (amount < GameNumber.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Resource amount cannot be negative.");
            }

            ResourceId = resourceId;
            Amount = amount;
        }

        /// <summary>
        /// Gets the resource identifier.
        /// </summary>
        public string ResourceId { get; }

        /// <summary>
        /// Gets the amount associated with the resource.
        /// </summary>
        public GameNumber Amount { get; }

        /// <inheritdoc />
        public bool Equals(ResourceAmount other)
        {
            return string.Equals(ResourceId, other.ResourceId, StringComparison.Ordinal) && Amount == other.Amount;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is ResourceAmount other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(ResourceId, Amount);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{ResourceId}: {Amount}";
        }

        /// <summary>
        /// Determines whether two <see cref="ResourceAmount"/> values are equal.
        /// </summary>
        public static bool operator ==(ResourceAmount left, ResourceAmount right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two <see cref="ResourceAmount"/> values are not equal.
        /// </summary>
        public static bool operator !=(ResourceAmount left, ResourceAmount right)
        {
            return !left.Equals(right);
        }
    }
}

#nullable restore
