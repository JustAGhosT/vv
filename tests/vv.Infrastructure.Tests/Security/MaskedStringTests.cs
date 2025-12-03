using System.Text.Json;
using vv.Core.Security;
using Xunit;
using FluentAssertions;

namespace vv.Infrastructure.Tests.Security
{
    public class MaskedStringTests
    {
        [Fact]
        public void ToString_ShouldReturnMaskedValue()
        {
            // Arrange
            var maskedString = new MaskedString("my-secret-api-key");

            // Act
            var result = maskedString.ToString();

            // Assert
            result.Should().Be("***");
        }

        [Fact]
        public void ToString_WithNullValue_ShouldReturnMaskedValue()
        {
            // Arrange
            var maskedString = new MaskedString(null);

            // Act
            var result = maskedString.ToString();

            // Assert
            result.Should().Be("***");
        }

        [Fact]
        public void GetUnmaskedValue_ShouldReturnOriginalValue()
        {
            // Arrange
            const string secret = "my-secret-api-key";
            var maskedString = new MaskedString(secret);

            // Act
            var result = maskedString.GetUnmaskedValue();

            // Assert
            result.Should().Be(secret);
        }

        [Fact]
        public void GetUnmaskedValue_WithNullInput_ShouldReturnEmptyString()
        {
            // Arrange
            var maskedString = new MaskedString(null);

            // Act
            var result = maskedString.GetUnmaskedValue();

            // Assert
            result.Should().Be(string.Empty);
        }

        [Fact]
        public void HasValue_WithNonEmptyString_ShouldReturnTrue()
        {
            // Arrange
            var maskedString = new MaskedString("secret");

            // Act & Assert
            maskedString.HasValue.Should().BeTrue();
        }

        [Fact]
        public void HasValue_WithNullInput_ShouldReturnFalse()
        {
            // Arrange
            var maskedString = new MaskedString(null);

            // Act & Assert
            maskedString.HasValue.Should().BeFalse();
        }

        [Fact]
        public void HasValue_WithEmptyString_ShouldReturnFalse()
        {
            // Arrange
            var maskedString = new MaskedString(string.Empty);

            // Act & Assert
            maskedString.HasValue.Should().BeFalse();
        }

        [Fact]
        public void ImplicitConversion_FromString_ShouldWork()
        {
            // Arrange & Act
            MaskedString maskedString = "my-secret";

            // Assert
            maskedString.GetUnmaskedValue().Should().Be("my-secret");
            maskedString.ToString().Should().Be("***");
        }

        [Fact]
        public void Equals_WithSameValue_ShouldReturnTrue()
        {
            // Arrange
            var maskedString1 = new MaskedString("secret");
            var maskedString2 = new MaskedString("secret");

            // Act & Assert
            maskedString1.Equals(maskedString2).Should().BeTrue();
            (maskedString1 == maskedString2).Should().BeTrue();
        }

        [Fact]
        public void Equals_WithDifferentValue_ShouldReturnFalse()
        {
            // Arrange
            var maskedString1 = new MaskedString("secret1");
            var maskedString2 = new MaskedString("secret2");

            // Act & Assert
            maskedString1.Equals(maskedString2).Should().BeFalse();
            (maskedString1 != maskedString2).Should().BeTrue();
        }

        [Fact]
        public void JsonSerialization_ShouldSerializeAsMaskedValue()
        {
            // Arrange
            var maskedString = new MaskedString("actual-secret-value");

            // Act
            var json = JsonSerializer.Serialize(maskedString);

            // Assert
            json.Should().Be("\"***\"");
            json.Should().NotContain("actual-secret-value");
        }

        [Fact]
        public void JsonSerialization_InObject_ShouldMaskApiKeyHash()
        {
            // Arrange
            var dto = new TestDtoWithMaskedString
            {
                Name = "Test Integration",
                ApiKeyHash = new MaskedString("super-secret-key-12345")
            };

            // Act
            var json = JsonSerializer.Serialize(dto);

            // Assert
            json.Should().Contain("\"ApiKeyHash\":\"***\"");
            json.Should().NotContain("super-secret-key-12345");
        }

        [Fact]
        public void JsonDeserialization_ShouldReadValue()
        {
            // Arrange
            var json = "\"input-value\"";

            // Act
            var maskedString = JsonSerializer.Deserialize<MaskedString>(json);

            // Assert
            maskedString.Should().NotBeNull();
            maskedString!.GetUnmaskedValue().Should().Be("input-value");
        }

        [Fact]
        public void JsonDeserialization_InObject_ShouldReadValue()
        {
            // Arrange
            var json = "{\"Name\":\"Test\",\"ApiKeyHash\":\"secret-from-json\"}";

            // Act
            var dto = JsonSerializer.Deserialize<TestDtoWithMaskedString>(json);

            // Assert
            dto.Should().NotBeNull();
            dto!.ApiKeyHash.GetUnmaskedValue().Should().Be("secret-from-json");
            dto.ApiKeyHash.ToString().Should().Be("***");
        }

        [Fact]
        public void RoundTrip_SerializeDeserializeReserialize_ShouldAlwaysMask()
        {
            // Arrange
            var original = new MaskedString("my-original-secret");

            // Act - Serialize (masks it)
            var json1 = JsonSerializer.Serialize(original);
            
            // Deserialize (reads the masked value "***")
            var deserialized = JsonSerializer.Deserialize<MaskedString>(json1);
            
            // Serialize again
            var json2 = JsonSerializer.Serialize(deserialized);

            // Assert
            json1.Should().Be("\"***\"");
            json2.Should().Be("\"***\"");
            deserialized!.GetUnmaskedValue().Should().Be("***"); // The value is now "***" after deserialization
        }

        private class TestDtoWithMaskedString
        {
            public string Name { get; set; } = string.Empty;
            public MaskedString ApiKeyHash { get; set; } = new MaskedString(string.Empty);
        }
    }
}
