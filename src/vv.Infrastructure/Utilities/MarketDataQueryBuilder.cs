using System;
using System.Linq.Expressions;
using vv.Data.Utilities;
using vv.Domain.Models;

namespace vv.Infrastructure.Utilities
{
    /// <summary>
    /// Builder class for creating market data query predicates
    /// </summary>
    public class MarketDataQueryBuilder<T> where T : IMarketDataEntity
    {
        private Expression<Func<T, bool>> _predicate = e => true;

        /// <summary>
        /// Creates a specification for market data with all common parameters
        /// </summary>
        public static MarketDataQueryBuilder<T> ForMarketData(
            string dataType,
            string assetClass,
            string assetId,
            string region,
            DateOnly asOfDate,
            string documentType)
        {
            return new MarketDataQueryBuilder<T>()
                .WithDataType(dataType)
                .WithAssetClass(assetClass)
                .WithAssetId(assetId)
                .WithRegion(region)
                .WithAsOfDate(asOfDate)
                .WithDocumentType(documentType);
        }

        /// <summary>
        /// Creates a specification for range queries with required and optional parameters
        /// </summary>
        public static MarketDataQueryBuilder<T> ForRangeQuery(
            string dataType,
            string assetClass,
            string? assetId = null,
            DateOnly? fromDate = null,
            DateOnly? toDate = null)
        {
            var builder = new MarketDataQueryBuilder<T>()
                .WithDataType(dataType)
                .WithAssetClass(assetClass);

            if (!string.IsNullOrEmpty(assetId))
                builder.WithAssetId(assetId);

            if (fromDate.HasValue)
                builder.WithFromDate(fromDate.Value);

            if (toDate.HasValue)
                builder.WithToDate(toDate.Value);

            return builder;
        }

        /// <summary>
        /// Creates a specification for currency pair queries with FX data type and spot asset class
        /// </summary>
        /// <param name="baseCurrency">The base currency code (e.g., "USD")</param>
        /// <param name="quoteCurrency">The quote currency code (e.g., "EUR")</param>
        /// <exception cref="ArgumentException">Thrown when baseCurrency or quoteCurrency is null or whitespace</exception>
        public static MarketDataQueryBuilder<T> ForCurrencyPair(
            string baseCurrency,
            string quoteCurrency)
        {
            if (string.IsNullOrWhiteSpace(baseCurrency))
                throw new ArgumentException("Base currency cannot be null or whitespace", nameof(baseCurrency));
            if (string.IsNullOrWhiteSpace(quoteCurrency))
                throw new ArgumentException("Quote currency cannot be null or whitespace", nameof(quoteCurrency));

            string assetId = $"{baseCurrency}{quoteCurrency}".ToLowerInvariant();
            return new MarketDataQueryBuilder<T>()
                .WithDataType(DataTypes.PriceSpot)
                .WithAssetClass(AssetClass.Fx)
                .WithAssetId(assetId);
        }

        /// <summary>
        /// Adds a data type filter
        /// </summary>
        public MarketDataQueryBuilder<T> WithDataType(string dataType)
        {
            _predicate = ExpressionCombiner.CombinePredicates(_predicate, e => e.DataType == dataType);
            return this;
        }

        /// <summary>
        /// Adds an asset class filter
        /// </summary>
        public MarketDataQueryBuilder<T> WithAssetClass(string assetClass)
        {
            _predicate = ExpressionCombiner.CombinePredicates(_predicate, e => e.AssetClass == assetClass);
            return this;
        }

        /// <summary>
        /// Adds an asset ID filter (case-insensitive)
        /// </summary>
        public MarketDataQueryBuilder<T> WithAssetId(string assetId)
        {
            string normalizedAssetId = assetId.ToLowerInvariant();
            _predicate = ExpressionCombiner.CombinePredicates(_predicate, e => e.AssetId == normalizedAssetId);
            return this;
        }

        /// <summary>
        /// Adds a region filter
        /// </summary>
        public MarketDataQueryBuilder<T> WithRegion(string region)
        {
            _predicate = ExpressionCombiner.CombinePredicates(_predicate, e => e.Region == region);
            return this;
        }

        /// <summary>
        /// Adds a document type filter
        /// </summary>
        public MarketDataQueryBuilder<T> WithDocumentType(string documentType)
        {
            _predicate = ExpressionCombiner.CombinePredicates(_predicate, e => e.DocumentType == documentType);
            return this;
        }

        /// <summary>
        /// Adds a specific date filter
        /// </summary>
        public MarketDataQueryBuilder<T> WithAsOfDate(DateOnly asOfDate)
        {
            _predicate = ExpressionCombiner.CombinePredicates(_predicate, e => e.AsOfDate == asOfDate);
            return this;
        }

        /// <summary>
        /// Adds a minimum date filter
        /// </summary>
        public MarketDataQueryBuilder<T> WithFromDate(DateOnly fromDate)
        {
            _predicate = ExpressionCombiner.CombinePredicates(_predicate, e => e.AsOfDate >= fromDate);
            return this;
        }

        /// <summary>
        /// Adds a maximum date filter
        /// </summary>
        public MarketDataQueryBuilder<T> WithToDate(DateOnly toDate)
        {
            _predicate = ExpressionCombiner.CombinePredicates(_predicate, e => e.AsOfDate <= toDate);
            return this;
        }

        /// <summary>
        /// Builds the final predicate
        /// </summary>
        public Expression<Func<T, bool>> Build() => _predicate;

        // Static helper methods for commonly used predicates to reduce code duplication

        /// <summary>
        /// Builds a predicate for market data with all common parameters
        /// </summary>
        public static Expression<Func<T, bool>> BuildMarketDataPredicate(
            string dataType,
            string assetClass,
            string assetId,
            string region,
            DateOnly asOfDate,
            string documentType)
        {
            return ForMarketData(dataType, assetClass, assetId, region, asOfDate, documentType).Build();
        }

        /// <summary>
        /// Builds a predicate for range queries with required and optional parameters
        /// </summary>
        public static Expression<Func<T, bool>> BuildRangeQueryPredicate(
            string dataType,
            string assetClass,
            string? assetId = null,
            DateOnly? fromDate = null,
            DateOnly? toDate = null)
        {
            return ForRangeQuery(dataType, assetClass, assetId, fromDate, toDate).Build();
        }

        /// <summary>
        /// Builds a predicate for currency pair queries with specific date
        /// </summary>
        public static Expression<Func<T, bool>> BuildCurrencyPairPredicate(
            string baseCurrency,
            string quoteCurrency,
            DateOnly asOfDate)
        {
            return ForCurrencyPair(baseCurrency, quoteCurrency).WithAsOfDate(asOfDate).Build();
        }
    }
}