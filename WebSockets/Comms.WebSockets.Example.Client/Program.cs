using Comms.WebSockets;
using Comms.WebSockets.Contracts;
using Comms.WebSockets.Example.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

var services = new ServiceCollection()
    .AddWebSocketConnectionServices()
    .AddLogging(builder => builder.AddSimpleConsole(
        options =>
        {
            options.IncludeScopes = true;
            options.TimestampFormat = "HH:mm:ss.fff ";
            options.SingleLine = true;
            options.ColorBehavior = LoggerColorBehavior.Enabled;
        }));
var serviceProvider = services.BuildServiceProvider();

var (run, url) = ConsoleConfigurator.ConfigureConsoleApp(initialBaseUrl: "ws://localhost:5150");
if (!run)
{
    return;
}

var globalCancellationTokenSource = new CancellationTokenSource();

var logger = serviceProvider.GetService<ILogger<Program>>();

var connTaskWs = StartNewClientAsync(serviceProvider, url, "ws", logger, globalCancellationTokenSource, out var connWs);
var connTaskAbc = StartNewClientAsync(serviceProvider, url, "abc", logger, globalCancellationTokenSource, out var connAbc);

while (!globalCancellationTokenSource.Token.IsCancellationRequested)
{
    var input = Console.ReadLine();
    if (input.Equals("x", StringComparison.OrdinalIgnoreCase))
    {
        globalCancellationTokenSource.Cancel();
        continue;
    }

    if (string.IsNullOrWhiteSpace(input))
    {
        continue;
    }

    await SendMessageToWebSocket(input, "ws", logger, connWs, globalCancellationTokenSource);
    await SendMessageToWebSocket(input, "abc", logger, connWs, globalCancellationTokenSource);
}

await connTaskWs;
await connTaskAbc;
Console.WriteLine("done");

Task StartNewClientAsync(
    ServiceProvider serviceProvider1,
    string baseUrl,
    string endpoint,
    ILogger<Program>? logger,
    CancellationTokenSource cancellationTokenSource,
    out IWebSocketClientConnection webSocketConnection)
{
    return serviceProvider1.GetService<IWebSocketConnectionFactory>()!.StartNewClientAsync(
        url: $"{baseUrl}/{endpoint}",
        onMessageReceived: message => logger?.LogInformation("Message received from {Endpoint}: {Message}", endpoint, message),
        onConnected: () => logger?.LogWarning("Connected to {Endpoint}", endpoint),
        onDisconnected: () => logger?.LogWarning("Disconnected from {Endpoint}", endpoint),
        receiveTimeout: null,
        sendTimeout: TimeSpan.FromSeconds(2),
        connectionTimeout: TimeSpan.FromSeconds(5),
        cancellationToken: cancellationTokenSource.Token,
        out webSocketConnection,
        receiveBufferSize: 40_000);
}

async Task SendMessageToWebSocket(
    string input,
    string target,
    ILogger<Program>? logger,
    IWebSocketClientConnection connWs1,
    CancellationTokenSource globalCancellationTokenSource1)
{
    if (input.StartsWith($"{target}>"))
    {
        logger?.LogInformation("Sending message to '{Target}' through socket {Message}", target, input);
        await connWs1.SendMessageAsync(input, globalCancellationTokenSource1.Token);
    }
}
