using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using vv.Data.Repositories;
using vv.Domain.Models;
using vv.Domain.Repositories;
using vv.Domain.Repositories.Components;
using vv.Infrastructure.Repositories;
using System;
using IRepositoryFactory = vv.Domain.Repositories.IRepositoryFactory;

namespace vv.Infrastructure.Factories
{
    /// <summary>
    /// Factory for creating repository components
    /// </summary>
    public class RepositoryFactory : IRepositoryFactory
    {
        private readonly IServiceProvider _services;

        public RepositoryFactory(IServiceProvider services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
        }

        /// <inheritdoc/>
        public IMarketDataQueries CreateMarketDataQueries()
        {
            // Get dependencies
            var container = _services.GetRequiredService<Container>();
            var logger = _services.GetRequiredService<ILogger<MarketDataQueries>>();

            // Create cosmos repository for queries
            var cosmosRepo = new vv.Infrastructure.Repositories.Components.CosmosRepository<FxSpotPriceData>(
                container,
                logger,
                null, // No event publisher needed for queries
                entity => entity.AssetId.ToLowerInvariant());

            // Create versioning component
            var versioningComponent = new vv.Infrastructure.Repositories.Components.VersioningComponent<FxSpotPriceData>(
                container,
                logger,
                cosmosRepo,
                cosmosRepo);

            // Create market data queries implementation
            return new MarketDataQueries(
                cosmosRepo,
                versioningComponent,
                logger);
        }

        /// <inheritdoc/>
        public IMarketDataCommands CreateMarketDataCommands()
        {
            // Get dependencies
            var container = _services.GetRequiredService<Container>();
            var logger = _services.GetRequiredService<ILogger<MarketDataCommands>>();
            var idGenerator = _services.GetRequiredService<IEntityIdGenerator<FxSpotPriceData>>();
            var eventPublisher = _services.GetService<vv.Domain.Events.IEventPublisher>();

            // Create cosmos repository for commands
            var cosmosRepo = new vv.Infrastructure.Repositories.Components.CosmosRepository<FxSpotPriceData>(
                container,
                logger,
                eventPublisher,
                entity => entity.AssetId.ToLowerInvariant());

            // Create versioning component
            var versioningComponent = new vv.Infrastructure.Repositories.Components.VersioningComponent<FxSpotPriceData>(
                container,
                logger,
                cosmosRepo,
                cosmosRepo);

            // Create market data commands implementation
            return new MarketDataCommands(
                cosmosRepo,
                versioningComponent,
                eventPublisher,
                logger);
        }
    }
}