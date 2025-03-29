using System.Collections.Immutable;
using Windows.Devices.Bluetooth;
using Comms.BluetoothLowEnergy.Contracts;

namespace Comms.BluetoothLowEnergy;

internal class BleDevice : IBleDevice
{
    private readonly BluetoothLEDevice device;

    public BleDevice(BluetoothLEDevice device)
    {
        this.device = device;
        this.device.ConnectionStatusChanged += DeviceOnConnectionStatusChanged;
    }

    private void DeviceOnConnectionStatusChanged(BluetoothLEDevice sender, object args)
    {
        if (sender.ConnectionStatus == BluetoothConnectionStatus.Disconnected)
        {
            OnDisconnected?.Invoke(this, "Disconnected");
        }
    }

    public EventHandler<string> OnDisconnected { get; set; }

    public async Task<IBleCharacteristicCollection?> GetCharacteristicsAsync(
        Guid serviceId,
        params Guid[] characteristicIdFilter)
    {
        var result = await device.GetGattServicesAsync(BluetoothCacheMode.Uncached);
        if (result == null)
        {
            return null;
        }
        var service = result.Services.FirstOrDefault(s => s.Uuid == serviceId);
        if(service == null)
        {
            return null;
        }
        var characteristics = (await service.GetCharacteristicsAsync(BluetoothCacheMode.Uncached))
            .Characteristics
            .Where(c => characteristicIdFilter.Contains(c.Uuid))
            .ToImmutableDictionary(c => c.Uuid, c => c);

        return new BleCharacteristicCollection(characteristics);
    }

    public bool IsConnected()
    {
        return device.ConnectionStatus == BluetoothConnectionStatus.Connected;
    }

    public void Dispose()
    {
        device.Dispose();
    }
}
