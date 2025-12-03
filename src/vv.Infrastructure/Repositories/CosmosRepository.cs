using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using vv.Data.Repositories;
using vv.Domain.Models;

namespace vv.Infrastructure.Repositories
{
    /// <summary>
    /// Cosmos DB specific implementation of BaseRepository
    /// </summary>
    /// <typeparam name="T">The type of entity managed by this repository</typeparam>
    public abstract partial class CosmosRepository<T> : BaseRepository<T> where T : class, IMarketDataEntity
    {
        protected readonly Container _container;
        protected readonly ILogger _logger;
        protected readonly Func<T, string> _partitionKeyResolver;

        protected CosmosRepository(
            Container container,
            ILogger logger,
            vv.Domain.Events.IEventPublisher? eventPublisher = null,
            Func<T, string>? partitionKeyResolver = null)
            : base(eventPublisher)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _partitionKeyResolver = partitionKeyResolver ?? (e => e.AssetId);
        }

        /// <summary>
        /// Gets the partition key for an entity
        /// </summary>
        protected PartitionKey GetPartitionKey(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return new PartitionKey(_partitionKeyResolver(entity));
        }

        /// <summary>
        /// Executes a query with the provided predicate
        /// </summary>
        public async Task<IEnumerable<T>> QueryAsync(
            Expression<Func<T, bool>> predicate,
            int? maxItems = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Executing query with expression on {EntityType}", typeof(T).Name);
            try
            {
                // Convert expression to query definition
                var query = _container.GetItemLinqQueryable<T>()
                    .Where(predicate)
                    .ToFeedIterator();

                var results = new List<T>();

                // Retrieve results in pages
                while (query.HasMoreResults)
                {
                    var response = await query.ReadNextAsync(cancellationToken);
                    results.AddRange(response);

                    // Check if we've reached the maximum number of items
                    if (maxItems.HasValue && results.Count >= maxItems.Value)
                    {
                        results = results.Take(maxItems.Value).ToList();
                        break;
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing query on {EntityType}", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// Gets an entity by its ID - override from BaseRepository
        /// </summary>
        public override async Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("GetByIdAsync called with null or empty ID");
                return null;
            }

            try
            {
                _logger.LogDebug("GetByIdAsync called for ID {Id}", id);
                var partitionKey = DeterminePartitionKey(id);

                try
                {
                    var response = await _container.ReadItemAsync<T>(id, partitionKey, cancellationToken: cancellationToken);
                    _logger.LogDebug("Got response with status code {StatusCode}", response.StatusCode);

                    // Check soft delete
                    if (response.Resource is ISoftDeletable sd && sd.IsDeleted)
                    {
                        _logger.LogDebug("Entity {Id} is marked as deleted, returning null", id);
                        return null;
                    }

                    return response.Resource;
                }
                catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogInformation("Entity with ID {Id} not found", id);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByIdAsync for ID {Id}: {ErrorMessage}", id, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Checks if an entity with the given ID exists - override from BaseRepository
        /// </summary>
        public override async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }

            try
            {
                _logger.LogDebug("ExistsAsync called for ID {Id}", id);
                var partitionKey = DeterminePartitionKey(id);

                try
                {
                    var response = await _container.ReadItemAsync<T>(id, partitionKey, cancellationToken: cancellationToken);

                    // Check soft delete
                    if (response.Resource is ISoftDeletable sd && sd.IsDeleted)
                    {
                        return false;
                    }

                    return true;
                }
                catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ExistsAsync for ID {Id}: {ErrorMessage}", id, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Gets all entities - override from BaseRepository
        /// </summary>
        public override async Task<IEnumerable<T>> GetAllAsync(
            bool includeSoftDeleted = false,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("GetAllAsync called for {EntityType}", typeof(T).Name);

            try
            {
                // Query all items
                var query = _container.GetItemQueryIterator<T>();
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
                _logger.LogError(ex, "Error in GetAllAsync for {EntityType}", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// Queries entities - override from BaseRepository
        /// </summary>
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
        /// Counts entities - override from BaseRepository
        /// </summary>
        public override async Task<int> CountAsync(
            Expression<Func<T, bool>>? predicate = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("CountAsync called for {EntityType}", typeof(T).Name);

            try
            {
                // Base query
                IQueryable<T> query = _container.GetItemLinqQueryable<T>();

                // Apply predicate if provided
                if (predicate != null)
                {
                    query = query.Where(predicate);
                }

                // Execute count query
                var count = await query.CountAsync();
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CountAsync for {EntityType}", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// Gets paged results - override from BaseRepository
        /// </summary>
        public override async Task<(IEnumerable<T> Items, string? ContinuationToken)> GetPagedAsync(
            int pageSize,
            string? continuationToken = null,
            bool includeSoftDeleted = false,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("GetPagedAsync called for {EntityType} with pageSize {PageSize}", typeof(T).Name, pageSize);

            try
            {
                var options = new QueryRequestOptions
                {
                    MaxItemCount = pageSize
                };

                var queryDef = new QueryDefinition("SELECT * FROM c");

                var resultSet = _container.GetItemQueryIterator<T>(
                    queryDef,
                    continuationToken,
                    options);

                var response = await resultSet.ReadNextAsync(cancellationToken);
                var items = response.ToList();

                // Filter out soft-deleted items if needed
                if (!includeSoftDeleted)
                {
                    items = items.Where(e => !(e is ISoftDeletable sd) || !sd.IsDeleted).ToList();
                }

                return (items, response.ContinuationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPagedAsync for {EntityType}", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// Retrieves an item with its ETag
        /// </summary>
        protected async Task<(T? Entity, string? ETag)> GetItemWithETagAsync(
            string id,
            PartitionKey partitionKey,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _container.ReadItemAsync<T>(
                    id,
                    partitionKey,
                    cancellationToken: cancellationToken);
                return (response.Resource, response.ETag);
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogInformation("Entity with ID {Id} not found", id);
                return (null, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving item with ETag for ID {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Creates a new entity in the container - override from BaseRepository
        /// </summary>
        public override async Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _logger.LogDebug("Creating {EntityType} with ID {Id}", typeof(T).Name, entity.Id);

            try
            {
                var response = await _container.CreateItemAsync(
                    entity,
                    GetPartitionKey(entity),
                    cancellationToken: cancellationToken);

                await PublishEntityCreatedEventAsync(response.Resource, cancellationToken);

                return response.Resource;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating {EntityType} with ID {Id}", typeof(T).Name, entity.Id);
                throw;
            }
        }

        /// <summary>
        /// Updates an existing entity in the container - override from BaseRepository
        /// </summary>
        public override async Task<T> UpdateAsync(T entity, string? etag = null, CancellationToken cancellationToken = default)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _logger.LogDebug("Updating {EntityType} with ID {Id}", typeof(T).Name, entity.Id);

            try
            {
                var options = new ItemRequestOptions();
                if (!string.IsNullOrEmpty(etag))
                {
                    options.IfMatchEtag = etag;
                }

                var response = await _container.ReplaceItemAsync(
                    entity,
                    entity.Id,
                    GetPartitionKey(entity),
                    options,
                    cancellationToken);

                await PublishEntityUpdatedEventAsync(response.Resource, cancellationToken);

                return response.Resource;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating {EntityType} with ID {Id}", typeof(T).Name, entity.Id);
                throw;
            }
        }

        /// <summary>
        /// Upserts an entity in the container - override from BaseRepository
        /// </summary>
        public override async Task<T> UpsertAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _logger.LogDebug("Upserting {EntityType} with ID {Id}", typeof(T).Name, entity.Id);

            try
            {
                var response = await _container.UpsertItemAsync(
                    entity,
                    GetPartitionKey(entity),
                    cancellationToken: cancellationToken);

                await PublishEntityUpdatedEventAsync(response.Resource, cancellationToken);

                return response.Resource;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error upserting {EntityType} with ID {Id}", typeof(T).Name, entity.Id);
                throw;
            }
        }

        /// <summary>
        /// Bulk inserts entities - override from BaseRepository
        /// </summary>
        public override async Task<int> BulkInsertAsync(
            IEnumerable<T> entities,
            CancellationToken cancellationToken = default)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            var entitiesList = entities.ToList();
            if (!entitiesList.Any())
                return 0;

            _logger.LogDebug("Bulk inserting {Count} {EntityType} entities", entitiesList.Count, typeof(T).Name);

            try
            {
                var tasks = entitiesList.Select(entity =>
                    _container.CreateItemAsync(entity, GetPartitionKey(entity), cancellationToken: cancellationToken));

                await Task.WhenAll(tasks);
                return entitiesList.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk inserting {Count} {EntityType} entities",
                    entitiesList.Count, typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// Deletes an entity - override from BaseRepository
        /// </summary>
        public override async Task<bool> DeleteAsync(
            string id,
            bool soft = false,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("DeleteAsync called with null or empty ID");
                return false;
            }

            _logger.LogDebug("Deleting {EntityType} with ID {Id}", typeof(T).Name, id);

            try
            {
                // Get the entity first
                var entity = await GetByIdAsync(id, cancellationToken);
                if (entity == null)
                {
                    _logger.LogWarning("Entity {EntityType} with ID {Id} not found for deletion", typeof(T).Name, id);
                    return false;
                }

                var partitionKey = GetPartitionKey(entity);

                if (soft && entity is ISoftDeletable softDeletable)
                {
                    // Soft delete - mark as deleted and update
                    softDeletable.IsDeleted = true;
                    await _container.ReplaceItemAsync(
                        entity,
                        id,
                        partitionKey,
                        cancellationToken: cancellationToken);
                }
                else
                {
                    // Hard delete
                    await _container.DeleteItemAsync<T>(
                        id,
                        partitionKey,
                        cancellationToken: cancellationToken);
                }

                await PublishEntityDeletedEventAsync(id, cancellationToken);

                return true;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Entity {EntityType} with ID {Id} not found for deletion", typeof(T).Name, id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting {EntityType} with ID {Id}", typeof(T).Name, id);
                throw;
            }
        }

        /// <summary>
        /// Purges soft-deleted entities - override from BaseRepository
        /// </summary>
        public override async Task<int> PurgeSoftDeletedAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Purging soft deleted {EntityType} entities", typeof(T).Name);

            try
            {
                // Query for soft-deleted entities
                var query = _container.GetItemLinqQueryable<T>()
                    .Where(e => e is ISoftDeletable && ((ISoftDeletable)e).IsDeleted)
                    .ToFeedIterator();

                var results = new List<T>();

                while (query.HasMoreResults)
                {
                    var response = await query.ReadNextAsync(cancellationToken);
                    results.AddRange(response);
                }

                // Delete each soft-deleted entity
                var tasks = results.Select(entity =>
                    _container.DeleteItemAsync<T>(
                        entity.Id,
                        GetPartitionKey(entity),
                        cancellationToken: cancellationToken));

                await Task.WhenAll(tasks);
                return results.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error purging soft deleted {EntityType} entities", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// Determines the partition key for an ID
        /// </summary>
        protected virtual PartitionKey DeterminePartitionKey(string id)
        {
            string? extractedAssetId = null;

            // Extract asset ID from the ID format (e.g., "price.spot__fx__eurusd__global__2025-05-14__official__1")
            if (id.Contains("__"))
            {
                var parts = id.Split("__");
                if (parts.Length >= 3)
                {
                    // AssetId is typically the third part in this format
                    extractedAssetId = parts[2];
                    _logger.LogDebug("Extracted asset ID '{AssetId}' from ID '{Id}'", extractedAssetId, id);
                }
            }

            // Use the extracted assetId or fall back to the ID itself
            return new PartitionKey(extractedAssetId ?? id);
        }
    }
}