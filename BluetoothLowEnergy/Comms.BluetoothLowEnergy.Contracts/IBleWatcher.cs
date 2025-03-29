using System.Collections.Immutable;

namespace Comms.BluetoothLowEnergy.Contracts;

public interface IBleWatcher
{
    void StartWatching(Guid[] serviceIdFilter, TimeSpan duration);

    void StopWatching();

    IImmutableSet<DeviceInfo> GetDeviceInfos();
}
