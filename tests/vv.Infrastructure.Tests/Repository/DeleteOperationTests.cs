using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Moq;
using vv.Domain.Models;
using vv.Infrastructure.Tests.Repository.Base;
using Xunit;

namespace vv.Infrastructure.Tests.Repository
{
    public class DeleteOperationTests : BaseMarketDataRepositoryTests
    {
        [Fact]
        public async Task DeleteAsync_ShouldCallDeleteItemAsync_WhenUsingHardDelete()
        {
            // Arrange - First setup ReadItemAsync for GetByIdAsync which is called first
            var mockGetResponse = new Mock<ItemResponse<FxSpotPriceData>>();
            mockGetResponse.Setup(r => r.Resource).Returns(MarketData);
            mockGetResponse.Setup(r => r.StatusCode).Returns(System.Net.HttpStatusCode.OK);

            MockContainer
                .Setup(c => c.ReadItemAsync<FxSpotPriceData>(
                    It.Is<string>(id => id == Id),
                    It.IsAny<PartitionKey>(),
                    It.IsAny<ItemRequestOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockGetResponse.Object);

            // Setup DeleteItemAsync
            MockContainer
                .Setup(c => c.DeleteItemAsync<FxSpotPriceData>(
                    It.Is<string>(id => id == Id),
                    It.IsAny<PartitionKey>(),
                    It.IsAny<ItemRequestOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Mock<ItemResponse<FxSpotPriceData>>().Object);

            // Act
            var result = await Repository.DeleteAsync(Id, false);

            // Assert
            Assert.True(result);

            MockContainer.Verify(c =>
                c.DeleteItemAsync<FxSpotPriceData>(
                    It.Is<string>(id => id == Id),
                    It.IsAny<PartitionKey>(),
                    It.IsAny<ItemRequestOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            // Verify event was published
            MockEventPublisher.Verify(e =>
                e.PublishAsync(
                    It.IsAny<object>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}