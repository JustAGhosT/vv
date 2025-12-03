using System;
using System.Collections.Generic;
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
    public class MarketDataRepository : MarketDataRepositoryBase, IMarketDataRepository
    {
        public MarketDataRepository(
            IRepository<FxSpotPriceData> repository,
            IVersioningCapability<FxSpotPriceData> versioning,
            ILogger<MarketDataRepository> logger)
            : base(repository, versioning, logger)
        {
        }

        /// <inheritdoc/>
        public Task<FxSpotPriceData?> GetLatestMarketDataAsync(
            string dataType,
            string assetClass,
            string assetId,
            string region,
            DateOnly asOfDate,
            string documentType,
            CancellationToken cancellationToken = default)
        {
            return GetLatestMarketDataInternalAsync(dataType, assetClass, assetId, region, asOfDate, documentType, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FxSpotPriceData>> QueryAsync(
            Expression<Func<FxSpotPriceData, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            Logger.LogInformation("Executing expression query on market data");

            return await Repository.QueryAsync(predicate, cancellationToken: cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<FxSpotPriceData> CreateMarketDataAsync(FxSpotPriceData data)
        {
            Logger.LogInformation(
                "Creating market data: DataType={DataType}, AssetClass={AssetClass}, AssetId={AssetId}",
                data.DataType, data.AssetClass, data.AssetId);

            var predicate = MarketDataQueryBuilder<FxSpotPriceData>.BuildMarketDataPredicate(
                data.DataType, data.AssetClass, data.AssetId, 
                data.Region, data.AsOfDate, data.DocumentType);

            return await Versioning.SaveVersionedEntityAsync(data, predicate);
        }

        /// <inheritdoc/>
        public async Task<FxSpotPriceData> UpdateMarketDataAsync(FxSpotPriceData data)
        {
            Logger.LogInformation(
                "Updating market data: Id={Id}, DataType={DataType}, AssetClass={AssetClass}, AssetId={AssetId}",
                data.Id, data.DataType, data.AssetClass, data.AssetId);

            return await Repository.UpdateAsync(data);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteMarketDataAsync(string id)
        {
            Logger.LogInformation("Deleting market data: Id={Id}", id);
            return await Repository.DeleteAsync(id);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<FxSpotPriceData>> QueryByRangeAsync(
            string dataType,
            string assetClass,
            string? assetId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            CancellationToken cancellationToken = default)
        {
            return QueryByRangeInternalAsync(dataType, assetClass, assetId, fromDate, toDate, cancellationToken);
        }
    }
}
