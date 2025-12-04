using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Moq;
using vv.Domain.Models;
using vv.Infrastructure.Repositories;
using vv.Infrastructure.Tests.Repository.Base;
using Xunit;

namespace vv.Infrastructure.Tests.Repository
{
    public class BatchOperationTests : BaseMarketDataRepositoryTests
    {
        [Fact]
        public async Task BulkInsertAsync_ShouldCallCreateItemAsyncForEachItem()
        {
            // Arrange
            var secondItem = new FxSpotPriceData
            {
                Price = 1.2m,
                Version = 1,
                AssetId = "jpyusd",
                AssetClass = "fx",
                DataType = "price.spot",
                Region = "global",
                DocumentType = "official",
                AsOfDate = new DateOnly(2025, 5, 14),
                SchemaVersion = "0.0.0"
                // Don't set Id directly, it's calculated from the above properties
            };

            var items = new List<FxSpotPriceData> { MarketData, secondItem };
            var mockResponse = new Mock<ItemResponse<FxSpotPriceData>>();
            mockResponse.Setup(r => r.Resource).Returns(MarketData);

            MockContainer
                .Setup(c => c.CreateItemAsync(
                    It.IsAny<FxSpotPriceData>(),
                    It.IsAny<PartitionKey>(),
                    It.IsAny<ItemRequestOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            // Act
            var result = await Repository.BulkInsertAsync(items);

            // Assert
            Assert.Equal(2, result);
            MockContainer.Verify(c =>
                c.CreateItemAsync(
                    It.IsAny<FxSpotPriceData>(),
                    It.IsAny<PartitionKey>(),
                    It.IsAny<ItemRequestOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(2));
        }
    }
}