using System.Net;
using System.Net.WebSockets;
using System.Text;

namespace Comms.WebSockets.Example.Host;

internal class WebSocketWrapper : IDisposable
{
    private WebSocket? webSocket;
    
    public async Task ConnectAsync(HttpContext context, CancellationToken cancellationToken)
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            webSocket = await context.WebSockets.AcceptWebSocketAsync();
            
            while (true)
            {
                await webSocket.SendAsync(Encoding.ASCII.GetBytes($"Test - {DateTime.Now}"), WebSocketMessageType.Text, true, CancellationToken.None);
                await Task.Delay(1000);
            }
        }
        else
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        }
    }


    public void Dispose()
    {
        webSocket?.Dispose();
    }
}