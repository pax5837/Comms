using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;
using Comms.BluetoothLowEnergy.Contracts;

namespace Comms.BluetoothLowEnergy;

internal abstract class BleCharacteristic<T> : IBleCharacteristic<T>
{
    protected readonly GattCharacteristic GattCharacteristic;

    internal BleCharacteristic(GattCharacteristic gattCharacteristic)
    {
        this.GattCharacteristic = gattCharacteristic;
    }

    public Guid Uuid => GattCharacteristic.Uuid;
    public Endianness Endianness { get; set; } = Endianness.BigEndian;
    protected ByteOrder ByteOrder => Endianness == Endianness.LittleEndian ? ByteOrder.LittleEndian : ByteOrder.BigEndian;

    public IBleCharacteristic<T> WithEndianness(Endianness endianness)
    {
        Endianness = endianness;
        return this;
    }

    protected async Task<DataReader> GetDataReader()
    {
        var res = await GattCharacteristic.ReadValueAsync(BluetoothCacheMode.Uncached);
        var reader = Windows.Storage.Streams.DataReader.FromBuffer(res.Value);
        return reader;
    }

    public abstract Task<T> ReadValueAsync();

    public abstract Task<bool> WriteWithoutResponseAsync(T value);

    public abstract Task SetupNotificationHandler(Action<T> onValueReceived);
}
