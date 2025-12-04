using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Moq;
using vv.Domain.Models;
using vv.Infrastructure.Tests.Repository.Base;
using Xunit;

namespace vv.Infrastructure.Tests.Repository
{
    public class ErrorHandlingTests : BaseMarketDataRepositoryTests
    {
        [Fact]
        public async Task AddAsync_ShouldThrowException_WhenCosmosThrowsException()
        {
            // Arrange
            var cosmosException = new CosmosException("Failed to create", System.Net.HttpStatusCode.InternalServerError, 500, "1", 1.0);

            MockContainer
                .Setup(c => c.CreateItemAsync(
                    It.IsAny<FxSpotPriceData>(),
                    It.IsAny<PartitionKey>(),
                    It.IsAny<ItemRequestOptions>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(cosmosException);

            // Act & Assert
            await Assert.ThrowsAsync<CosmosException>(() => Repository.CreateAsync(MarketData));

            // Verify no event was published due to failure
            MockEventPublisher.Verify(e =>
                e.PublishAsync(
                    It.IsAny<object>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowException_WhenCosmosThrowsException()
        {
            // Arrange
            var cosmosException = new CosmosException("Failed to update", System.Net.HttpStatusCode.InternalServerError, 500, "1", 1.0);

            MockContainer
                .Setup(c => c.ReplaceItemAsync(
                    It.IsAny<FxSpotPriceData>(),
                    It.IsAny<string>(),
                    It.IsAny<PartitionKey>(),
                    It.IsAny<ItemRequestOptions>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(cosmosException);

            // Act & Assert
            await Assert.ThrowsAsync<CosmosException>(() => Repository.UpdateAsync(MarketData));

            // Verify no event was published due to failure
            MockEventPublisher.Verify(e =>
                e.PublishAsync(
                    It.IsAny<object>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenItemNotFoundDuringHardDelete()
        {
            // Arrange - First setup ReadItemAsync to throw NotFound (item doesn't exist)
            var notFoundException = new CosmosException("Not found", System.Net.HttpStatusCode.NotFound, 404, "1", 1.0);

            MockContainer
                .Setup(c => c.ReadItemAsync<FxSpotPriceData>(
                    It.IsAny<string>(),
                    It.IsAny<PartitionKey>(),
                    It.IsAny<ItemRequestOptions>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(notFoundException);

            // Act
            var result = await Repository.DeleteAsync(Id, false);

            // Assert
            Assert.False(result);

            // Verify no event was published
            MockEventPublisher.Verify(e =>
                e.PublishAsync(
                    It.IsAny<object>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}