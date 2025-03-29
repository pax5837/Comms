using Windows.Devices.Bluetooth.Advertisement;

namespace Comms.BluetoothLowEnergy.Contracts;

public record DeviceInfo(ulong BluetoothAddress, BluetoothLEAdvertisement BleAdvertisement);
