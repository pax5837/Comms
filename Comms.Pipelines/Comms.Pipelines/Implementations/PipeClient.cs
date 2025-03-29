using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using Comms.Pipelines.Contracts;

namespace Comms.Pipelines.Implementations;

internal class PipeClient : IPipeClient, IDisposable
{
    private StreamReader pipeReader;
    private StreamWriter pipeWriter;
    private readonly NamedPipeClientStream client;
    private bool isConnected;
    private CancellationTokenSource cancellationTokenSource;

    public PipeClient(
        string pipeName,
        Action<string> onMessageReceived,
        Action? onConnected,
        Action? onDisconnected)
    {
        client = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);

        cancellationTokenSource = new CancellationTokenSource();
        Task.Factory.StartNew(() => RunClient(onMessageReceived, onConnected, onDisconnected, cancellationTokenSource.Token));
    }

    private void RunClient(Action<string> onMessageReceived, Action? onConnected, Action? onDisconnected, CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                client.Connect();
                onConnected?.Invoke();
                pipeReader = new StreamReader(client);
                pipeWriter = new StreamWriter(client);
                isConnected = true;
                while (client.IsConnected && !cancellationToken.IsCancellationRequested)
                {
                    var line = pipeReader.ReadLine();
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        onMessageReceived.Invoke(line);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            isConnected = false;
            if (cancellationToken.IsCancellationRequested)
            {
                onDisconnected?.Invoke();
            }
        }
    }

    public void Send(string message)
    {
        if (client.IsConnected && pipeWriter != null)
        {
            pipeWriter.WriteLine($"{message}\r\n");
            pipeWriter.Flush();
        }
    }

    public async Task SendAsync(string message)
    {
        await pipeWriter.WriteLineAsync($"{message}\r\n");
        await pipeWriter.FlushAsync();
    }

    public bool IsConnected()
    {
        return isConnected;
    }

    public void Disconnect()
    {
        cancellationTokenSource.Cancel();
        try
        {
            pipeReader.Close();
        }
        catch (Exception)
        { }

        try
        {
            pipeWriter.Close();
        }
        catch (Exception )
        { }
    }

    public void Dispose()
    {
        Disconnect();
        client.Dispose();
    }
}
