namespace Comms.WebSockets.Example.Client;

internal static class ConsoleConfigurator
{
    public static (bool DoRun, string BaseUrl) ConfigureConsoleApp(string initialBaseUrl)
    {
        var doRun = false;

        while (!doRun)
        {
            Task.Delay(300).Wait();
            Console.WriteLine($"\n\nReady to run the websocket client on url '{initialBaseUrl}'.");
            Console.WriteLine("Enter:");
            Console.WriteLine("- s\t\t: to start the client with the current settings");
            Console.WriteLine("- url>{URL}\t: to define the base url.\texample: url>ws://192.168.1.116/ws");
            Console.WriteLine("- x\t\t: to exit");

            var userInput = Console.ReadLine();

            if (userInput.Equals("x", StringComparison.OrdinalIgnoreCase))
            {
                return (false, string.Empty);
            }

            if (userInput.StartsWith("url>", StringComparison.OrdinalIgnoreCase))
            {
                var url = userInput.Substring("url>".Length);
                try
                {
                    var uri = new Uri(url);
                    initialBaseUrl = url;
                }
                catch (Exception)
                {
                    Console.WriteLine($"'{url}' does not seem to be a valid url.");
                }
            }

            if (userInput.Equals("s", StringComparison.OrdinalIgnoreCase))
            {
                return (true, initialBaseUrl);
            }
        }

        return (false, string.Empty);
    }
}
