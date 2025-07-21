public class VersioningComponent<T> : IVersioningCapability<T> 
    where T : class, IMarketDataEntity, IVersionedEntity
{
    private readonly Container _container;
    private readonly ILogger _logger;
    private readonly IEntityIdGenerator<T>? _idGenerator;
    private readonly IRepository<T> _repository;
    private readonly IDataStoreAdapter<T> _dataStore;

    public VersioningComponent(
        Container container,
        ILogger logger,
        IRepository<T> repository,
        IDataStoreAdapter<T> dataStore,
        IEntityIdGenerator<T>? idGenerator = null)
    {
        _container = container ?? throw new ArgumentNullException(nameof(container));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _dataStore = dataStore ?? throw new ArgumentNullException(nameof(dataStore));
        _idGenerator = idGenerator;
    }

    public async Task<int> GetNextVersionAsync(
        ISpecification<T> specification, 
        CancellationToken cancellationToken = default)
    {
        // Implementation similar to current code but using specification instead of Expression
        var entities = await _repository.QueryAsync(
            specification.ToExpression(), 
            cancellationToken: cancellationToken);
            
        // Find max version using LINQ for better performance and safety
        int maxVersion = entities.Any() ? entities.Max(e => e.Version) : 0;
        
        return maxVersion + 1;
    }
    
    public async Task<(T? Result, string? ETag)> GetByLatestVersionAsync(
        ISpecification<T> specification, 
        CancellationToken cancellationToken = default)
    {
        // Implementation similar to current code but using specification
        var entities = await _repository.QueryAsync(
            specification.ToExpression(), 
            cancellationToken: cancellationToken);
            
        // Find highest version using LINQ for better performance
        var latest = entities
            .OrderByDescending(e => e.Version)
            .FirstOrDefault();
        
        if (latest == null)
            return (null, null);
            
        // Get ETag for the latest version
        var etag = await _dataStore.GetETagAsync(latest.Id, cancellationToken);
        return (latest, etag);
    }
    
    // Implement other IVersioningCapability<T> methods similarly
    
    public async Task<T> SaveVersionedEntityAsync(
        T entity, 
        ISpecification<T> specification, 
        CancellationToken cancellationToken = default)
    {
        // Implementation similar to current code but using specification
        // 1. Get next version number
        int nextVersion = await GetNextVersionAsync(specification, cancellationToken);
        
        // 2. Set the version on the entity
        entity.Version = nextVersion;
        
        // 3. Ensure ID is set correctly (using id generator if available)
        if (_idGenerator != null && string.IsNullOrEmpty(entity.Id))
        {
            entity.Id = _idGenerator.GenerateId(entity);
        }
        
        // 4. Save the entity
        return await _repository.CreateAsync(entity, cancellationToken);
    }
}