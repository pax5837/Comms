using Comms.WebSockets.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Comms.WebSockets;

public static class DependencyInjectionRegistrations
{
    public static IServiceCollection AddWebSocketConnectionServices(this IServiceCollection services)
    {
        services
            .AddSingleton<IWebSocketConnectionFactory, WebSocketConnectionFactory>();

        return services;
    }
}
