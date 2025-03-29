using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace Comms.BluetoothLowEnergy;

internal class Int16BleCharacteristic : BleCharacteristic<short>
{
    public Int16BleCharacteristic(GattCharacteristic gattCharacteristic) : base(gattCharacteristic)
    { }

    public override async Task<short> ReadValueAsync()
    {
        using var reader = await GetDataReader();
        reader.ByteOrder = ByteOrder;
        return reader.ReadInt16();
    }

    public override async Task<bool> WriteWithoutResponseAsync(short value)
    {
        using var dataWriter = new Windows.Storage.Streams.DataWriter();
        dataWriter.ByteOrder = ByteOrder;
        dataWriter.WriteInt16(value);
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

    public override async Task SetupNotificationHandler(Action<short> onValueReceived)
    {
        await GattCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
            GattClientCharacteristicConfigurationDescriptorValue.Notify);
        GattCharacteristic.ValueChanged += (sender, eventArgs) =>
        {
            using var reader = Windows.Storage.Streams.DataReader.FromBuffer(eventArgs.CharacteristicValue);
            reader.ByteOrder = ByteOrder;
            var value = reader.ReadInt16();
            onValueReceived?.Invoke(value);
        };
    }
}
