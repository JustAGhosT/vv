using Microsoft.Extensions.Logging;
using vv.Domain.Models;
using vv.Domain.Repositories;
using vv.Domain.Repositories.Components;
using vv.Infrastructure.Utilities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace vv.Infrastructure.Repositories
{
    /// <summary>
    /// Implementation of query operations for market data
    /// </summary>
    public class MarketDataQueries : MarketDataRepositoryBase, IMarketDataQueries
    {
        public MarketDataQueries(
            IRepository<FxSpotPriceData> repository,
            IVersioningCapability<FxSpotPriceData> versioning,
            ILogger<MarketDataQueries> logger)
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
        public async Task<IEnumerable<FxSpotPriceData>> QueryByExpressionAsync(
            Expression<Func<FxSpotPriceData, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            Logger.LogInformation("Executing expression query on market data");
            return await Repository.QueryAsync(predicate, cancellationToken: cancellationToken);
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

        /// <inheritdoc/>
        public Task<(FxSpotPriceData? Result, string? ETag)> GetBySpecifiedVersionAsync(
            string dataType,
            string assetClass,
            string assetId,
            string region,
            DateOnly asOfDate,
            string documentType,
            int version)
        {
            var key = new MarketDataKey(dataType, assetClass, assetId, region, asOfDate, documentType);
            return GetBySpecifiedVersionInternalAsync(key, version, default);
        }

        /// <inheritdoc/>
        public Task<(FxSpotPriceData? Result, string? ETag)> GetByLatestVersionAsync(
            string dataType,
            string assetClass,
            string assetId,
            string region,
            DateOnly asOfDate,
            string documentType,
            CancellationToken cancellationToken = default)
        {
            return GetByLatestVersionInternalAsync(dataType, assetClass, assetId, region, asOfDate, documentType, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<FxSpotPriceData>> QueryAsync(
            string dataType,
            string assetClass,
            string? assetId = null,
            DateOnly? fromDate = null,
            DateOnly? toDate = null,
            CancellationToken cancellationToken = default)
        {
            DateTime? fromDateTime = fromDate.HasValue ? fromDate.Value.ToDateTime(TimeOnly.MinValue) : null;
            DateTime? toDateTime = toDate.HasValue ? toDate.Value.ToDateTime(TimeOnly.MaxValue) : null;

            return QueryByRangeInternalAsync(dataType, assetClass, assetId, fromDateTime, toDateTime, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<FxSpotPriceRate> GetLatestExchangeRateAsync(
            string baseCurrency,
            string quoteCurrency,
            DateOnly asOfDate,
            CancellationToken cancellationToken = default)
        {
            var predicate = MarketDataQueryBuilder<FxSpotPriceData>.BuildCurrencyPairPredicate(
                baseCurrency, quoteCurrency, asOfDate);
            var (entity, _) = await Versioning.GetByLatestVersionAsync(predicate, cancellationToken);
            
            if (entity == null)
            {
                throw new InvalidOperationException(
                    $"No exchange rate found for {baseCurrency}/{quoteCurrency} as of {asOfDate}");
            }
            
            return FxSpotPriceRate.FromEntity(entity);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FxSpotPriceData>> GetExchangeRateHistoryAsync(
            string baseCurrency,
            string quoteCurrency,
            DateOnly fromDate,
            DateOnly toDate,
            CancellationToken cancellationToken = default)
        {
            var predicate = MarketDataQueryBuilder<FxSpotPriceData>
                .ForCurrencyPair(baseCurrency, quoteCurrency)
                .WithFromDate(fromDate)
                .WithToDate(toDate)
                .Build();

            return await Repository.QueryAsync(predicate, cancellationToken: cancellationToken);
        }
    }
}
