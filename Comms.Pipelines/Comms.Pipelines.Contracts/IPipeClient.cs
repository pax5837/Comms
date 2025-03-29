using System.Threading.Tasks;

namespace Comms.Pipelines.Contracts;

public interface IPipeClient
{
    void Send(string message);

    Task SendAsync(string message);

    bool IsConnected();

    void Disconnect();
}