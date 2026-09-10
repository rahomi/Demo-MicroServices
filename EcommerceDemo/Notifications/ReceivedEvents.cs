using System.Collections.Concurrent;

namespace Notifications;

/// <summary>
/// Thread-safe in-memory store for received integration events.
/// Singleton — shared between the consumers and the API endpoint.
/// </summary>
public class ReceivedEvents
{
    private readonly ConcurrentBag<ReceivedEvent> _events = [];

    public void Add(ReceivedEvent @event) => _events.Add(@event);

    public List<ReceivedEvent> GetAll() => _events
        .OrderByDescending(e => e.ReceivedAt)
        .ToList();
}

/// <summary>
/// A received integration event stored in memory.
/// </summary>
public record ReceivedEvent(
    string EventType,
    string RoutingKey,
    string Payload,
    DateTime ReceivedAt);
