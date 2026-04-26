#nullable enable

using System;
using System.Globalization;

namespace Game.Core
{
    /// <summary>
    /// Represents an economy number without exposing the rest of the codebase to the current numeric storage.
    /// </summary>
    public readonly struct GameNumber : IEquatable<GameNumber>, IComparable, IComparable<GameNumber>, IFormattable
    {
        private const string CanonicalFormat = "0.################E0";
        private static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

        private readonly double _value;

        private GameNumber(double value)
        {
            _value = value;
        }

        /// <summary>
        /// Gets the additive identity.
        /// </summary>
        public static GameNumber Zero => new(0d);

        /// <summary>
        /// Gets the multiplicative identity.
        /// </summary>
        public static GameNumber One => new(1d);

        /// <summary>
        /// Creates a validated <see cref="GameNumber"/> from a <see cref="double"/>.
        /// </summary>
        /// <param name="value">The source value.</param>
        /// <returns>A validated game number.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="value"/> is not finite.
        /// </exception>
        public static GameNumber FromDouble(double value)
        {
            EnsureFinite(value, nameof(value));
            return new GameNumber(value);
        }

        /// <summary>
        /// Parses an invariant numeric string such as <c>1.23e5</c>.
        /// </summary>
        /// <param name="value">The string to parse.</param>
        /// <returns>The parsed game number.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
        /// <exception cref="FormatException">Thrown when <paramref name="value"/> is not a valid finite number.</exception>
        public static GameNumber Parse(string value)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (!TryParse(value, out var result))
            {
                throw new FormatException($"'{value}' is not a valid {nameof(GameNumber)}.");
            }

            return result;
        }

        /// <summary>
        /// Attempts to parse an invariant numeric string such as <c>1.23e5</c>.
        /// </summary>
        /// <param name="value">The string to parse.</param>
        /// <param name="result">The parsed value when successful.</param>
        /// <returns><see langword="true"/> when parsing succeeds; otherwise <see langword="false"/>.</returns>
        public static bool TryParse(string? value, out GameNumber result)
        {
            result = Zero;

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            if (!double.TryParse(value.Trim(), NumberStyles.Float, InvariantCulture, out var parsedValue))
            {
                return false;
            }

            if (!double.IsFinite(parsedValue))
            {
                return false;
            }

            result = new GameNumber(parsedValue);
            return true;
        }

        /// <summary>
        /// Parses a JSON string literal such as <c>"1.23e5"</c>.
        /// </summary>
        /// <param name="json">The JSON string literal to parse.</param>
        /// <returns>The parsed game number.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
        /// <exception cref="FormatException">Thrown when the JSON string literal is invalid.</exception>
        public static GameNumber ParseJsonString(string json)
        {
            if (json is null)
            {
                throw new ArgumentNullException(nameof(json));
            }

            if (!TryParseJsonString(json, out var result))
            {
                throw new FormatException($"'{json}' is not a valid JSON string literal for {nameof(GameNumber)}.");
            }

            return result;
        }

        /// <summary>
        /// Attempts to parse a JSON string literal such as <c>"1.23e5"</c>.
        /// </summary>
        /// <param name="json">The JSON string literal to parse.</param>
        /// <param name="result">The parsed value when successful.</param>
        /// <returns><see langword="true"/> when parsing succeeds; otherwise <see langword="false"/>.</returns>
        public static bool TryParseJsonString(string? json, out GameNumber result)
        {
            result = Zero;

            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            var trimmed = json.Trim();
            if (trimmed.Length < 2 || trimmed[0] != '"' || trimmed[^1] != '"')
            {
                return false;
            }

            return TryParse(trimmed[1..^1], out result);
        }

        /// <summary>
        /// Returns the current number as the wrapped <see cref="double"/>.
        /// </summary>
        /// <returns>The current value as <see cref="double"/>.</returns>
        public double ToDouble()
        {
            return _value;
        }

        /// <summary>
        /// Serializes the number as a JSON string literal.
        /// </summary>
        /// <returns>A JSON string literal such as <c>"1.23e5"</c>.</returns>
        public string ToJsonString()
        {
            return $"\"{ToCanonicalString(_value)}\"";
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return ToCanonicalString(_value);
        }

        /// <inheritdoc />
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            if (string.IsNullOrWhiteSpace(format))
            {
                return ToString();
            }

            if (string.Equals(format, "J", StringComparison.OrdinalIgnoreCase))
            {
                return ToJsonString();
            }

            return _value.ToString(format, formatProvider ?? InvariantCulture);
        }

        /// <inheritdoc />
        public bool Equals(GameNumber other)
        {
            return _value.Equals(other._value);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is GameNumber other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        /// <inheritdoc />
        public int CompareTo(GameNumber other)
        {
            return _value.CompareTo(other._value);
        }

        /// <inheritdoc />
        public int CompareTo(object? obj)
        {
            if (obj is null)
            {
                return 1;
            }

            if (obj is GameNumber other)
            {
                return CompareTo(other);
            }

            throw new ArgumentException($"Object must be of type {nameof(GameNumber)}.", nameof(obj));
        }

        /// <summary>
        /// Creates a validated <see cref="GameNumber"/> from a <see cref="double"/>.
        /// </summary>
        /// <param name="value">The source value.</param>
        public static explicit operator GameNumber(double value)
        {
            return FromDouble(value);
        }

        /// <summary>
        /// Converts a <see cref="GameNumber"/> into a <see cref="double"/>.
        /// </summary>
        /// <param name="value">The source game number.</param>
        public static explicit operator double(GameNumber value)
        {
            return value._value;
        }

        /// <summary>
        /// Adds two game numbers.
        /// </summary>
        public static GameNumber operator +(GameNumber left, GameNumber right)
        {
            return CreateChecked(left._value + right._value, "add");
        }

        /// <summary>
        /// Subtracts one game number from another.
        /// </summary>
        public static GameNumber operator -(GameNumber left, GameNumber right)
        {
            return CreateChecked(left._value - right._value, "subtract");
        }

        /// <summary>
        /// Multiplies two game numbers.
        /// </summary>
        public static GameNumber operator *(GameNumber left, GameNumber right)
        {
            return CreateChecked(left._value * right._value, "multiply");
        }

        /// <summary>
        /// Divides one game number by another.
        /// </summary>
        public static GameNumber operator /(GameNumber left, GameNumber right)
        {
            if (right._value == 0d)
            {
                throw new DivideByZeroException("Cannot divide a GameNumber by zero.");
            }

            return CreateChecked(left._value / right._value, "divide");
        }

        /// <summary>
        /// Determines whether two values are equal.
        /// </summary>
        public static bool operator ==(GameNumber left, GameNumber right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two values are not equal.
        /// </summary>
        public static bool operator !=(GameNumber left, GameNumber right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Determines whether the left value is less than the right value.
        /// </summary>
        public static bool operator <(GameNumber left, GameNumber right)
        {
            return left._value < right._value;
        }

        /// <summary>
        /// Determines whether the left value is greater than the right value.
        /// </summary>
        public static bool operator >(GameNumber left, GameNumber right)
        {
            return left._value > right._value;
        }

        /// <summary>
        /// Determines whether the left value is less than or equal to the right value.
        /// </summary>
        public static bool operator <=(GameNumber left, GameNumber right)
        {
            return left._value <= right._value;
        }

        /// <summary>
        /// Determines whether the left value is greater than or equal to the right value.
        /// </summary>
        public static bool operator >=(GameNumber left, GameNumber right)
        {
            return left._value >= right._value;
        }

        private static GameNumber CreateChecked(double value, string operation)
        {
            if (!double.IsFinite(value))
            {
                throw new OverflowException($"GameNumber cannot {operation} into a non-finite value.");
            }

            return new GameNumber(value);
        }

        private static void EnsureFinite(double value, string paramName)
        {
            if (!double.IsFinite(value))
            {
                throw new ArgumentOutOfRangeException(paramName, value, "GameNumber supports only finite values.");
            }
        }

        private static string ToCanonicalString(double value)
        {
            if (value == 0d)
            {
                return "0";
            }

            return value.ToString(CanonicalFormat, InvariantCulture).Replace('E', 'e');
        }
    }
}

#nullable restore
