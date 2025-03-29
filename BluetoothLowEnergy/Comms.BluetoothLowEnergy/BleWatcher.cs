using System.Collections.Immutable;
using Windows.Devices.Bluetooth.Advertisement;
using Comms.BluetoothLowEnergy.Contracts;
using Microsoft.Extensions.Logging;

namespace Comms.BluetoothLowEnergy;

internal class BleWatcher : IBleWatcher
{
    private readonly ILogger<BleWatcher> logger;
    private readonly BluetoothLEAdvertisementWatcher watcher;

    private IImmutableSet<Guid>? serviceIdFilter;
    private bool isWatching;

    private readonly IDictionary<ulong, BluetoothLEAdvertisement> advertisementByBluetoothAddress =
        new Dictionary<ulong, BluetoothLEAdvertisement>();

    public BleWatcher(ILogger<BleWatcher> logger)
    {
        this.logger = logger;
        watcher = new BluetoothLEAdvertisementWatcher
        {
            ScanningMode = BluetoothLEScanningMode.Active,
        };
        watcher.Received += WatcherOnReceived;
    }

    public void StartWatching(IImmutableSet<Guid> serviceIdFilter)
    {
        isWatching = true;
        this.serviceIdFilter = serviceIdFilter;
        try
        {
            watcher.Start();
        }
        catch (Exception)
        {
            logger.LogError("Could not connect to bluetooth, verify bluetooth is switched on.");
            throw;
        }
    }

    public void StopWatching()
    {
        watcher.Stop();
        isWatching = false;
    }

    public IImmutableSet<DeviceInfo> GetDeviceInfos()
    {
        return advertisementByBluetoothAddress
            .Select(kvp  => new DeviceInfo(kvp.Key, kvp.Value))
            .ToImmutableHashSet();
    }

    private void WatcherOnReceived(
        BluetoothLEAdvertisementWatcher sender,
        BluetoothLEAdvertisementReceivedEventArgs args)
    {
        if (args.IsConnectable
            && ( !serviceIdFilter.Any() || AdvertisementContainsOneOfTheServiceIds(args)))
        {
            logger.LogInformation("Received BLE advertisement from {BluetoothAddress}", args.BluetoothAddress);

            if (advertisementByBluetoothAddress.ContainsKey(args.BluetoothAddress))
            {
                advertisementByBluetoothAddress.Remove(args.BluetoothAddress);
            }

            advertisementByBluetoothAddress.Add(args.BluetoothAddress, args.Advertisement);
        }
    }

    private bool AdvertisementContainsOneOfTheServiceIds(BluetoothLEAdvertisementReceivedEventArgs args)
    {
        return serviceIdFilter.Intersect(args.Advertisement.ServiceUuids).Any();
    }
}
