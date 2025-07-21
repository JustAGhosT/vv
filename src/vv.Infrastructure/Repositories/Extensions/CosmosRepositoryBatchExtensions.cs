using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using vv.Domain.Models;

namespace vv.Infrastructure.Repositories.Extensions
{
    /// <summary>
    /// Extension methods for batch operations on CosmosRepository
    /// </summary>
    public static class CosmosRepositoryBatchExtensions
    {
        /// <summary>
        /// Adds multiple entities in parallel with controlled concurrency
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="repository">Repository instance</param>
        /// <param name="items">Collection of entities to add</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>Collection of added entities</returns>
        public static async Task<IEnumerable<T>> AddSequentialAsync<T>(
            this CosmosRepository<T> repository,
            IEnumerable<T> items,
            CancellationToken cancellationToken = default)
            where T : class, IMarketDataEntity
        {
            // Use controlled parallelism to avoid overwhelming the database
            const int maxConcurrency = 10;
            var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
            
            var tasks = items.Select(async item =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    return await repository.CreateAsync(item, cancellationToken);
                }
                finally
                {
                    semaphore.Release();
                }
            });
            
            var results = await Task.WhenAll(tasks);
            semaphore.Dispose();
            return results;
        }

        /// <summary>
        /// Updates multiple entities in sequence
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="repository">Repository instance</param>
        /// <param name="items">Collection of entities to update</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>Collection of updated entities</returns>
        public static async Task<IEnumerable<T>> UpdateSequentialAsync<T>(
            this CosmosRepository<T> repository,
            IEnumerable<T> items,
            CancellationToken cancellationToken = default)
            where T : class, IMarketDataEntity
        {
            var results = new List<T>();
            foreach (var item in items)
            {
                var result = await repository.UpdateAsync(item, cancellationToken);
                results.Add(result);
            }
            return results;
        }

        /// <summary>
        /// Deletes multiple entities by ID in sequence
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="repository">Repository instance</param>
        /// <param name="ids">Collection of entity IDs to delete</param>
        /// <param name="useSoftDelete">Whether to use soft delete</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>Collection of deletion results (true if deleted)</returns>
        public static async Task<IEnumerable<bool>> DeleteSequentialAsync<T>(
            this CosmosRepository<T> repository,
            IEnumerable<string> ids,
            bool useSoftDelete = true,
            CancellationToken cancellationToken = default)
            where T : class, IMarketDataEntity
        {
            var results = new List<bool>();
            foreach (var id in ids)
            {
                try
                {
                    var result = await repository.DeleteAsync(id, useSoftDelete, cancellationToken);
                    results.Add(result);
                }
                catch
                {
                    results.Add(false); // Indicate failure for this ID
                }
            }
            return results;
        }
    }
}