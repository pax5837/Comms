using Windows.Devices.Bluetooth;
using Comms.BluetoothLowEnergy.Contracts;

namespace Comms.BluetoothLowEnergy;

public class BleDeviceFactory : IBleDeviceFactory
{
    public async Task<IBleDevice?> GetDeviceAsync(ulong bluetoothAdress)
    {
        var device = await BluetoothLEDevice.FromBluetoothAddressAsync(bluetoothAdress);
        return device == null ? null : new BleDevice(device);
    }
}
