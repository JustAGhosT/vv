using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using vv.Domain.Events;
using vv.Domain.Models;
using vv.Domain.Repositories;

namespace vv.Data.Repositories
{
    /// <summary>
    /// Base implementation for versioned repositories
    /// </summary>
    public abstract class BaseVersionedRepository<T> : BaseRepository<T>, IVersionedRepository<T>
        where T : class, IMarketDataEntity, IVersionedEntity
    {
        protected BaseVersionedRepository(IEventPublisher? eventPublisher = null)
            : base(eventPublisher)
        {
        }

        // IVersionedRepository<T> methods
        public abstract Task<int> GetNextVersionAsync(
            Expression<Func<T, bool>> keyPredicate,
            CancellationToken cancellationToken = default);

        public abstract Task<(T? Result, string? ETag)> GetByLatestVersionAsync(
            Expression<Func<T, bool>> keyPredicate,
            CancellationToken cancellationToken = default);

        public abstract Task<(T? Result, string? ETag)> GetBySpecifiedVersionAsync(
            Expression<Func<T, bool>> keyPredicate,
            int version,
            CancellationToken cancellationToken = default);

        public abstract Task<IEnumerable<T>> GetAllVersionsAsync(
            string baseId,
            CancellationToken cancellationToken = default);

        public abstract Task<T> SaveVersionedEntityAsync(
            T entity,
            Expression<Func<T, bool>> keyPredicate,
            CancellationToken cancellationToken = default);

        // Helper methods
        protected Expression<Func<T, bool>> BuildKeyPredicate(
            string dataType, string assetClass, string assetId,
            string region, DateOnly asOfDate, string documentType)
        {
            return e => e.DataType == dataType &&
                      e.AssetClass == assetClass &&
                      e.AssetId == assetId &&
                      e.Region == region &&
                      e.AsOfDate == asOfDate &&
                      e.DocumentType == documentType;
        }
    }
}