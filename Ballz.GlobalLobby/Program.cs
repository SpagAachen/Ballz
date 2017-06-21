using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

using RailPhase;
using Lidgren.Network;
using System.Threading;

namespace Ballz.GlobalLobby
{
    class Program
    {
        static void UdpMain(GameList gameList)
        {
            var config = new NetPeerConfiguration("SpagAachen.Ballz");
            config.Port = 16117;
            config.EnableMessageType(NetIncomingMessageType.UnconnectedData);
            var peer = new NetPeer(config);
            peer.Start();
            while(true)
            {
                Thread.Sleep(10);
                NetIncomingMessage msg;
                while((msg = peer.ReadMessage()) != null)
                {
                    switch(msg.MessageType)
                    {
                        case NetIncomingMessageType.UnconnectedData:
                            gameList.RequestAddGame(msg);
                            break;
                    }
                    peer.Recycle(msg);
                }
            }
        }

        static void Main(string[] args)
        {
            var app = new App();
            var gameList = new GameList();

            var udpThread = new Thread(() => UdpMain(gameList));
            udpThread.Start();

            app.AddView(@"^/game/list/$", ctx => gameList.RequestGetGames(ctx));
            app.AddView(@"^/game/remove/.*$", ctx => gameList.RequestRemoveGame(ctx));
            app.AddView(@"^/game/keepalive/.*$", ctx => gameList.RequestKeepalive(ctx));

            Console.WriteLine("Running Ballz.GlobalLobby...");

            app.RunHttpServer("http://*:9162/");
        }
    }
}
