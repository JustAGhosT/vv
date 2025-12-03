using System;
using MediatR;

namespace vv.Domain.Events
{
    public abstract class DomainEvent : INotification
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string Version { get; set; } = "1.0";

        public string Source { get; set; } = string.Empty;

        public string EventType => GetType().Name;
    }
}
