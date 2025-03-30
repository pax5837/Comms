using System.Net.WebSockets;
using System.Text;

namespace Comms.WebSockets.Example.Client;

internal class WebSocketWrapper
{
    public ushort ReceiveTimeoutMilliseconds { get; set; } = 2000;
    public ushort SendTimeoutMilliseconds { get; set; } = 2000;
    public string? OutboundMessage { get; set; }

    public async Task StartAsync(string url, Action<string> onMessageReceived, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            using (var webSocket = new ClientWebSocket())
            {

                var webSocketCancellationTokenSource = new CancellationTokenSource();
                var linkedCancellationToken =
                    CancellationTokenSource.CreateLinkedTokenSource(
                            webSocketCancellationTokenSource.Token,
                            cancellationToken)
                        .Token;
                Console.WriteLine($"Connecting to websocket");
                await webSocket.ConnectAsync(new Uri(url), linkedCancellationToken);

                new TaskFactory().StartNew(async () => await LoopReceiveMessages(
                    webSocket,
                    () => webSocketCancellationTokenSource.Cancel(),
                    onMessageReceived,
                    linkedCancellationToken));

                await PerformSendMessageFromConsoleLoop(
                    webSocket,
                    () => webSocketCancellationTokenSource.Cancel(),
                    linkedCancellationToken);
            }

            Console.WriteLine("Web socket loop iteration finished");
        }
    }

    private async Task LoopReceiveMessages(
        ClientWebSocket clientWebSocket,
        Action onConnectionLost,
        Action<string> action,
        CancellationToken cancellationToken)
    {
        var buffer = new byte[256];
        while (clientWebSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
        {
            var linkedTokenSource =
                CancellationTokenSource.CreateLinkedTokenSource(new CancellationTokenSource(ReceiveTimeoutMilliseconds).Token,
                    cancellationToken);
            try
            {
                var result = await clientWebSocket.ReceiveAsync(buffer, linkedTokenSource.Token);
                Console.WriteLine(result.MessageType);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null,
                        cancellationToken);
                }
                else
                {
                    action(Encoding.ASCII.GetString(buffer, 0, result.Count));
                }
            }
            catch (TaskCanceledException)
            {
                onConnectionLost();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                onConnectionLost();
            }
        }

        Console.WriteLine($"Receiving loop exited Ws state {clientWebSocket.State}");
    }

    private async Task PerformSendMessageFromConsoleLoop(
        ClientWebSocket webSocket,
        Action onConnectionLost,
        CancellationToken cancellationToken)
    {
        Console.WriteLine("Starting sending loop");

        while (!cancellationToken.IsCancellationRequested)
        {
            var input = OutboundMessage;
            if (input == null)
            {
                await Task.Delay(100);
                continue;
            }
            OutboundMessage = null;

            var sendTimeoutCancellationTokenSource = new CancellationTokenSource(SendTimeoutMilliseconds);
            var linkedCancellationToken = CancellationTokenSource
                .CreateLinkedTokenSource(
                    sendTimeoutCancellationTokenSource.Token,
                    cancellationToken).Token;

            try
            {
                await webSocket.SendAsync(
                    buffer: Encoding.ASCII.GetBytes(input),
                    messageType: WebSocketMessageType.Text,
                    endOfMessage: true,
                    cancellationToken: linkedCancellationToken);
            }
            catch (TaskCanceledException)
            {

            }
            catch (Exception e)
            {
                Console.WriteLine($"problem happpend {e.ToString()}");
            }
        }

        Console.WriteLine("Exit sending loop");
    }
}
