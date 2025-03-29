using Comms.BluetoothLowEnergy;
using Comms.BluetoothLowEnergy.Contracts;
using Microsoft.Extensions.DependencyInjection;

IServiceCollection services = new ServiceCollection();

var serviceProvider =services
    .AddLogging()
    .AddBleServices()
    .BuildServiceProvider();

var watcher = serviceProvider.GetService<IBleWatcher>();

var serviceId = Guid.Parse("f033cf82-73e3-4d91-b022-cade9d63c3b8");
var readFromDeviceCharacteristicId = Guid.Parse("c709b99a-6829-4e67-8ebb-462235418cb5");
var writeToDeviceCharacteristicId = Guid.Parse("d939184f-6caf-4b11-82d0-2ad0258874d5");

watcher.StartWatching([serviceId]);
await Task.Delay(5000);
watcher.StopWatching();

var deviceInfos = watcher.GetDeviceInfos();

foreach (var deviceInfo in deviceInfos)
{
    Console.WriteLine(deviceInfo.ToString());
}

var bleDeviceFactory = serviceProvider.GetService<IBleDeviceFactory>();
var bluetoothDevice = await bleDeviceFactory.GetDeviceAsync(deviceInfos.First().BluetoothAddress);
Console.WriteLine(bluetoothDevice.ToString());

var characteristics = await bluetoothDevice.GetCharacteristicsAsync(serviceId, [readFromDeviceCharacteristicId, writeToDeviceCharacteristicId]);
var readFromDevice = characteristics.GetCharacteristic<string>(readFromDeviceCharacteristicId);
readFromDevice.SetupNotificationHandler(message =>
{
    Console.WriteLine(message);
});

var writeToDevice = characteristics.GetCharacteristic<string>(writeToDeviceCharacteristicId);


while (true)
{
    Console.WriteLine("Message to send, or x to exit:");
    var input = Console.ReadLine();

    if (input.Equals("x", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }
    else if (!string.IsNullOrWhiteSpace(input))
    {
        await writeToDevice.WriteWithoutResponseAsync(input);
    }
}
