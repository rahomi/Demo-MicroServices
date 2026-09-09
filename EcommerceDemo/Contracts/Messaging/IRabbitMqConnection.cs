using RabbitMQ.Client;

namespace Contracts.Messaging;

/// <summary>
/// Abstraction over a persistent RabbitMQ connection with automatic reconnect.
/// </summary>
public interface IRabbitMqConnection : IAsyncDisposable
{
    bool IsConnected { get; }

    Task<bool> TryConnectAsync();

    Task<IChannel> CreateChannelAsync();
}
