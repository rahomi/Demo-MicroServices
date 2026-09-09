namespace Contracts.Messaging;

/// <summary>
/// Publishes integration events to RabbitMQ.
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync<T>(string exchange, string routingKey, T @event, CancellationToken cancellationToken = default) where T : class;
}
