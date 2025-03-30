using System.Net;
using System.Net.WebSockets;
using System.Text;

namespace Comms.WebSockets.Example.Host;

internal class EchoSocket
{
    public static void MapWebSocketEndpoint(WebApplication webApplication, string path, CancellationTokenSource cancellationTokenSource1, IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetService<ILogger<EchoSocket>>()!;

        webApplication.Map(
            path,
            async context => await HandleWebSocketCommunication(path, context, logger, cancellationTokenSource1));
    }

    private static async Task HandleWebSocketCommunication(
        string path,
        HttpContext context,
        ILogger<EchoSocket>? logger,
        CancellationTokenSource cancellationTokenSource1)
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            await Listen(path, context, logger, cancellationTokenSource1);
        }
        else
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        }
    }

    private static async Task Listen(string path, HttpContext context, ILogger<EchoSocket>? logger,
        CancellationTokenSource cancellationTokenSource1)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();

        new TaskFactory().StartNew(async () =>
        {
            while (!cancellationTokenSource1.Token.IsCancellationRequested)
            {
                // await webSocket.SendAsync(Encoding.ASCII.GetBytes($"Test - {DateTime.Now:hh:mm:ss.fff}"),
                //     WebSocketMessageType.Text, true, CancellationToken.None);
                await Task.Delay(20);
            }
        });

        var buffer = new byte[256];
        while (!cancellationTokenSource1.Token.IsCancellationRequested)
        {
            var result = await webSocket.ReceiveAsync(buffer, cancellationTokenSource1.Token);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                //await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, cancellationTokenSource.Token);
            }
            else
            {
                var receivedMessage = Encoding.ASCII.GetString(buffer, 0, result.Count);
                logger.LogInformation("Received: {ReceivedMessage}", receivedMessage);
                await webSocket.SendAsync(
                    Encoding.ASCII.GetBytes($"Echo from {path} : {receivedMessage}"),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
            }
        }
    }
}
