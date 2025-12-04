using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq; // Add this namespace for ToFeedIterator extension method
using Microsoft.Extensions.Logging;
using vv.Data.Repositories;
using vv.Data.Utilities;
using vv.Domain.Events;
using vv.Domain.Models;
using vv.Infrastructure.Utilities;

namespace vv.Infrastructure.Repositories
{
    /// <summary>
    /// Cosmos DB specific implementation of BaseVersionedRepository
    /// </summary>
    /// <typeparam name="T">The type of entity managed by this repository</typeparam>
    public abstract class VersionedCosmosRepository<T> : BaseVersionedRepository<T>
        where T : class, IMarketDataEntity, IVersionedEntity
    {
        protected readonly Container _container;
        protected readonly ILogger _logger;
        protected readonly Func<T, string> _partitionKeyResolver;
        protected readonly IEntityIdGenerator<T>? _idGenerator;

        protected VersionedCosmosRepository(
            Container container,
            ILogger logger,
            IEntityIdGenerator<T>? idGenerator = null,
            IEventPublisher? eventPublisher = null,
            Func<T, string>? partitionKeyResolver = null)
            : base(eventPublisher)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _idGenerator = idGenerator;
            _partitionKeyResolver = partitionKeyResolver ?? (e => e.AssetId);
        }

        /// <summary>
        /// Gets the next version number for an entity based on the provided predicate
        /// </summary>
        public override async Task<int> GetNextVersionAsync(
            Expression<Func<T, bool>> keyPredicate,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting next version for {EntityType} with predicate", typeof(T).Name);

            try
            {
                // More efficient approach using direct SQL query with MAX aggregate
                var sqlQuery = new QueryDefinition(
                    "SELECT VALUE MAX(c.version) FROM c WHERE c.entityType = @entityType");
                sqlQuery.WithParameter("@entityType", typeof(T).Name);

                var query = _container.GetItemQueryIterator<int?>(sqlQuery);
                if (query.HasMoreResults)
                {
                    var response = await query.ReadNextAsync(cancellationToken);
                    var maxVersion = response.Resource.FirstOrDefault() ?? 0;
                    return maxVersion + 1;
                }

                // Fallback to the original implementation if the optimized query fails
                var entities = await QueryAsync(keyPredicate, includeSoftDeleted: true, cancellationToken);
                var maxVersionFallback = entities
                    .Select(e => e.Version)
                    .DefaultIfEmpty(0)
                    .Max();

                return maxVersionFallback + 1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting next version for {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public override async Task<(T? Result, string? ETag)> GetByLatestVersionAsync(
            Expression<Func<T, bool>> keyPredicate,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting latest version for {EntityType} with predicate", typeof(T).Name);

            try
            {
                // Find all entities matching the key predicate
                var entities = await QueryAsync(keyPredicate, includeSoftDeleted: false, cancellationToken);

                // Get the entity with the highest version
                var latestEntity = entities
                    .OrderByDescending(e => e.Version)
                    .FirstOrDefault();

                if (latestEntity == null || string.IsNullOrEmpty(latestEntity.Id))
                {
                    return (null, null);
                }

                // Get ETag if entity exists
                try
                {
                    var response = await _container.ReadItemAsync<T>(
                        latestEntity.Id,
                        GetPartitionKey(latestEntity),
                        cancellationToken: cancellationToken);

                    return (response.Resource, response.ETag);
                }
                catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Entity with ID {Id} not found when retrieving ETag", latestEntity.Id);
                    return (latestEntity, null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving latest version for {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public override async Task<(T? Result, string? ETag)> GetBySpecifiedVersionAsync(
            Expression<Func<T, bool>> keyPredicate,
            int version,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting version {Version} for {EntityType} with predicate",
                version, typeof(T).Name);

            try
            {
                // Combine the key predicate with the version predicate
                Expression<Func<T, bool>> versionPredicate = e => e.Version == version;
                var combinedPredicate = ExpressionCombiner.CombinePredicates(keyPredicate, versionPredicate);

                // Find the entity with the specified version
                var entities = await QueryAsync(combinedPredicate, includeSoftDeleted: false, cancellationToken);
                var entity = entities.FirstOrDefault();

                if (entity == null || string.IsNullOrEmpty(entity.Id))
                {
                    return (null, null);
                }

                // Get ETag if entity exists
                try
                {
                    var response = await _container.ReadItemAsync<T>(
                        entity.Id,
                        GetPartitionKey(entity),
                        cancellationToken: cancellationToken);

                    return (response.Resource, response.ETag);
                }
                catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Entity with ID {Id} not found when retrieving ETag", entity.Id);
                    return (entity, null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving version {Version} for {EntityType}",
                    version, typeof(T).Name);
                throw;
            }
        }

        public override async Task<IEnumerable<T>> GetAllVersionsAsync(
            string baseId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting all versions for base ID {BaseId}", baseId);

            try
            {
                // Base ID is expected to be a prefix of the actual ID
                // The format is typically "[dataType]__[assetClass]__[assetId]__[region]__[date]__[documentType]"

                // Extract asset ID from the base ID to use as partition key
                string? partitionKey = null;
                if (baseId.Contains("__"))
                {
                    var parts = baseId.Split("__");
                    if (parts.Length >= 3)
                    {
                        partitionKey = parts[2]; // AssetId component
                    }
                }

                // Query using SQL to find all entities with IDs starting with the base ID
                QueryDefinition queryDef = new QueryDefinition(
                    "SELECT * FROM c WHERE STARTSWITH(c.id, @baseId)");
                queryDef.WithParameter("@baseId", baseId);

                var queryRequestOptions = new QueryRequestOptions();
                if (!string.IsNullOrEmpty(partitionKey))
                {
                    queryRequestOptions.PartitionKey = new PartitionKey(partitionKey);
                }

                var query = _container.GetItemQueryIterator<T>(
                    queryDef,
                    requestOptions: queryRequestOptions);

                var results = new List<T>();
                while (query.HasMoreResults)
                {
                    var response = await query.ReadNextAsync(cancellationToken);
                    results.AddRange(response);
                }

                return results.OrderBy(e => e.Version);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all versions for base ID {BaseId}", baseId);
                throw;
            }
        }

        public override async Task<T> SaveVersionedEntityAsync(
            T entity,
            Expression<Func<T, bool>> keyPredicate,
            CancellationToken cancellationToken = default)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            try
            {
                // If no version is set, determine the next version number
                if (entity.Version <= 0)
                {
                    entity.Version = await GetNextVersionAsync(keyPredicate, cancellationToken);
                    _logger.LogDebug("Assigned version {Version} to entity with ID {Id}", entity.Version, entity.Id);
                }

                // Use the UpsertAsync method to perform an upsert operation
                var response = await _container.UpsertItemAsync(
                    entity,
                    GetPartitionKey(entity),
                    cancellationToken: cancellationToken);

                await PublishEntityCreatedEventAsync(response.Resource, cancellationToken);

                return response.Resource;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving versioned entity with ID {Id}", entity.Id);
                throw;
            }
        }

        // Helper method to get partition key - reused from CosmosRepository
        protected PartitionKey GetPartitionKey(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return new PartitionKey(_partitionKeyResolver(entity));
        }

        // Implement the required base repository methods by delegating to the container
        public override async Task<IEnumerable<T>> QueryAsync(
            Expression<Func<T, bool>> predicate,
            bool includeSoftDeleted = false,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("QueryAsync called for {EntityType} with predicate", typeof(T).Name);

            try
            {
                // Query with the provided predicate
                var query = _container.GetItemLinqQueryable<T>()
                    .Where(predicate)
                    .ToFeedIterator();

                var results = new List<T>();

                while (query.HasMoreResults)
                {
                    var response = await query.ReadNextAsync(cancellationToken);

                    // Filter out soft-deleted items if needed
                    if (!includeSoftDeleted)
                    {
                        results.AddRange(response.Where(e => !(e is ISoftDeletable sd) || !sd.IsDeleted));
                    }
                    else
                    {
                        results.AddRange(response);
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in QueryAsync for {EntityType}", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// Gets the next version number for a market data entity with the specified characteristics
        /// </summary>
        public async Task<int> GetNextVersionAsync(
            string dataType, string assetClass, string assetId, string region,
            DateOnly asOfDate, string documentType,
            CancellationToken cancellationToken = default)
        {
            // Create a predicate that matches the entity without version
            Expression<Func<T, bool>> keyPredicate = e =>
                e.DataType == dataType &&
                e.AssetClass == assetClass &&
                e.AssetId == assetId &&
                e.Region == region &&
                e.AsOfDate == asOfDate &&
                e.DocumentType == documentType;

            return await GetNextVersionAsync(keyPredicate, cancellationToken);
        }

        /// <summary>
        /// Publishes an entity created event
        /// </summary>
        protected new virtual Task PublishEntityCreatedEventAsync(T entity, CancellationToken cancellationToken)
        {
            if (_eventPublisher == null || entity == null)
                return Task.CompletedTask;

            var @event = new EntityCreatedEvent<T>(entity);
            return _eventPublisher.PublishAsync(@event, cancellationToken: cancellationToken);
        }
    }
}