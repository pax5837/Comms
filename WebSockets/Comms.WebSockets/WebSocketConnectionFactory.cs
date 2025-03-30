using Comms.WebSockets.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Comms.WebSockets;

internal class WebSocketConnectionFactory : IWebSocketConnectionFactory
{
    private readonly IServiceProvider serviceProvider;

    public WebSocketConnectionFactory(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public Task StartNewClientAsync(
        string url,
        Action<string> onMessageReceived,
        Action? onConnected,
        Action? onDisconnected,
        TimeSpan? receiveTimeout,
        TimeSpan sendTimeout,
        TimeSpan connectionTimeout,
        CancellationToken cancellationToken,
        out IWebSocketClientConnection clientConnection,
        int receiveBufferSize)
    {
        var logger = serviceProvider.GetService<ILogger<WebSocketClientConnection>>();

        var conn = new WebSocketClientConnection(
            url: url,
            onMessageReceived: onMessageReceived,
            onConnected: onConnected,
            onDisconnected: onDisconnected,
            receiveTimeout: receiveTimeout,
            sendTimeout: sendTimeout,
            connectionTimeout: connectionTimeout,
            logger: logger,
            bufferSize: receiveBufferSize);

        clientConnection = conn;

        return conn.StartAsync(cancellationToken);
    }
}
