using System;
using Comms.Pipelines.Contracts;

namespace Comms.Pipelines.Implementations;

internal class PipeFactory : IPipeFactory
{
    public IPipeClient GetPipeClient(string pipeName, Action<string> onMessageReceived, Action? onConnected, Action? onDisconnected)
    {
        return new PipeClient(pipeName, onMessageReceived, onConnected, onDisconnected);
    }

    public IPipeServer GetPipeServer(string pipeName, Action<string> onMessageReceived, Action? onConnected, Action? onDisconnected)
    {
        return new PipeServer(pipeName, onMessageReceived, onConnected, onDisconnected);
    }
}
