using System;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using vv.Data.Repositories;
using vv.Domain.Events;
using vv.Domain.Models;
using vv.Domain.Repositories;
using vv.Domain.Repositories.Components;
using vv.Infrastructure.Configuration;
using vv.Infrastructure.Repositories.Components;
using IRepositoryFactory = vv.Domain.Repositories.IRepositoryFactory;

namespace vv.Infrastructure.Repositories
{
    /// <summary>
    /// Factory for creating Cosmos DB repositories
    /// </summary>
    public class CosmosRepositoryFactory : IRepositoryFactory
    {
        private readonly CosmosClient _cosmosClient;
        private readonly ILoggerFactory _loggerFactory;
        private readonly CosmosDbOptions _options;
        private readonly IEventPublisher? _eventPublisher;

        public CosmosRepositoryFactory(
            CosmosClient cosmosClient,
            ILoggerFactory loggerFactory,
            IOptions<CosmosDbOptions> options,
            IEventPublisher? eventPublisher = null)
        {
            _cosmosClient = cosmosClient ?? throw new ArgumentNullException(nameof(cosmosClient));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _eventPublisher = eventPublisher;
        }

        /// <inheritdoc/>
        public IMarketDataQueries CreateMarketDataQueries()
        {
            var container = GetMarketDataContainer();
            var logger = _loggerFactory.CreateLogger<MarketDataQueries>();

            // Create cosmos repository for queries
            var cosmosRepo = new Components.CosmosRepository<FxSpotPriceData>(
                container,
                logger,
                null, // No event publisher needed for queries
                entity => entity.AssetId.ToLowerInvariant());

            // Create versioning component
            var versioningComponent = new VersioningComponent<FxSpotPriceData>(
                container,
                logger,
                cosmosRepo,
                cosmosRepo,
                null); // No ID generator needed for queries

            return new MarketDataQueries(
                cosmosRepo,
                versioningComponent,
                logger);
        }

        /// <inheritdoc/>
        public IMarketDataCommands CreateMarketDataCommands()
        {
            var container = GetMarketDataContainer();
            var logger = _loggerFactory.CreateLogger<MarketDataCommands>();
            var idGenerator = new MarketDataIdGenerator<FxSpotPriceData>();

            // Create cosmos repository for commands
            var cosmosRepo = new Components.CosmosRepository<FxSpotPriceData>(
                container,
                logger,
                _eventPublisher,
                entity => entity.AssetId.ToLowerInvariant());

            // Create versioning component
            var versioningComponent = new VersioningComponent<FxSpotPriceData>(
                container,
                logger,
                cosmosRepo,
                cosmosRepo,
                idGenerator);

            return new MarketDataCommands(
                cosmosRepo,
                versioningComponent,
                _eventPublisher,
                logger);
        }

        /// <summary>
        /// Gets the Cosmos DB container for market data
        /// </summary>
        private Container GetMarketDataContainer()
        {
            return _cosmosClient.GetContainer(_options.DatabaseName, _options.MarketDataContainerName);
        }
    }
}