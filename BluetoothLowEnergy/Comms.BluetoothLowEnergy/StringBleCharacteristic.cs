using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace Comms.BluetoothLowEnergy;

internal class StringBleCharacteristic : BleCharacteristic<string>
{
    public StringBleCharacteristic(GattCharacteristic gattCharacteristic) : base(gattCharacteristic)
    { }

    public override async Task<string> ReadValueAsync()
    {
        using var reader = await GetDataReader();
        return reader.ReadString(reader.UnconsumedBufferLength);
    }

    public override async Task<bool> WriteWithoutResponseAsync(string value)
    {
        using var dataWriter = new Windows.Storage.Streams.DataWriter();
        dataWriter.WriteString(value);
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

    public override async Task SetupNotificationHandler(Action<string> onValueReceived)
    {
        await GattCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
            GattClientCharacteristicConfigurationDescriptorValue.Notify);
        GattCharacteristic.ValueChanged += (sender, eventArgs) =>
        {
            using var reader = Windows.Storage.Streams.DataReader.FromBuffer(eventArgs.CharacteristicValue);
            reader.ByteOrder = ByteOrder;
            var value = reader.ReadString(reader.UnconsumedBufferLength);
            onValueReceived?.Invoke(value);
        };
    }
}
