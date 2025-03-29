using Comms.Pipelines.Contracts;
using Comms.Pipelines.Implementations;

namespace Comms.Pipelines;

public static class PipeFactoryProvider
{
    public static IPipeFactory GetPipeFactoryInstance()
    {
        return new PipeFactory();
    }
}