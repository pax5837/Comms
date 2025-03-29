using System;
using Comms.Pipelines.Contracts;

namespace Comms.Pipelines.Implementations;

internal class PipeFactory : IPipeFactory
{
    public IPipeClient GetPipeClient(string pipeName, Action<string> onMessageReceived, Action? onDisconnected)
    {
        return new PipeClient(pipeName, onMessageReceived, onDisconnected);
    }

    public IPipeServer GetPipeServer(string pipeName, Action<string> onMessageReceived, Action? onDisconnected)
    {
        return new PipeServer(pipeName, onMessageReceived, onDisconnected);
    }
}