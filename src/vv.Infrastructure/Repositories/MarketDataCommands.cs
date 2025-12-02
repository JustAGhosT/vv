using Microsoft.Extensions.Logging;
using vv.Domain.Events;
using vv.Domain.Models;
using vv.Domain.Repositories;
using vv.Domain.Repositories.Components;
using vv.Domain.Specifications;
using vv.Infrastructure.Utilities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace vv.Infrastructure.Repositories
{
    /// <summary>
    /// Implementation of command operations for market data
    /// </summary>
    public class MarketDataCommands : IMarketDataCommands
    {
        private readonly ILogger<MarketDataCommands> _logger;
        private readonly IRepository<FxSpotPriceData> _repository;
        private readonly IVersioningCapability<FxSpotPriceData> _versioning;
        private readonly IEventPublisher? _eventPublisher;

        public MarketDataCommands(
            IRepository<FxSpotPriceData> repository,
            IVersioningCapability<FxSpotPriceData> versioning,
            IEventPublisher? eventPublisher,
            ILogger<MarketDataCommands> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _versioning = versioning ?? throw new ArgumentNullException(nameof(versioning));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventPublisher = eventPublisher;
        }

        /// <inheritdoc/>
        public async Task<string> SaveAsync(
            FxSpotPriceData marketData,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Saving market data: DataType={DataType}, AssetClass={AssetClass}, AssetId={AssetId}, Region={Region}, AsOf={AsOf}, DocType={DocType}, Version={Version}",
                marketData.DataType, marketData.AssetClass, marketData.AssetId, marketData.Region,
                marketData.AsOfDate, marketData.DocumentType, marketData.Version);

            // Use shared factory method for specification
            var spec = MarketDataQueryBuilder<FxSpotPriceData>.ForMarketData(
                marketData.DataType, marketData.AssetClass, marketData.AssetId,
                marketData.Region, marketData.AsOfDate, marketData.DocumentType);

            // Save the entity with versioning
            var result = await _versioning.SaveVersionedEntityAsync(marketData, spec, cancellationToken);

            // Publish event if available
            if (_eventPublisher != null)
            {
                await _eventPublisher.PublishAsync(new MarketDataSavedEvent
                {
                    EntityId = result.Id,
                    EntityType = typeof(FxSpotPriceData).Name,
                    Timestamp = DateTime.UtcNow
                }, cancellationToken);
            }

            return result.Id;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(
            string id,
            bool soft = false,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Deleting market data: Id={Id}, SoftDelete={SoftDelete}",
                id, soft);

            var result = await _repository.DeleteAsync(id, soft, cancellationToken);

            // Publish event if available
            if (result && _eventPublisher != null)
            {
                await _eventPublisher.PublishAsync(new MarketDataDeletedEvent
                {
                    EntityId = id,
                    EntityType = typeof(FxSpotPriceData).Name,
                    IsSoftDelete = soft,
                    Timestamp = DateTime.UtcNow
                }, cancellationToken);
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<int> PurgeSoftDeletedAsync(
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Purging soft-deleted market data");

            var count = await _repository.PurgeSoftDeletedAsync(cancellationToken);

            // Publish event if available
            if (count > 0 && _eventPublisher != null)
            {
                await _eventPublisher.PublishAsync(new MarketDataPurgedEvent
                {
                    EntityCount = count,
                    EntityType = typeof(FxSpotPriceData).Name,
                    Timestamp = DateTime.UtcNow
                }, cancellationToken);
            }

            return count;
        }

        /// <inheritdoc/>
        public async Task<string> SaveExchangeRateAsync(
            string baseCurrency,
            string quoteCurrency,
            decimal rate,
            DateOnly asOfDate,
            string region = "global",
            string documentType = "official",
            CancellationToken cancellationToken = default)
        {
            // Domain-specific method implementation
            var marketData = new FxSpotPriceData
            {
                DataType = "price.spot",
                AssetClass = "fx",
                AssetId = $"{baseCurrency}{quoteCurrency}".ToLowerInvariant(),
                Region = region,
                AsOfDate = asOfDate,
                DocumentType = documentType,
                Rate = rate,
                // Other properties as needed
            };

            return await SaveAsync(marketData, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<int> BulkSaveAsync(
            IEnumerable<FxSpotPriceData> marketDataItems,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Bulk saving market data items");

            int count = 0;
            foreach (var item in marketDataItems)
            {
                await SaveAsync(item, cancellationToken);
                count++;
            }

            return count;
        }
    }
}