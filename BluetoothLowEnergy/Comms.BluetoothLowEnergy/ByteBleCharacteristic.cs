using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace Comms.BluetoothLowEnergy;

internal class ByteBleCharacteristic : BleCharacteristic<byte>
{
    public ByteBleCharacteristic(GattCharacteristic gattCharacteristic) : base(gattCharacteristic)
    { }

    public override async Task<byte> ReadValueAsync()
    {
        using var reader = await GetDataReader();
        reader.ByteOrder = ByteOrder;
        return reader.ReadByte();
    }

    public override async Task<bool> WriteWithoutResponseAsync(byte value)
    {
        using var dataWriter = new Windows.Storage.Streams.DataWriter();
        dataWriter.ByteOrder = ByteOrder;
        dataWriter.WriteByte(value);
        try
        {
            await GattCharacteristic.WriteValueAsync(dataWriter.DetachBuffer());
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public override async Task SetupNotificationHandler(Action<byte> onValueReceived)
    {
        await GattCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
            GattClientCharacteristicConfigurationDescriptorValue.Notify);
        GattCharacteristic.ValueChanged += (sender, eventArgs) =>
        {
            using var reader = Windows.Storage.Streams.DataReader.FromBuffer(eventArgs.CharacteristicValue);
            reader.ByteOrder = ByteOrder;
            var value = reader.ReadByte();
            onValueReceived?.Invoke(value);
        };
    }
}
