using System.Collections.Immutable;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Comms.BluetoothLowEnergy.Contracts;

namespace Comms.BluetoothLowEnergy;

internal class BleCharacteristicCollection : IBleCharacteristicCollection
{
    private readonly IImmutableDictionary<Guid, GattCharacteristic> characteristicsByGuid;

    private readonly IImmutableDictionary<Type, Func<GattCharacteristic,object>> chars = new Dictionary<Type, Func<GattCharacteristic, object>>
    {
        { typeof(string), gc => new StringBleCharacteristic(gc) },
        { typeof(short), gc => new Int16BleCharacteristic(gc) },
        { typeof(int), gc => new Int32BleCharacteristic(gc) },
        { typeof(float), gc => new SingleBleCharacteristic(gc) },
        { typeof(bool), gc => new BoolBleCharacteristic(gc) },
        { typeof(byte), gc => new ByteBleCharacteristic(gc) },
    }.ToImmutableDictionary();

    public BleCharacteristicCollection(IImmutableDictionary<Guid, GattCharacteristic> characteristicsByGuid)
    {
        this.characteristicsByGuid = characteristicsByGuid;
    }

    public IBleCharacteristic<T>? GetCharacteristic<T>(Guid id)
    {
        return (IBleCharacteristic<T>)chars[typeof(T)].Invoke(characteristicsByGuid[id]);
    }
}
