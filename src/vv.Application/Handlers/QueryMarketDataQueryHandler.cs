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

            return await _marketDataService.QueryAsync(e =>
                (e.AssetClass == request.AssetClass) &&
                (string.IsNullOrEmpty(request.AssetId) || e.AssetId == request.AssetId?.ToLowerInvariant()) &&
                (request.FromDate == null || e.AsOfDate >= request.FromDate) &&
                (request.ToDate == null || e.AsOfDate <= request.ToDate) &&
                (string.IsNullOrEmpty(request.Region) || e.Region == request.Region)
            );
        }
    }
}