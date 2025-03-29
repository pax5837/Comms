using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace Comms.BluetoothLowEnergy;

internal class SingleBleCharacteristic : BleCharacteristic<float>
{
    public SingleBleCharacteristic(GattCharacteristic gattCharacteristic) : base(gattCharacteristic)
    { }

    public override async Task<float> ReadValueAsync()
    {
        using var reader = await GetDataReader();
        reader.ByteOrder = ByteOrder;
        return reader.ReadSingle();
    }

    public override async Task<bool> WriteWithoutResponseAsync(float value)
    {
        using var dataWriter = new Windows.Storage.Streams.DataWriter();
        dataWriter.ByteOrder = ByteOrder;
        dataWriter.WriteSingle(value);
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

    public override async Task SetupNotificationHandler(Action<float> onValueReceived)
    {
        await GattCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
            GattClientCharacteristicConfigurationDescriptorValue.Notify);
        GattCharacteristic.ValueChanged += (sender, eventArgs) =>
        {
            using var reader = Windows.Storage.Streams.DataReader.FromBuffer(eventArgs.CharacteristicValue);
            reader.ByteOrder = ByteOrder;
            var value = reader.ReadSingle();
            onValueReceived?.Invoke(value);
        };
    }
}
