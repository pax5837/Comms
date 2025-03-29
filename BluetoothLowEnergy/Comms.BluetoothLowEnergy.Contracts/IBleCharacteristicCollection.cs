namespace Comms.BluetoothLowEnergy.Contracts;

public interface IBleCharacteristicCollection
{
    IBleCharacteristic<T>? GetCharacteristic<T>(Guid id);
}