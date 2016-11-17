using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;

namespace ConsoleApplication
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // This is a demo, give the web project long enough to start.
            RunAsync().Wait();
            Console.WriteLine("Done");
            Console.ReadKey();
        }

        private static async Task RunAsync()
        {
            var tcs = new TaskCompletionSource<object>();

            var connection = new HubConnection("http://localhost:5000/signalr/hubs");
            connection.StateChanged +=
                change =>
                {
                    Console.WriteLine($"StateChanged: {change.NewState}. Transport: {connection.Transport?.Name}");
                };

            var hub = connection.CreateHubProxy("MyHub");
            hub.On("helloWorld", () =>
            {
                Console.WriteLine("Hello World Called.");
                tcs.SetResult(null);
            });

            await connection.Start();
            await tcs.Task;
        }
    }
}