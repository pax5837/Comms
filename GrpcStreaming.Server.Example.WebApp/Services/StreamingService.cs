using System.Collections.Concurrent;
using Grpc.Core;
using Streamy;

namespace GrpcStreaming.Server.Example.WebApp.Services;

public class StreamingService : Streamy.StreamingService.StreamingServiceBase
{
    public override async Task<Response> StreamedRequest(IAsyncStreamReader<Request> requestStream, ServerCallContext context)
    {
        var counter = 0;

        while (await requestStream.MoveNext())
        {
            var message = requestStream.Current;
            counter++;
            Console.WriteLine(message);
        }

        Console.WriteLine("Stream ended");

        return new Response { Message = "Processed messages: " + counter };
    }

    public override async Task StreamedResponse(
        Request request,
        IServerStreamWriter<Response> responseStream,
        ServerCallContext context)
    {
        Console.WriteLine(request);
        for (int i = 0; i < request.Number; i++)
        {
            var responseText = $"Reply to {request.Message} - {i+1}/{request.Number}";
            Console.WriteLine($"Replying with: {responseText}");
            await responseStream.WriteAsync(new Response { Message = responseText });
        }
    }

    public override async Task BidirectionalStream(
        IAsyncStreamReader<Request> requestStream,
        IServerStreamWriter<Response> responseStream,
        ServerCallContext context)
    {
        var queue = new  ConcurrentQueue<string>();
        var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken);

        var t = ReplyTask(responseStream, queue, cancellationTokenSource.Token);

        while (await requestStream.MoveNext())
        {
            var message = requestStream.Current;
            Console.WriteLine(message);
            EnqueueingFireAndForgetTask(message.Message, message.Number, queue, cancellationTokenSource.Token);
        }

        Console.WriteLine("Stream ended");
        await cancellationTokenSource.CancelAsync();
        await t;
    }

    private async Task EnqueueingFireAndForgetTask(string message,int count, ConcurrentQueue<string> queue, CancellationToken cancellationToken){
    {
        for (var i = 0; i < count; i++)
        {
            var responseText = $"Reply to {message} - {i + 1}/{count}";
            Console.WriteLine($"Replying with: {responseText}");
            queue.Enqueue(responseText);
            await Task.Delay(1000, cancellationToken);
        }

        Console.WriteLine($"Enqueueing task for message {message} completed");
    }}

    private async Task ReplyTask(IServerStreamWriter<Response> responseStream, ConcurrentQueue<string> queue, CancellationToken cancellationToken )
    {
        try
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                if(queue.TryDequeue(out var msg))
                {
                    await responseStream.WriteAsync(new Response { Message = msg }, cancellationToken);
                }
                else
                {
                    await Task.Delay(50, cancellationToken);
                }
            }
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("Task cancelled");
        }
    }
}
