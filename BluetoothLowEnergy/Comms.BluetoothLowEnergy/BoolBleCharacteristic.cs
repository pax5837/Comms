using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace Comms.BluetoothLowEnergy;

internal class BoolBleCharacteristic : BleCharacteristic<bool>
{
    public BoolBleCharacteristic(GattCharacteristic gattCharacteristic) : base(gattCharacteristic)
    { }

    public override async Task<bool> ReadValueAsync()
    {
        using var reader = await GetDataReader();
        return reader.ReadBoolean();
    }

    public override async Task<bool> WriteWithoutResponseAsync(bool value)
    {
        using var dataWriter = new Windows.Storage.Streams.DataWriter();
        dataWriter.ByteOrder = ByteOrder;
        dataWriter.WriteBoolean(value);
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

    public override async Task SetupNotificationHandler(Action<bool> onValueReceived)
    {
        await GattCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
            GattClientCharacteristicConfigurationDescriptorValue.Notify);
        GattCharacteristic.ValueChanged += (sender, eventArgs) =>
        {
            using var reader = Windows.Storage.Streams.DataReader.FromBuffer(eventArgs.CharacteristicValue);
            reader.ByteOrder = ByteOrder;
            var value = reader.ReadBoolean();
            onValueReceived?.Invoke(value);
        };
    }
}
