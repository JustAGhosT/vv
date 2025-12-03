using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using vv.Data.Repositories;
using vv.Domain.Models;
using vv.Domain.Repositories.Components;
using vv.Domain.Specifications;

namespace vv.Infrastructure.Repositories.Components
{
    public class VersioningComponent<T> : IVersioningCapability<T>
        where T : class, IMarketDataEntity, IVersionedEntity
    {
        private readonly Container _container;
        private readonly ILogger _logger;
        private readonly IRepository<T> _repository;
        private readonly IDataStoreAdapter<T> _dataStore;

        public VersioningComponent(
            Container container,
            ILogger logger,
            IRepository<T> repository,
            IDataStoreAdapter<T> dataStore)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _dataStore = dataStore ?? throw new ArgumentNullException(nameof(dataStore));
        }

        public async Task<int> GetNextVersionAsync(
            ISpecification<T> specification,
            CancellationToken cancellationToken = default)
        {
            var entities = await _repository.QueryAsync(
                specification.ToExpression(),
                cancellationToken: cancellationToken);

            int maxVersion = entities.Any() ? entities.Max(e => e.Version) : 0;
            return maxVersion + 1;
        }

        public async Task<int> GetNextVersionAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            var entities = await _repository.QueryAsync(
                predicate,
                cancellationToken: cancellationToken);

            int maxVersion = entities.Any() ? entities.Max(e => e.Version) : 0;
            return maxVersion + 1;
        }

        public async Task<(T? Result, string? ETag)> GetByLatestVersionAsync(
            ISpecification<T> specification,
            CancellationToken cancellationToken = default)
        {
            var entities = await _repository.QueryAsync(
                specification.ToExpression(),
                cancellationToken: cancellationToken);

            var latest = entities
                .OrderByDescending(e => e.Version)
                .FirstOrDefault();

            if (latest == null)
                return (null, null);

            var etag = await _dataStore.GetETagAsync(latest.Id, cancellationToken);
            return (latest, etag);
        }

        public async Task<(T? Result, string? ETag)> GetByLatestVersionAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            var entities = await _repository.QueryAsync(
                predicate,
                cancellationToken: cancellationToken);

            var latest = entities
                .OrderByDescending(e => e.Version)
                .FirstOrDefault();

            if (latest == null)
                return (null, null);

            var etag = await _dataStore.GetETagAsync(latest.Id, cancellationToken);
            return (latest, etag);
        }

        public async Task<(T? Result, string? ETag)> GetBySpecifiedVersionAsync(
            ISpecification<T> specification,
            int version,
            CancellationToken cancellationToken = default)
        {
            var entities = await _repository.QueryAsync(
                specification.ToExpression(),
                cancellationToken: cancellationToken);

            var entity = entities.FirstOrDefault(e => e.Version == version);

            if (entity == null)
                return (null, null);

            var etag = await _dataStore.GetETagAsync(entity.Id, cancellationToken);
            return (entity, etag);
        }

        public async Task<(T? Result, string? ETag)> GetBySpecifiedVersionAsync(
            Expression<Func<T, bool>> predicate,
            int version,
            CancellationToken cancellationToken = default)
        {
            var entities = await _repository.QueryAsync(
                predicate,
                cancellationToken: cancellationToken);

            var entity = entities.FirstOrDefault(e => e.Version == version);

            if (entity == null)
                return (null, null);

            var etag = await _dataStore.GetETagAsync(entity.Id, cancellationToken);
            return (entity, etag);
        }

        public async Task<IEnumerable<T>> GetAllVersionsAsync(
            string baseId,
            CancellationToken cancellationToken = default)
        {
            return await _repository.QueryAsync(
                e => e.BaseVersionId == baseId,
                cancellationToken: cancellationToken);
        }

        public async Task<T> SaveVersionedEntityAsync(
            T entity,
            ISpecification<T> specification,
            CancellationToken cancellationToken = default)
        {
            int nextVersion = await GetNextVersionAsync(specification, cancellationToken);
            entity.Version = nextVersion;

            // Id is calculated from entity properties (DataType, AssetClass, AssetId, etc.)
            // No need to set it manually

            return await _repository.CreateAsync(entity, cancellationToken);
        }

        public async Task<T> SaveVersionedEntityAsync(
            T entity,
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            int nextVersion = await GetNextVersionAsync(predicate, cancellationToken);
            entity.Version = nextVersion;

            // Id is calculated from entity properties (DataType, AssetClass, AssetId, etc.)
            // No need to set it manually

            return await _repository.CreateAsync(entity, cancellationToken);
        }
    }
}