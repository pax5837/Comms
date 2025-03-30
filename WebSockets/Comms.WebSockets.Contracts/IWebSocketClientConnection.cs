namespace Comms.WebSockets.Contracts;

public interface IWebSocketClientConnection : IDisposable
{
    Task SendMessageAsync(string message, CancellationToken cancellationToken);

    bool IsConnected { get; }

    void Stop();
}
