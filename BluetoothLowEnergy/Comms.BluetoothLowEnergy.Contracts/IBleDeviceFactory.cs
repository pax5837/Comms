namespace Comms.BluetoothLowEnergy.Contracts;

public interface IBleDeviceFactory
{
    Task<IBleDevice?> GetDeviceAsync(ulong bluetoothAdress);
}