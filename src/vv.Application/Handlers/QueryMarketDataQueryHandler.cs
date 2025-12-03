using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using vv.Application.Queries;
using vv.Application.Services;
using vv.Domain.Models;

namespace vv.Application.Handlers
{
    public class QueryMarketDataQueryHandler : IRequestHandler<QueryMarketDataQuery, IEnumerable<FxSpotPriceData>>
    {
        private readonly IMarketDataService _marketDataService;
        private readonly ILogger<QueryMarketDataQueryHandler> _logger;

        public QueryMarketDataQueryHandler(
            IMarketDataService marketDataService,
            ILogger<QueryMarketDataQueryHandler> logger)
        {
            _marketDataService = marketDataService ?? throw new ArgumentNullException(nameof(marketDataService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<FxSpotPriceData>> Handle(QueryMarketDataQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling QueryMarketDataQuery for AssetClass: {AssetClass}, AssetId: {AssetId}",
                request.AssetClass, request.AssetId ?? "any");

            // Capture request values to use in expression (avoid closure over request object)
            var assetClass = request.AssetClass;
            var assetId = request.AssetId?.ToLowerInvariant();
            var fromDate = request.FromDate;
            var toDate = request.ToDate;
            var region = request.Region;

            return await _marketDataService.QueryAsync(e =>
                (e.AssetClass == assetClass) &&
                (string.IsNullOrEmpty(assetId) || e.AssetId == assetId) &&
                (fromDate == null || e.AsOfDate >= fromDate) &&
                (toDate == null || e.AsOfDate <= toDate) &&
                (string.IsNullOrEmpty(region) || e.Region == region),
                cancellationToken
            );
        }
    }
}