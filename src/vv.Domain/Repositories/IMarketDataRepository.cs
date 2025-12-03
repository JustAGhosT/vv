using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using vv.Domain.Models;

namespace vv.Domain.Repositories
{
    public interface IMarketDataRepository
    {
        Task<FxSpotPriceData?> GetLatestMarketDataAsync(
            string dataType,
            string assetClass,
            string assetId,
            string region,
            DateOnly asOfDate,
            string documentType,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<FxSpotPriceData>> QueryAsync(Func<FxSpotPriceData, bool> predicate);

        Task<FxSpotPriceData> CreateMarketDataAsync(FxSpotPriceData data);

        Task<FxSpotPriceData> UpdateMarketDataAsync(FxSpotPriceData data);

        Task<bool> DeleteMarketDataAsync(string id);

        Task<IEnumerable<FxSpotPriceData>> QueryByRangeAsync(
            string dataType,
            string assetClass,
            string? assetId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            CancellationToken cancellationToken = default);
    }
}
