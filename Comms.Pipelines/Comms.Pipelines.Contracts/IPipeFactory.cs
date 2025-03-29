using System;

namespace Comms.Pipelines.Contracts;

public interface IPipeFactory
{
    IPipeClient GetPipeClient(
        string pipeName,
        Action<string> onMessageReceived,
        Action? onConnected,
        Action? onDisconnected);

    IPipeServer GetPipeServer(
        string pipeName,
        Action<string> onMessageReceived,
        Action? onConnected,
        Action? onDisconnected);
}
