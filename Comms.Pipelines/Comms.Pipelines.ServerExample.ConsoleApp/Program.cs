using Comms.Pipelines;

var server = PipeFactoryProvider.GetPipeFactoryInstance().GetPipeServer(
    pipeName: "MyPipe27018dcf-5872-4db1-b772-c7ebd2d2da73",
    onMessageReceived: message => Console.WriteLine($"Received massage from client: {message}\n\nEnter message or 'x' to exit"),
    onConnected: () => Console.WriteLine("Client connected"),
    onDisconnected: () => Console.WriteLine("Client disconnected"));

var doRun = true;
while (doRun)
{
    Console.WriteLine("Enter message or 'x' to exit");
    var input = Console.ReadLine();

    if (input.Equals("x", StringComparison.InvariantCultureIgnoreCase))
    {
        doRun = false;
    }
    else if (!string.IsNullOrWhiteSpace(input))
    {
        server.Send(input);
    }
}

server.Disconnect();
