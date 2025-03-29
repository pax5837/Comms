using System.Threading.Tasks;

namespace Comms.Pipelines.Contracts;

public interface IPipeServer
{
    void Send(string message);

    Task SendAsync(string message);

    bool IsConnected();

    void Disconnect();
}
