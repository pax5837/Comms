using System.Net.WebSockets;
using System.Text;
using Comms.WebSockets.Contracts;
using Microsoft.Extensions.Logging;

namespace Comms.WebSockets;

internal class WebSocketClientConnection : IWebSocketClientConnection
{
    private readonly ILogger<WebSocketClientConnection>? logger;
    private readonly int bufferSize;

    private readonly string url;
    private readonly Action<string> onMessageReceived;
    private readonly Action? onConnected;
    private readonly Action? onDisconnected;
    private readonly ushort? receiveTimeoutMilliseconds;
    private readonly ushort sendTimeoutMilliseconds;
    private readonly ushort connectionTimeoutMilliseconds;
    private ClientWebSocket? webSocketClient;
    private CancellationTokenSource? webSocketCancellationTokenSource;
    private bool run;
    private bool isConnected;

    internal WebSocketClientConnection(string url,
        Action<string> onMessageReceived,
        Action? onConnected,
        Action? onDisconnected,
        TimeSpan? receiveTimeout,
        TimeSpan sendTimeout,
        TimeSpan connectionTimeout,
        ILogger<WebSocketClientConnection>? logger,
        int bufferSize)
    {
        this.url = url;
        this.onMessageReceived = onMessageReceived;
        this.onConnected = onConnected;
        this.onDisconnected = onDisconnected;
        this.logger = logger;
        this.bufferSize = bufferSize;
        this.receiveTimeoutMilliseconds = receiveTimeout != null
            ? (ushort)receiveTimeout.Value.TotalMilliseconds
            : null;
        this.sendTimeoutMilliseconds = (ushort)sendTimeout.TotalMilliseconds;
        this.connectionTimeoutMilliseconds = (ushort)connectionTimeout.TotalMilliseconds;
    }

    internal async Task StartAsync(CancellationToken cancellationToken)
    {
        run = true;
        while (!cancellationToken.IsCancellationRequested && run)
        {
            try
            {
                await ConnectAsync(cancellationToken);
                await LoopAsync(cancellationToken);
            }
            catch (Exception e)
            {
                isConnected = false;
                onDisconnected?.Invoke();
                CleanUpBeforeReconnecting(e);
            }
        }
    }

    private void CleanUpBeforeReconnecting(Exception e)
    {
        logger?.LogWarning("Connection lost");

        if (e is not System.Net.WebSockets.WebSocketException)
        {
            logger?.LogWarning(e.ToString());
        }

        var client = webSocketClient;
        webSocketClient = null;
        client?.Dispose();
        logger?.LogInformation("Client disposed");
    }

    private async Task LoopAsync(CancellationToken cancellationToken)
    {
        logger?.LogInformation("Starting listening loop");
        if (cancellationToken.IsCancellationRequested)
        {
            logger?.LogWarning("Cancellation requested");
        }
        while (DoLoop(cancellationToken))
        {
            var linkedTokenSource = receiveTimeoutMilliseconds != null
                ? CancellationTokenSource.CreateLinkedTokenSource(
                    new CancellationTokenSource(receiveTimeoutMilliseconds.Value).Token,
                    cancellationToken)
                : CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken);

            var buffer = new byte[bufferSize];
            if (webSocketClient is null)
            {
                logger?.LogWarning("Web socket client is null");
            }
            var result = await webSocketClient.ReceiveAsync(buffer, linkedTokenSource.Token);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await webSocketClient.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    null,
                    cancellationToken);
            }
            else if(result.Count < 2000)
            {
                onMessageReceived(Encoding.ASCII.GetString(buffer, 0, result.Count));
            }
        }
    }

    private bool DoLoop(CancellationToken cancellationToken)
    {
        var webSocketState = webSocketClient?.State;
        if (webSocketState != WebSocketState.Open)
        {
            var reason = webSocketClient?.CloseStatusDescription;
            logger?.LogWarning("Websocket is not open, current state: {WebSocketState}, close status description: {CloseDescription}",
                webSocketState,
                reason);
        }

        var cancellationRequested = cancellationToken.IsCancellationRequested;
        if (cancellationRequested)
        {
            logger?.LogWarning("Cancellation requested");
        }
        return webSocketState == WebSocketState.Open && !cancellationRequested;
    }

    private async Task ConnectAsync(CancellationToken cancellationToken)
    {
        logger?.LogInformation("Connection loop started");
        webSocketClient = new ClientWebSocket();
        webSocketCancellationTokenSource = new CancellationTokenSource();
        var timeoutCancellationTokenSource = new CancellationTokenSource(connectionTimeoutMilliseconds);

        var linkedCancellationToken =
            CancellationTokenSource.CreateLinkedTokenSource(
                    webSocketCancellationTokenSource.Token,
                    timeoutCancellationTokenSource.Token,
                    cancellationToken)
                .Token;
        logger?.LogInformation("Connecting to websocket {Url}", url);
        await webSocketClient.ConnectAsync(new Uri(url), linkedCancellationToken);
        isConnected = true;
        onConnected?.Invoke();
    }


    public async Task SendMessageAsync(string message, CancellationToken cancellationToken)
    {
        if (!isConnected)
        {
            return;
        }

        var sendTimeoutCancellationTokenSource = new CancellationTokenSource(sendTimeoutMilliseconds);
        var linkedCancellationToken = CancellationTokenSource
            .CreateLinkedTokenSource(
                sendTimeoutCancellationTokenSource.Token,
                cancellationToken).Token;
        try
        {
            var sendTask = webSocketClient?.SendAsync(
                buffer: Encoding.ASCII.GetBytes(message),
                messageType: WebSocketMessageType.Text,
                endOfMessage: true,
                cancellationToken: linkedCancellationToken);
            if (sendTask != null)
            {
                await sendTask;
            }
        }
        catch (TaskCanceledException)
        {
            if (sendTimeoutCancellationTokenSource.Token.IsCancellationRequested)
            {
                webSocketCancellationTokenSource?.Cancel();
            }
        }
        catch (Exception e)
        {
            logger?.LogWarning("Problem happened {Problem}", e.ToString());
            onDisconnected?.Invoke();
            isConnected = false;
        }
    }

    public bool IsConnected => isConnected;

    public void Stop()
    {
        run = false;
        webSocketCancellationTokenSource?.Cancel();
    }

    public void Dispose()
    {
        run = false;
        webSocketClient?.Dispose();
        webSocketCancellationTokenSource?.Cancel();
        webSocketCancellationTokenSource?.Dispose();
    }
}
