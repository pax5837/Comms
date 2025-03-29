using Comms.Pipelines.Contracts;
using Comms.Pipelines.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace Comms.Pipelines;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddPipelineServices(this IServiceCollection services)
    {
        services
            .AddTransient<IPipeFactory, PipeFactory>();

        return services;
    }
}