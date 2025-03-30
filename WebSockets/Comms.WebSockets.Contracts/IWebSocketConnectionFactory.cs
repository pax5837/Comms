namespace Comms.WebSockets.Contracts;

public interface IWebSocketConnectionFactory
{
    /// <summary>
    /// Starts a new web socket connection, which reconnects automatically.
    /// </summary>
    /// <param name="url">The url to the web socket.</param>
    /// <param name="onMessageReceived">A func that handles the received message.</param>
    /// <param name="onConnected">The action to be called when a connection occurs</param>
    /// <param name="onDisconnected">The action to be called when a disconnection occurs.</param>
    /// <param name="receiveTimeout">When no message is received during this time, the connection will be assumed to be broken, and a reconnection attempt will be started.</param>
    /// <param name="sendTimeout">When sending a message takes longer than this time, the connection will be assumed to be broken, and a reconnection attempt will be started.</param>
    /// <param name="connectionTimeout">When the connection does not succeed during this time, a fresh reconnection attempt will be started.</param>
    /// <param name="cancellationToken">A cancellation token that stops the connection.</param>
    /// <param name="clientConnection">The connection class, providing for example means to send messages.</param>
    /// <param name="receiveBufferSize">The buffer size used for reception.</param>
    /// <returns>The task running the web socket listener.</returns>
    Task StartNewClientAsync(string url,
        Action<string> onMessageReceived,
        Action? onConnected,
        Action? onDisconnected,
        TimeSpan? receiveTimeout,
        TimeSpan sendTimeout,
        TimeSpan connectionTimeout,
        CancellationToken cancellationToken,
        out IWebSocketClientConnection clientConnection,
        int receiveBufferSize = 256);
}
