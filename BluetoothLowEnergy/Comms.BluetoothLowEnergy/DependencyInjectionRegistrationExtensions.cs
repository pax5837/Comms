using Comms.BluetoothLowEnergy.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Comms.BluetoothLowEnergy;

public static class DependencyInjectionRegistrationExtensions
{
    public static IServiceCollection AddBleServices(this IServiceCollection services)
    {
        services
            .AddSingleton<IBleDeviceFactory, BleDeviceFactory>()
            .AddSingleton<IBleWatcher, BleWatcher>();
        return services;
    }
}
