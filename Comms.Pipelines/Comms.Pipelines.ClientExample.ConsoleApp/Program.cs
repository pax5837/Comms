using Comms.Pipelines;

var client = PipeFactoryProvider.GetPipeFactoryInstance().GetPipeClient(
    "MyPipe27018dcf-5872-4db1-b772-c7ebd2d2da73",
    message => Console.WriteLine(message),
    () => Console.WriteLine("Disconnected"));


var doRun = true;
while (doRun)
{
    Console.WriteLine("Enter message or 'x' to exit");
    var input = Console.ReadLine();

    if (input.Equals("x", StringComparison.OrdinalIgnoreCase))
    {
        doRun = false;
    }

    if (!string.IsNullOrWhiteSpace(input))
    {
        client.Send(input);
    }
}

client.Disconnect();
