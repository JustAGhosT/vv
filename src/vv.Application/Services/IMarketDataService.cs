using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using vv.Domain.Models;

namespace vv.Application.Services
{
    public interface IMarketDataService
    {
        /// <summary>
        /// Gets the latest market data for a specific asset
        /// </summary>
        /// <param name="dataType">Type of data (e.g., "price.spot")</param>
        /// <param name="assetClass">Asset class (e.g., "fx")</param>
        /// <param name="assetId">Asset identifier (e.g., "eurusd")</param>
        /// <param name="region">Region (e.g., "global")</param>
        /// <param name="asOfDate">Reference date</param>
        /// <param name="documentType">Document type (e.g., "official")</param>
        /// <returns>The latest market data matching the criteria</returns>
        Task<FxSpotPriceData> GetLatestMarketDataAsync(
            string dataType,
            string assetClass,
            string assetId,
            string region,
            DateOnly asOfDate,
            string documentType);

        /// <summary>
        /// Queries market data based on a predicate expression
        /// </summary>
        /// <param name="predicate">The filter predicate expression for database query translation</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of market data matching the criteria</returns>
        Task<IEnumerable<FxSpotPriceData>> QueryAsync(
            Expression<Func<FxSpotPriceData, bool>> predicate,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates new market data
        /// </summary>
        /// <param name="data">The market data to create</param>
        /// <returns>The ID of the created item</returns>
        Task<string> CreateMarketDataAsync(FxSpotPriceData data);
    }
}