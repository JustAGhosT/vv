using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using vv.Domain.Models;
using vv.Domain.Repositories;
using vv.Domain.Repositories.Components;
using vv.Infrastructure.Utilities;

namespace vv.Infrastructure.Repositories
{
    /// <summary>
    /// Cosmos DB repository implementation for market data using composition
    /// </summary>
    public class MarketDataRepository : IMarketDataRepository
    {
        private readonly ILogger<MarketDataRepository> _logger;
        private readonly IRepository<FxSpotPriceData> _repository;
        private readonly IVersioningCapability<FxSpotPriceData> _versioning;

        public MarketDataRepository(
            IRepository<FxSpotPriceData> repository,
            IVersioningCapability<FxSpotPriceData> versioning,
            ILogger<MarketDataRepository> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _versioning = versioning ?? throw new ArgumentNullException(nameof(versioning));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<FxSpotPriceData?> GetLatestMarketDataAsync(
            string dataType,
            string assetClass,
            string assetId,
            string region,
            DateOnly asOfDate,
            string documentType,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Retrieving latest market data: DataType={DataType}, AssetClass={AssetClass}, AssetId={AssetId}, Region={Region}, AsOf={AsOf}, DocType={DocType}",
                dataType, assetClass, assetId, region, asOfDate, documentType);

            var predicate = BuildMarketDataPredicate(dataType, assetClass, assetId, region, asOfDate, documentType);
            var (entity, _) = await _versioning.GetByLatestVersionAsync(predicate, cancellationToken);
            return entity;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Note: This method loads all data into memory for filtering. For large datasets,
        /// consider using QueryByRangeAsync with database-level filtering instead.
        /// </remarks>
        public async Task<IEnumerable<FxSpotPriceData>> QueryAsync(Func<FxSpotPriceData, bool> predicate)
        {
            _logger.LogInformation("Executing predicate query on market data");

            var results = await _repository.GetAllAsync(cancellationToken: default);
            return results.Where(predicate);
        }

        /// <inheritdoc/>
        public async Task<FxSpotPriceData> CreateMarketDataAsync(FxSpotPriceData data)
        {
            _logger.LogInformation(
                "Creating market data: DataType={DataType}, AssetClass={AssetClass}, AssetId={AssetId}",
                data.DataType, data.AssetClass, data.AssetId);

            var predicate = BuildMarketDataPredicate(
                data.DataType, data.AssetClass, data.AssetId, 
                data.Region, data.AsOfDate, data.DocumentType);

            return await _versioning.SaveVersionedEntityAsync(data, predicate);
        }

        /// <inheritdoc/>
        public async Task<FxSpotPriceData> UpdateMarketDataAsync(FxSpotPriceData data)
        {
            _logger.LogInformation(
                "Updating market data: Id={Id}, DataType={DataType}, AssetClass={AssetClass}, AssetId={AssetId}",
                data.Id, data.DataType, data.AssetClass, data.AssetId);

            return await _repository.UpdateAsync(data);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteMarketDataAsync(string id)
        {
            _logger.LogInformation("Deleting market data: Id={Id}", id);
            return await _repository.DeleteAsync(id);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FxSpotPriceData>> QueryByRangeAsync(
            string dataType,
            string assetClass,
            string? assetId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Querying market data by range: DataType={DataType}, AssetClass={AssetClass}, AssetId={AssetId}, FromDate={FromDate}, ToDate={ToDate}",
                dataType, assetClass, assetId ?? "any", fromDate, toDate);

            DateOnly? fromDateOnly = fromDate.HasValue ? DateOnly.FromDateTime(fromDate.Value) : null;
            DateOnly? toDateOnly = toDate.HasValue ? DateOnly.FromDateTime(toDate.Value) : null;

            var predicate = BuildRangeQueryPredicate(dataType, assetClass, assetId, fromDateOnly, toDateOnly);
            return await _repository.QueryAsync(predicate, cancellationToken: cancellationToken);
        }

        // Shared helper methods to reduce code duplication
        private static Expression<Func<FxSpotPriceData, bool>> BuildMarketDataPredicate(
            string dataType,
            string assetClass,
            string assetId,
            string region,
            DateOnly asOfDate,
            string documentType)
        {
            return MarketDataQueryBuilder<FxSpotPriceData>
                .ForMarketData(dataType, assetClass, assetId, region, asOfDate, documentType)
                .Build();
        }

        private static Expression<Func<FxSpotPriceData, bool>> BuildRangeQueryPredicate(
            string dataType,
            string assetClass,
            string? assetId,
            DateOnly? fromDate,
            DateOnly? toDate)
        {
            return MarketDataQueryBuilder<FxSpotPriceData>
                .ForRangeQuery(dataType, assetClass, assetId, fromDate, toDate)
                .Build();
        }
    }
}
