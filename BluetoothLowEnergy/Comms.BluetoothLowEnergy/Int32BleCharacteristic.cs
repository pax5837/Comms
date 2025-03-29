using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace Comms.BluetoothLowEnergy;

internal class Int32BleCharacteristic : BleCharacteristic<int>
{
    public Int32BleCharacteristic(GattCharacteristic gattCharacteristic) : base(gattCharacteristic)
    { }

    public override async Task<int> ReadValueAsync()
    {
        using var reader = await GetDataReader();
        reader.ByteOrder = ByteOrder;
        return reader.ReadInt16();
    }

    public override async Task<bool> WriteWithoutResponseAsync(int value)
    {
        using var dataWriter = new Windows.Storage.Streams.DataWriter();
        dataWriter.ByteOrder = ByteOrder;
        dataWriter.WriteInt32(value);
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

    public override async Task SetupNotificationHandler(Action<int> onValueReceived)
    {
        await GattCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
            GattClientCharacteristicConfigurationDescriptorValue.Notify);
        GattCharacteristic.ValueChanged += (sender, eventArgs) =>
        {
            using var reader = Windows.Storage.Streams.DataReader.FromBuffer(eventArgs.CharacteristicValue);
            reader.ByteOrder = ByteOrder;
            var value = reader.ReadInt32();
            onValueReceived?.Invoke(value);
        };
    }
}
