using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using Comms.Pipelines.Contracts;

namespace Comms.Pipelines.Implementations;

internal class PipeClient : IPipeClient, IDisposable
{
    private StreamReader pipeReader;
    private StreamWriter pipeWriter;
    private readonly NamedPipeClientStream client;
    private bool isConnected;
    private bool doRun;

    public PipeClient(string pipeName, Action<string> onMessageReceived, Action? onDisconnected)
    {
        client = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);

        doRun = true;
        Task.Factory.StartNew(() => RunClient(onMessageReceived, onDisconnected));
    }

    private void RunClient(Action<string> onMessageReceived, Action? onDisconnected)
    {
        try
        {
            while (doRun)
            {
                client.Connect();
                Console.WriteLine("connected");
                pipeReader = new StreamReader(client);
                pipeWriter = new StreamWriter(client);
                isConnected = true;
                while (client.IsConnected && doRun)
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
            if (!doRun)
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
        doRun = false;
        // server.Disconnect();
        pipeReader.Close();
        pipeWriter.Close();
    }

    public void Dispose()
    {
        Disconnect();
        pipeReader.Dispose();
        pipeWriter.Dispose();
        client.Dispose();
    }
}