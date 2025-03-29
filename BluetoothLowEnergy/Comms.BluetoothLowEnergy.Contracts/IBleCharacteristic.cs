namespace Comms.BluetoothLowEnergy.Contracts;

public interface IBleCharacteristic
{
    Guid Uuid { get; }
    Endianness Endianness { get; set; }
}

public interface IBleCharacteristic<T> : IBleCharacteristic
{
    Task<T> ReadValueAsync();
    
    Task<bool> WriteWithoutResponseAsync(T value);
    
    Task SetupNotificationHandler(Action<T> onValueReceived);
    
    IBleCharacteristic<T> WithEndianness(Endianness endianness);
}