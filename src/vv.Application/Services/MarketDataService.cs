using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using vv.Domain.Models;
using vv.Domain.Repositories;
using vv.Domain.Services;

namespace vv.Application.Services
{
    public class MarketDataService : IMarketDataService
    {
        private readonly IMarketDataRepository _repository;
        private readonly ILogger<MarketDataService> _logger;

        public MarketDataService(IMarketDataRepository repository, ILogger<MarketDataService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> PublishMarketDataAsync<T>(T marketData) where T : IMarketDataEntity
        {
            _logger.LogInformation("Publishing market data for {AssetId}", marketData.AssetId);

            // Currently we only support FxSpotPriceData
            if (marketData is FxSpotPriceData fxSpotData)
            {
                var result = await _repository.CreateMarketDataAsync(fxSpotData);
                _logger.LogInformation("Successfully published market data with ID {Id}", result.Id);
                return result.Id;
            }

            throw new NotSupportedException($"Market data type {typeof(T).Name} is not supported");
        }

        public async Task<bool> UpdateMarketDataAsync<T>(T marketData) where T : IMarketDataEntity
        {
            _logger.LogInformation("Updating market data for {AssetId}", marketData.AssetId);

            // Currently we only support FxSpotPriceData
            if (marketData is FxSpotPriceData fxSpotData)
            {
                await _repository.UpdateMarketDataAsync(fxSpotData);
                _logger.LogInformation("Successfully updated market data with ID {Id}", marketData.Id);
                return true;
            }

            throw new NotSupportedException($"Market data type {typeof(T).Name} is not supported");
        }

        public async Task<bool> DeleteMarketDataAsync<T>(string id) where T : IMarketDataEntity
        {
            _logger.LogInformation("Deleting market data with ID {Id}", id);

            var result = await _repository.DeleteMarketDataAsync(id);

            if (result)
            {
                _logger.LogInformation("Successfully deleted market data with ID {Id}", id);
            }
            else
            {
                _logger.LogWarning("Failed to delete market data with ID {Id}", id);
            }

            return result;
        }

        public async Task<T?> GetLatestMarketDataAsync<T>(
            string dataType,
            string assetClass,
            string assetId,
            string region,
            DateOnly asOfDate,
            string documentType) where T : IMarketDataEntity
        {
            _logger.LogInformation("Getting latest market data for {AssetId} as of {AsOfDate}", assetId, asOfDate);

            // Currently we only support FxSpotPriceData
            if (typeof(T) == typeof(FxSpotPriceData))
            {
                var result = await _repository.GetLatestMarketDataAsync(
                    dataType, assetClass, assetId, region, asOfDate, documentType);

                if (result == null)
                {
                    _logger.LogWarning("No market data found for {AssetId} as of {AsOfDate}", assetId, asOfDate);
                }

                return (T?)(object?)result;
            }

            throw new NotSupportedException($"Market data type {typeof(T).Name} is not supported");
        }

        public async Task<IEnumerable<T>> QueryMarketDataAsync<T>(MarketDataQueryFilter filter) where T : IMarketDataEntity
        {
            _logger.LogInformation("Querying market data with filter");

            // Currently we only support FxSpotPriceData
            if (typeof(T) == typeof(FxSpotPriceData))
            {
                // Transform the filter into a query for the repository
                // The IMarketDataRepository might need a QueryByRangeAsync method implementation
                var result = await _repository.QueryByRangeAsync(
                    filter.DataType,
                    filter.AssetClass,
                    filter.AssetId,
                    filter.FromDate.HasValue ? filter.FromDate.Value.ToDateTime(TimeOnly.MinValue) : null,
                    filter.ToDate.HasValue ? filter.ToDate.Value.ToDateTime(TimeOnly.MaxValue) : null);

                return result.Cast<T>();
            }

            throw new NotSupportedException($"Market data type {typeof(T).Name} is not supported");
        }


        public async Task<FxSpotPriceData> GetLatestMarketDataAsync(
            string dataType,
            string assetClass,
            string assetId,
            string region,
            DateOnly asOfDate,
            string documentType)
        {
            _logger.LogInformation("Getting latest market data for {AssetId} as of {AsOfDate}", assetId, asOfDate);
            
            var result = await _repository.GetLatestMarketDataAsync(
                dataType, assetClass, assetId, region, asOfDate, documentType);
            
            if (result == null)
            {
                _logger.LogWarning("No market data found for {AssetId} as of {AsOfDate}", assetId, asOfDate);
                throw new InvalidOperationException(
                    $"No market data found for asset '{assetId}' as of {asOfDate}");
            }
            
            return result;
        }

        public async Task<IEnumerable<FxSpotPriceData>> QueryAsync(
            Expression<Func<FxSpotPriceData, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Querying market data with predicate expression");
            
            var result = await _repository.QueryAsync(predicate, cancellationToken);
            
            return result;
        }

        public async Task<string> CreateMarketDataAsync(FxSpotPriceData data)
        {
            _logger.LogInformation("Creating market data for {AssetId}", data.AssetId);
            
            var result = await _repository.CreateMarketDataAsync(data);
            _logger.LogInformation("Successfully created market data with ID {Id}", result.Id);
            
            return result.Id;
        }
    }
}
