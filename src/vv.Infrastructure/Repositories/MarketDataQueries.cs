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
    public class MarketDataQueries : IMarketDataQueries
    {
        private readonly ILogger<MarketDataQueries> _logger;
        private readonly IRepository<FxSpotPriceData> _repository;
        private readonly IVersioningCapability<FxSpotPriceData> _versioning;

        public MarketDataQueries(
            IRepository<FxSpotPriceData> repository,
            IVersioningCapability<FxSpotPriceData> versioning,
            ILogger<MarketDataQueries> logger)
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
        public async Task<IEnumerable<FxSpotPriceData>> QueryByExpressionAsync(
            Expression<Func<FxSpotPriceData, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Executing expression query on market data");
            return await _repository.QueryAsync(predicate, cancellationToken: cancellationToken);
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

        /// <inheritdoc/>
        public async Task<(FxSpotPriceData? Result, string? ETag)> GetBySpecifiedVersionAsync(
            string dataType,
            string assetClass,
            string assetId,
            string region,
            DateOnly asOfDate,
            string documentType,
            int version)
        {
            _logger.LogInformation(
                "Retrieving specific version of market data: DataType={DataType}, AssetClass={AssetClass}, AssetId={AssetId}, Region={Region}, AsOf={AsOf}, DocType={DocType}, Version={Version}",
                dataType, assetClass, assetId, region, asOfDate, documentType, version);

            var predicate = BuildMarketDataPredicate(dataType, assetClass, assetId, region, asOfDate, documentType);
            return await _versioning.GetBySpecifiedVersionAsync(predicate, version);
        }

        /// <inheritdoc/>
        public async Task<(FxSpotPriceData? Result, string? ETag)> GetByLatestVersionAsync(
            string dataType,
            string assetClass,
            string assetId,
            string region,
            DateOnly asOfDate,
            string documentType)
        {
            _logger.LogInformation(
                "Retrieving latest version of market data: DataType={DataType}, AssetClass={AssetClass}, AssetId={AssetId}, Region={Region}, AsOf={AsOf}, DocType={DocType}",
                dataType, assetClass, assetId, region, asOfDate, documentType);

            var predicate = BuildMarketDataPredicate(dataType, assetClass, assetId, region, asOfDate, documentType);
            return await _versioning.GetByLatestVersionAsync(predicate);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FxSpotPriceData>> QueryAsync(
            string dataType,
            string assetClass,
            string? assetId = null,
            DateOnly? fromDate = null,
            DateOnly? toDate = null)
        {
            _logger.LogInformation(
                "Querying market data: DataType={DataType}, AssetClass={AssetClass}, AssetId={AssetId}, FromDate={FromDate}, ToDate={ToDate}",
                dataType, assetClass, assetId ?? "any", fromDate, toDate);

            var predicate = BuildRangeQueryPredicate(dataType, assetClass, assetId, fromDate, toDate);
            return await _repository.QueryAsync(predicate, cancellationToken: default);
        }

        /// <inheritdoc/>
        public async Task<FxSpotPriceRate> GetLatestExchangeRateAsync(
            string baseCurrency,
            string quoteCurrency,
            DateOnly asOfDate,
            CancellationToken cancellationToken = default)
        {
            var predicate = BuildCurrencyPairPredicate(baseCurrency, quoteCurrency, asOfDate);
            var (entity, _) = await _versioning.GetByLatestVersionAsync(predicate, cancellationToken);
            return FxSpotPriceRate.FromEntity(entity)!;
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

        private static Expression<Func<FxSpotPriceData, bool>> BuildCurrencyPairPredicate(
            string baseCurrency,
            string quoteCurrency,
            DateOnly asOfDate)
        {
            return MarketDataQueryBuilder<FxSpotPriceData>
                .ForCurrencyPair(baseCurrency, quoteCurrency)
                .WithAsOfDate(asOfDate)
                .Build();
        }
    }
}
