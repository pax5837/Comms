using Comms.Pipelines;

var client = PipeFactoryProvider.GetPipeFactoryInstance().GetPipeClient(
    pipeName: "MyPipe27018dcf-5872-4db1-b772-c7ebd2d2da73",
    onMessageReceived: message => Console.WriteLine($"Received massage from server: {message}\n\nEnter message or 'x' to exit"),
    onConnected: () => Console.WriteLine("Connected to server"),
    onDisconnected: () => Console.WriteLine("Disconnected from server"));

var doRun = true;
while (doRun)
{
    Console.WriteLine("Enter message or 'x' to exit");
    var input = Console.ReadLine();

    if (input.Equals("x", StringComparison.OrdinalIgnoreCase))
    {
        doRun = false;
    }
    else if (!string.IsNullOrWhiteSpace(input))
    {
        client.Send(input);
    }
}

client.Disconnect();
