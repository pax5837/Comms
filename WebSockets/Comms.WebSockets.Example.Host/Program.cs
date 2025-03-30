using Comms.WebSockets.Example.Host;
using Microsoft.Extensions.Logging.Console;

Console.Title = "WebSockets example host";
var builder = WebApplication.CreateBuilder();
builder.Services.AddLogging(builder => ConfigureLogging(builder));

var app = builder.Build();
app.UseWebSockets();

var cancellationTokenSource = new CancellationTokenSource();

EchoSocket.MapWebSocketEndpoint(app, "/ws", cancellationTokenSource, app.Services);
EchoSocket.MapWebSocketEndpoint(app, "/abc", cancellationTokenSource, app.Services);

await app.RunAsync();

ILoggingBuilder ConfigureLogging(ILoggingBuilder loggingBuilder)
{
    return loggingBuilder
        .AddSimpleConsole(options =>
        {
            options.TimestampFormat = "HH:mm:ss.fff ";
            options.SingleLine = true;
            options.ColorBehavior = LoggerColorBehavior.Enabled;
        });
}
