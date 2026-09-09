using System.Diagnostics;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Contracts.Messaging;

/// <summary>
/// Persistent RabbitMQ connection with automatic reconnect logic.
/// </summary>
public class RabbitMqConnection : IRabbitMqConnection
{
    private readonly IConnectionFactory _factory;
    private readonly ILogger<RabbitMqConnection> _logger;
    private IConnection? _connection;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public bool IsConnected => _connection is { IsOpen: true };

    public RabbitMqConnection(IConnectionFactory factory, ILogger<RabbitMqConnection> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public async Task<bool> TryConnectAsync()
    {
        if (IsConnected)
            return true;

        await _semaphore.WaitAsync();
        try
        {
            if (IsConnected)
                return true;

            _logger.LogInformation("Connecting to RabbitMQ...");

            _connection = await _factory.CreateConnectionAsync();
            _connection.ConnectionShutdownAsync += OnConnectionShutdownAsync;

            _logger.LogInformation("Connected to RabbitMQ successfully.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to RabbitMQ.");
            return false;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<IChannel> CreateChannelAsync()
    {
        if (!IsConnected)
            await TryConnectAsync();

        if (_connection is null)
            throw new InvalidOperationException("RabbitMQ connection is not established.");

        return await _connection.CreateChannelAsync();
    }

    private async Task OnConnectionShutdownAsync(object sender, ShutdownEventArgs e)
    {
        _logger.LogWarning("RabbitMQ connection shut down. Will attempt to reconnect on next use.");
        await Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
            await _connection.DisposeAsync();
        _semaphore.Dispose();
    }
}
