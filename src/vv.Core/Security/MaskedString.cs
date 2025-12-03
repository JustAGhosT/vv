using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace vv.Core.Security
{
    /// <summary>
    /// A string type that masks its value during serialization and ToString() to prevent
    /// accidental exposure of sensitive data in logs, JSON responses, or debugging output.
    /// </summary>
    [JsonConverter(typeof(MaskedStringJsonConverter))]
    public sealed class MaskedString : IEquatable<MaskedString>
    {
        private const string MaskedValue = "***";
        private readonly string _value;

        /// <summary>
        /// Creates a new MaskedString from the given value.
        /// </summary>
        /// <param name="value">The secret value to store (can be null or empty).</param>
        public MaskedString(string? value)
        {
            _value = value ?? string.Empty;
        }

        /// <summary>
        /// Gets the unmasked value. Use with caution - this exposes the secret.
        /// </summary>
        /// <returns>The original unmasked value.</returns>
        public string GetUnmaskedValue() => _value;

        /// <summary>
        /// Returns the masked placeholder value ("***").
        /// </summary>
        public override string ToString() => MaskedValue;

        /// <summary>
        /// Implicit conversion from string to MaskedString.
        /// </summary>
        public static implicit operator MaskedString(string? value) => new MaskedString(value);

        /// <summary>
        /// Checks if the masked string has an actual non-empty value.
        /// </summary>
        public bool HasValue => !string.IsNullOrEmpty(_value);

        public bool Equals(MaskedString? other)
        {
            if (other is null) return false;
            return string.Equals(_value, other._value, StringComparison.Ordinal);
        }

        public override bool Equals(object? obj)
        {
            return obj is MaskedString other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public static bool operator ==(MaskedString? left, MaskedString? right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        public static bool operator !=(MaskedString? left, MaskedString? right)
        {
            return !(left == right);
        }
    }

    /// <summary>
    /// JSON converter for MaskedString that always serializes as "***" to prevent
    /// secret exposure in JSON output.
    /// </summary>
    public sealed class MaskedStringJsonConverter : JsonConverter<MaskedString>
    {
        private const string MaskedValue = "***";

        public override MaskedString? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return new MaskedString(value);
        }

        public override void Write(Utf8JsonWriter writer, MaskedString value, JsonSerializerOptions options)
        {
            // Always write the masked value - never expose the secret
            writer.WriteStringValue(MaskedValue);
        }
    }
}
