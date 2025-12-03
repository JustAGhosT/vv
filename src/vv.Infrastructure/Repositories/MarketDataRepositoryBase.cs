using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using vv.Domain.Models;
using vv.Domain.Repositories.Components;
using vv.Infrastructure.Utilities;

namespace vv.Infrastructure.Repositories
{
    /// <summary>
    /// Parameters that identify market data for queries.
    /// Used to reduce parameter count in repository methods.
    /// </summary>
    public record MarketDataKey(
        string DataType,
        string AssetClass,
        string AssetId,
        string Region,
        DateOnly AsOfDate,
        string DocumentType);

    /// <summary>
    /// Base class containing shared functionality for market data repositories
    /// to reduce code duplication between MarketDataRepository and MarketDataQueries.
    /// </summary>
    public abstract class MarketDataRepositoryBase
    {
        protected readonly IRepository<FxSpotPriceData> Repository;
        protected readonly IVersioningCapability<FxSpotPriceData> Versioning;
        protected readonly ILogger Logger;

        protected MarketDataRepositoryBase(
            IRepository<FxSpotPriceData> repository,
            IVersioningCapability<FxSpotPriceData> versioning,
            ILogger logger)
        {
            Repository = repository ?? throw new ArgumentNullException(nameof(repository));
            Versioning = versioning ?? throw new ArgumentNullException(nameof(versioning));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the latest version of market data matching the specified criteria.
        /// Shared implementation used by both MarketDataRepository and MarketDataQueries.
        /// </summary>
        protected async Task<FxSpotPriceData?> GetLatestMarketDataInternalAsync(
            string dataType,
            string assetClass,
            string assetId,
            string region,
            DateOnly asOfDate,
            string documentType,
            CancellationToken cancellationToken = default)
        {
            Logger.LogInformation(
                "Retrieving latest market data: DataType={DataType}, AssetClass={AssetClass}, AssetId={AssetId}, Region={Region}, AsOf={AsOf}, DocType={DocType}",
                dataType, assetClass, assetId, region, asOfDate, documentType);

            var predicate = MarketDataQueryBuilder<FxSpotPriceData>.BuildMarketDataPredicate(
                dataType, assetClass, assetId, region, asOfDate, documentType);
            var (entity, _) = await Versioning.GetByLatestVersionAsync(predicate, cancellationToken);
            return entity;
        }

        /// <summary>
        /// Queries market data within a specified date range.
        /// Shared implementation used by both MarketDataRepository and MarketDataQueries.
        /// </summary>
        protected async Task<IEnumerable<FxSpotPriceData>> QueryByRangeInternalAsync(
            string dataType,
            string assetClass,
            string? assetId,
            DateOnly? fromDate,
            DateOnly? toDate,
            CancellationToken cancellationToken = default)
        {
            Logger.LogInformation(
                "Querying market data by range: DataType={DataType}, AssetClass={AssetClass}, AssetId={AssetId}, FromDate={FromDate}, ToDate={ToDate}",
                dataType, assetClass, assetId ?? "any", fromDate, toDate);

            var predicate = MarketDataQueryBuilder<FxSpotPriceData>.BuildRangeQueryPredicate(
                dataType, assetClass, assetId, fromDate, toDate);
            return await Repository.QueryAsync(predicate, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Gets the latest version with ETag for optimistic concurrency.
        /// Shared implementation used by both MarketDataRepository and MarketDataQueries.
        /// </summary>
        protected async Task<(FxSpotPriceData? Result, string? ETag)> GetByLatestVersionInternalAsync(
            string dataType,
            string assetClass,
            string assetId,
            string region,
            DateOnly asOfDate,
            string documentType,
            CancellationToken cancellationToken = default)
        {
            Logger.LogInformation(
                "Retrieving latest version of market data: DataType={DataType}, AssetClass={AssetClass}, AssetId={AssetId}, Region={Region}, AsOf={AsOf}, DocType={DocType}",
                dataType, assetClass, assetId, region, asOfDate, documentType);

            var predicate = MarketDataQueryBuilder<FxSpotPriceData>.BuildMarketDataPredicate(
                dataType, assetClass, assetId, region, asOfDate, documentType);
            return await Versioning.GetByLatestVersionAsync(predicate, cancellationToken);
        }

        /// <summary>
        /// Gets a specific version of market data.
        /// Shared implementation used by MarketDataQueries.
        /// </summary>
        protected async Task<(FxSpotPriceData? Result, string? ETag)> GetBySpecifiedVersionInternalAsync(
            MarketDataKey key,
            int version,
            CancellationToken cancellationToken = default)
        {
            Logger.LogInformation(
                "Retrieving specific version of market data: DataType={DataType}, AssetClass={AssetClass}, AssetId={AssetId}, Region={Region}, AsOf={AsOf}, DocType={DocType}, Version={Version}",
                key.DataType, key.AssetClass, key.AssetId, key.Region, key.AsOfDate, key.DocumentType, version);

            var predicate = MarketDataQueryBuilder<FxSpotPriceData>.BuildMarketDataPredicate(
                key.DataType, key.AssetClass, key.AssetId, key.Region, key.AsOfDate, key.DocumentType);
            return await Versioning.GetBySpecifiedVersionAsync(predicate, version, cancellationToken);
        }
    }
}
