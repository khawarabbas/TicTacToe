using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;
using System;
using System.Threading.Tasks;

[assembly: OwinStartup(typeof(TicTacToe.Startup1))]

namespace TicTacToe
{
    public class Startup1
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapHubs();
            GlobalHost.Configuration.MaxIncomingWebSocketMessageSize = null;

            // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=316888
        }
    }
}
