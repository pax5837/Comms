using System;
using System.Threading.Tasks;

namespace Comms.Pipelines.Contracts;

public interface IPipeServer
{
    public EventHandler<string> OnMessageReceived { get; set; }

    void Send(string message);

    Task SendAsync(string message);

    bool IsConnected();

    void Disconnect();
}