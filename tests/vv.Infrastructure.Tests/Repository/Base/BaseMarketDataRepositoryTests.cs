using System;
using System.Threading;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Moq;
using vv.Domain.Events;
using vv.Domain.Models;
using vv.Infrastructure.Repositories;

namespace vv.Infrastructure.Tests.Repository.Base
{
    /// <summary>
    /// Concrete test implementation of CosmosRepository for FxSpotPriceData
    /// </summary>
    public class TestFxSpotPriceDataRepository : CosmosRepository<FxSpotPriceData>
    {
        public TestFxSpotPriceDataRepository(
            Container container,
            ILogger logger,
            IEventPublisher? eventPublisher = null)
            : base(container, logger, eventPublisher)
        {
        }
    }

    public abstract class BaseMarketDataRepositoryTests
    {
        protected readonly Mock<Container> MockContainer;
        protected readonly Mock<ILogger<TestFxSpotPriceDataRepository>> MockLogger;
        protected readonly Mock<IEventPublisher> MockEventPublisher;
        protected readonly TestFxSpotPriceDataRepository Repository;
        protected readonly string Id;
        protected readonly string PartitionKey;
        protected readonly FxSpotPriceData MarketData;

        protected BaseMarketDataRepositoryTests()
        {
            MockContainer = new Mock<Container>();
            MockLogger = new Mock<ILogger<TestFxSpotPriceDataRepository>>();
            MockEventPublisher = new Mock<IEventPublisher>();

            Repository = new TestFxSpotPriceDataRepository(
                MockContainer.Object,
                MockLogger.Object,
                MockEventPublisher.Object);

            // Define these first to match the original file structure
            Id = "price.spot__fx__eurusd__global__2025-05-14__official__1";
            PartitionKey = "eurusd";

            // Initialize MarketData but don't set Id directly as it's calculated from properties
            MarketData = new FxSpotPriceData
            {
                Price = 1.1m,
                Version = 1,
                SchemaVersion = "0.0.0",
                AssetId = "eurusd",
                AssetClass = "fx",
                DataType = "price.spot",
                Region = "global",
                DocumentType = "official",
                AsOfDate = new DateOnly(2025, 5, 14)
            };

            // Note: Id is computed automatically based on the properties above
            // so we don't need to set it explicitly
        }
    }
}