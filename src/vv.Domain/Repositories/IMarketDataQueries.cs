using System.Linq.Expressions;
using vv.Domain.Models;

public interface IMarketDataQueries
{
    /// <summary>
    /// Gets the latest version of market data matching the specified criteria
    /// </summary>
    Task<FxSpotPriceData?> GetLatestMarketDataAsync(
        string dataType,
        string assetClass,
        string assetId,
        string region,
        DateOnly asOfDate,
        string documentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries market data using a custom expression
    /// </summary>
    Task<IEnumerable<FxSpotPriceData>> QueryByExpressionAsync(
        Expression<Func<FxSpotPriceData, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries market data within a specified range
    /// </summary>
    Task<IEnumerable<FxSpotPriceData>> QueryByRangeAsync(
        string dataType,
        string assetClass,
        string? assetId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific version of market data
    /// </summary>
    Task<(FxSpotPriceData? Result, string? ETag)> GetBySpecifiedVersionAsync(
        string dataType,
        string assetClass,
        string assetId,
        string region,
        DateOnly asOfDate,
        string documentType,
        int version);

    /// <summary>
    /// Gets the latest version of market data
    /// </summary>
    Task<(FxSpotPriceData? Result, string? ETag)> GetByLatestVersionAsync(
        string dataType,
        string assetClass,
        string assetId,
        string region,
        DateOnly asOfDate,
        string documentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all market data that matches the specified criteria
    /// </summary>
    Task<IEnumerable<FxSpotPriceData>> QueryAsync(
        string dataType,
        string assetClass,
        string? assetId = null,
        DateOnly? fromDate = null,
        DateOnly? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the latest exchange rate for a currency pair
    /// </summary>
    Task<FxSpotPriceRate> GetLatestExchangeRateAsync(
        string baseCurrency,
        string quoteCurrency,
        DateOnly asOfDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the exchange rate history for a currency pair
    /// </summary>
    Task<IEnumerable<FxSpotPriceData>> GetExchangeRateHistoryAsync(
        string baseCurrency,
        string quoteCurrency,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default);
}