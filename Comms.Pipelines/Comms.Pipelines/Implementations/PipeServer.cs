using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using Comms.Pipelines.Contracts;

namespace Comms.Pipelines.Implementations;

internal class PipeServer : IPipeServer, IDisposable
{
    private readonly string pipeName;
    private NamedPipeServerStream? pipeServerStream;
    private bool doRun;
    private StreamReader pipeReader;
    private StreamWriter pipeWriter;
    private bool isConnected;

    public EventHandler<string> OnMessageReceived { get; set; }

    public PipeServer(string pipeName, Action<string>? onMessageReceived, Action? onDisconnected)
    {
        this.pipeName = pipeName;


        doRun = true;
        Task.Factory.StartNew(() => RunServer(onMessageReceived, onDisconnected));
    }

    private void RunServer(Action<string>? onMessageReceived, Action? onDisconnected)
    {
        while (doRun)
        {
            OpenConnection(onMessageReceived, onDisconnected);
        }
    }

    private void OpenConnection(Action<string>? onMessageReceived, Action? onDisconnected)
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
            Console.WriteLine("connected");
            pipeReader = new StreamReader(pipeServerStream);
            pipeWriter = new StreamWriter(pipeServerStream);
            isConnected = true;
            while (pipeServerStream.IsConnected && doRun)
            {
                var line = pipeReader.ReadLine();
                if (!string.IsNullOrWhiteSpace(line))
                {
                    onMessageReceived?.Invoke(line);
                    OnMessageReceived?.Invoke(this, line);
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
            if (!doRun)
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
        doRun = false;
        pipeServerStream?.Disconnect();
        pipeServerStream?.Dispose();
        pipeServerStream = null;
    }

    public void Dispose()
    {
        Disconnect();
        pipeReader.Dispose();
        pipeWriter.Dispose();
        pipeServerStream.Dispose();
    }
}