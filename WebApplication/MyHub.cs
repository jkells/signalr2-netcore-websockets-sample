using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace WebApplication
{
    public class MyHub : Hub
    {
        public override Task OnConnected()
        {
            Clients.Caller.HelloWorld();
            return Task.CompletedTask;
        }
    }
}