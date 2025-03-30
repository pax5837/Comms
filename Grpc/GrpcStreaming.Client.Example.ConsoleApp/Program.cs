using Grpc.Core;
using Grpc.Net.Client;
using Streamy;

Console.WriteLine("Grpc streaming client.");

using var channel = GrpcChannel.ForAddress("http://localhost:5022");
var client = new Streamy.StreamingService.StreamingServiceClient(channel);

Console.WriteLine("What do you want to test:");
Console.WriteLine("1 -> StreamedRequest");
Console.WriteLine("2 -> StreamedResponse");
Console.WriteLine("3 -> BidirectionalStream");
Console.WriteLine("x -> Exit");
var input = Console.ReadLine();

if (input.Equals("x", StringComparison.OrdinalIgnoreCase))
{
    return;
}

if (input.Equals("1", StringComparison.OrdinalIgnoreCase))
{
    await TestStreamedRequest(client);
}

if (input.Equals("2", StringComparison.OrdinalIgnoreCase))
{
    await TestStreamedResponse(client);
}

if (input.Equals("3", StringComparison.OrdinalIgnoreCase))
{
    await TestBidirectionalStream(client);
}


async Task TestStreamedRequest(StreamingService.StreamingServiceClient streamingServiceClient)
{
    var requestStream = streamingServiceClient.StreamedRequest().RequestStream;
    while(true)
    {
        Console.WriteLine("Input (x to exit):");
        var input = Console.ReadLine();
        if (input.Equals("x", StringComparison.OrdinalIgnoreCase))
        {
            break;
        }

        await requestStream.WriteAsync(new Request { Message = input, Number = 1, });
    }

    await requestStream.CompleteAsync();
}

async Task TestStreamedResponse(StreamingService.StreamingServiceClient streamingServiceClient)
{
    Console.WriteLine("Enter message");
    var message = Console.ReadLine();
    if (message.Equals("x", StringComparison.OrdinalIgnoreCase))
    {
        return;
    }
    Console.WriteLine("Enter count");
    var count = int.Parse(Console.ReadLine());

    var request = new Request
    {
        Message = message,
        Number = count,
    };
    var responseStream = streamingServiceClient.StreamedResponse(request);
    while (await responseStream.ResponseStream.MoveNext(CancellationToken.None))
    {
        var response = responseStream.ResponseStream.Current;
        Console.WriteLine(response.Message);
    }
}

async Task TestBidirectionalStream(StreamingService.StreamingServiceClient streamingServiceClient)
{
    var call = streamingServiceClient.BidirectionalStream();
    var requestStream = call.RequestStream;
    var cancellationTokenSource = new CancellationTokenSource();
    var t = Task.Run(async () =>
    {
        var responseStream = call.ResponseStream;

        try
        {
            while (await responseStream.MoveNext(cancellationTokenSource.Token))
            {
                var resp = responseStream.Current;
                Console.WriteLine($"{resp.Message}");
            }
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("Task cancelled.");
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
        {
            Console.WriteLine("Stream cancelled.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error reading response: " + ex);
        }
    });

    while(true)
    {
        Console.WriteLine("Enter message");
        var message = Console.ReadLine();
        if (message.Equals("x", StringComparison.OrdinalIgnoreCase))
        {
            break;
        }
        Console.WriteLine("Enter count");
        var count = int.Parse(Console.ReadLine());

        await requestStream.WriteAsync(new Request { Message = message, Number = count });
    }

    await requestStream.CompleteAsync();
    cancellationTokenSource.Cancel();
    await t;
}

async Task ReadResponsesAsync(IAsyncStreamReader<Response> responseStream, CancellationToken cancellationToken)
{
    await foreach (var reps in responseStream.ReadAllAsync(cancellationToken))
    {
        Console.WriteLine(reps.Message);
    }
}
