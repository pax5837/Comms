using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using Comms.Pipelines.Contracts;

namespace Comms.Pipelines.Implementations;

internal class PipeServer : IPipeServer, IDisposable
{
    private readonly string pipeName;
    private NamedPipeServerStream? pipeServerStream;
    private StreamReader pipeReader;
    private StreamWriter pipeWriter;
    private bool isConnected;
    CancellationTokenSource cancellationTokenSource;

    public PipeServer(string pipeName, Action<string>? onMessageReceived, Action? onConnected, Action? onDisconnected)
    {
        this.pipeName = pipeName;


        cancellationTokenSource = new CancellationTokenSource();
        Task.Factory.StartNew(() => RunServer(onMessageReceived, onConnected, onDisconnected, cancellationTokenSource.Token));
    }

    private void RunServer(
        Action<string>? onMessageReceived,
        Action? onConnected,
        Action? onDisconnected,
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            OpenConnection(onMessageReceived, onConnected, onDisconnected, cancellationToken);
        }
    }

    private void OpenConnection(
        Action<string>? onMessageReceived,
        Action? onConnected,
        Action? onDisconnected,
        CancellationToken cancellationToken)
    {
        try
        {
            if (pipeServerStream is not null)
            {
                pipeServerStream.Dispose();
                pipeServerStream = null;
            }

            Task.Delay(500).Wait();

            pipeServerStream = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 2, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            pipeServerStream.WaitForConnection();
            onConnected?.Invoke();
            pipeReader = new StreamReader(pipeServerStream);
            pipeWriter = new StreamWriter(pipeServerStream);
            isConnected = true;
            while (pipeServerStream.IsConnected && !cancellationToken.IsCancellationRequested)
            {
                var line = pipeReader.ReadLine();
                if (!string.IsNullOrWhiteSpace(line))
                {
                    onMessageReceived?.Invoke(line);
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
            if (cancellationTokenSource.IsCancellationRequested)
            {
                onDisconnected?.Invoke();
            }
        }
    }

    public void Send(string message)
    {
        pipeWriter.WriteLine($"{message}\r\n");
        pipeWriter.Flush();
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
            pipeServerStream?.Disconnect();
        }
        catch (Exception)
        { }

        try
        {
            pipeServerStream?.Dispose();
        }
        catch (Exception )
        { }
        pipeServerStream = null;
    }

    public void Dispose()
    {
        Disconnect();
        try
        {
            pipeReader.Dispose();
        }
        catch (Exception )
        { }

        try
        {
            pipeWriter.Dispose();
        }
        catch (Exception)
        { }

        try
        {
            pipeServerStream?.Dispose();
        }
        catch (Exception)
        { }
    }
}
