using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using vv.Domain.Events;
using vv.Domain.Models;
using vv.Domain.Repositories;
using vv.Domain.Repositories.Components;

namespace vv.Data.Repositories
{
    /// <summary>
    /// Base implementation of IRepository that is database-agnostic
    /// </summary>
    public abstract class BaseRepository<T> : IRepository<T> where T : IMarketDataEntity
    {
        protected readonly IEventPublisher? _eventPublisher;

        protected BaseRepository(IEventPublisher? eventPublisher = null)
        {
            _eventPublisher = eventPublisher;
        }

        // Abstract methods that concrete implementations must provide
        public abstract Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        public abstract Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default);
        public abstract Task<IEnumerable<T>> GetAllAsync(bool includeSoftDeleted = false, CancellationToken cancellationToken = default);
        public abstract Task<IEnumerable<T>> QueryAsync(Expression<Func<T, bool>> predicate, bool includeSoftDeleted = false, CancellationToken cancellationToken = default);
        public abstract Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
        public abstract Task<(IEnumerable<T> Items, string? ContinuationToken)> GetPagedAsync(int pageSize, string? continuationToken = null, bool includeSoftDeleted = false, CancellationToken cancellationToken = default);
        public abstract Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);
        public abstract Task<T> UpdateAsync(T entity, string? etag = null, CancellationToken cancellationToken = default);
        public abstract Task<T> UpsertAsync(T entity, CancellationToken cancellationToken = default);
        public abstract Task<int> BulkInsertAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        public abstract Task<bool> DeleteAsync(string id, bool soft = false, CancellationToken cancellationToken = default);
        public abstract Task<int> PurgeSoftDeletedAsync(CancellationToken cancellationToken = default);

        public async Task<T> GetByIdOrThrowAsync(string id, CancellationToken cancellationToken = default)
        {
            var entity = await GetByIdAsync(id, cancellationToken);
            if (entity == null)
                throw new EntityNotFoundException(typeof(T).Name, id);
            return entity;
        }

        protected async Task PublishEntityCreatedEventAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (_eventPublisher != null)
            {
                await _eventPublisher.PublishAsync(new EntityCreatedEvent<T>(entity), cancellationToken: cancellationToken);
            }
        }

        protected async Task PublishEntityUpdatedEventAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (_eventPublisher != null)
            {
                await _eventPublisher.PublishAsync(new EntityUpdatedEvent<T>(entity), cancellationToken: cancellationToken);
            }
        }

        protected async Task PublishEntityDeletedEventAsync(string id, CancellationToken cancellationToken = default)
        {
            if (_eventPublisher != null)
            {
                await _eventPublisher.PublishAsync(new EntityDeletedEvent<T>(id), cancellationToken: cancellationToken);
            }
        }
    }
}
