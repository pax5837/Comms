using System.Collections.Immutable;

namespace Comms.BluetoothLowEnergy.Contracts;

public interface IBleWatcher
{
    void StartWatching(IImmutableSet<Guid> serviceIdFilter);

    void StopWatching();

    IImmutableSet<DeviceInfo> GetDeviceInfos();
}
