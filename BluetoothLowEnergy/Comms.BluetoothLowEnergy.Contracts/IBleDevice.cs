namespace Comms.BluetoothLowEnergy.Contracts;

public interface IBleDevice : IDisposable
{
    public EventHandler<string> OnDisconnected { get; set; }

    Task<IBleCharacteristicCollection?> GetCharacteristicsAsync(
        Guid serviceId,
        params Guid[] characteristicIdFilter);

    bool IsConnected();
}