using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using vv.Domain.Events;
using vv.Domain.Validation;
using vv.Domain.Models;
using vv.Domain.Repositories.Components;
using vv.Core.Validation;

namespace vv.Api.Services
{
    public interface IMarketDataService<T> where T : IMarketDataEntity
    {
        Task<string> PublishMarketDataAsync(T marketData, CancellationToken cancellationToken = default);
        Task<T?> GetLatestMarketDataAsync(
            string dataType, string assetClass, string assetId, string region,
            DateOnly asOfDate, string documentType,
            CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> QueryMarketDataAsync(
            string dataType, string assetClass, string? assetId = null,
            DateTime? fromDate = null, DateTime? toDate = null,
            CancellationToken cancellationToken = default);
    }

    public class MarketDataService<T> : IMarketDataService<T> where T : class, IMarketDataEntity
    {
        private readonly IRepository<T> _repository;
        private readonly IEventPublisher _eventPublisher;
        private readonly IValidator<T> _validator;

        public MarketDataService(
            IRepository<T> repository,
            IEventPublisher eventPublisher,
            IValidator<T> validator)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<string> PublishMarketDataAsync(T marketData, CancellationToken cancellationToken = default)
        {
            if (marketData == null)
                throw new ArgumentNullException(nameof(marketData));
            
            var validationResult = await _validator.ValidateAsync(marketData, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var result = await _repository.CreateAsync(marketData, cancellationToken);
            await _eventPublisher.PublishAsync(new EntityCreatedEvent<T>(result), cancellationToken: cancellationToken);

            return result?.Id ?? string.Empty;
        }

        public Task<T?> GetLatestMarketDataAsync(
            string dataType, string assetClass, string assetId, string region,
            DateOnly asOfDate, string documentType,
            CancellationToken cancellationToken = default)
        {
            // TODO: Implement using repository query methods
            throw new NotImplementedException("GetLatestMarketDataAsync is not yet implemented");
        }

        public Task<IEnumerable<T>> QueryMarketDataAsync(
            string dataType, string assetClass, string? assetId = null,
            DateTime? fromDate = null, DateTime? toDate = null,
            CancellationToken cancellationToken = default)
        {
            // TODO: Implement using repository query methods
            throw new NotImplementedException("QueryMarketDataAsync is not yet implemented");
        }
    }
}