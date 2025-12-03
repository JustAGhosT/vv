using System;

namespace vv.Domain.Events
{
    public class EntityCreatedEvent<T> : DomainEvent
    {
        public T Entity { get; }

        public EntityCreatedEvent(T entity)
        {
            Entity = entity;
            Source = "Repository";
        }
    }

    public class EntityUpdatedEvent<T> : DomainEvent
    {
        public T Entity { get; }

        public EntityUpdatedEvent(T entity)
        {
            Entity = entity;
            Source = "Repository";
        }
    }

    public class EntityDeletedEvent<T> : DomainEvent
    {
        public string EntityId { get; }

        public EntityDeletedEvent(string entityId)
        {
            EntityId = entityId;
            Source = "Repository";
        }
    }

    /// <summary>
    /// Event raised when market data is saved
    /// </summary>
    public class MarketDataSavedEvent : DomainEvent
    {
        public string EntityId { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;

        public MarketDataSavedEvent()
        {
            Source = "MarketDataCommands";
        }
    }

    /// <summary>
    /// Event raised when market data is deleted
    /// </summary>
    public class MarketDataDeletedEvent : DomainEvent
    {
        public string EntityId { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public bool IsSoftDelete { get; set; }

        public MarketDataDeletedEvent()
        {
            Source = "MarketDataCommands";
        }
    }

    /// <summary>
    /// Event raised when soft-deleted market data is purged
    /// </summary>
    public class MarketDataPurgedEvent : DomainEvent
    {
        public int EntityCount { get; set; }
        public string EntityType { get; set; } = string.Empty;

        public MarketDataPurgedEvent()
        {
            Source = "MarketDataCommands";
        }
    }
}
