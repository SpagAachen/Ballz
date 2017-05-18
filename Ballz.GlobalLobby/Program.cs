using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

using RailPhase;

namespace Ballz.GlobalLobby
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new App();
            var gameList = new GameList();

            gameList.AddGame(new Lobby.FullGameInfo
            {
                Name = "Hey",
                HostAddress = "Bla",
                HostPort = 123,
                IsPrivate = false,
                PublicId = "sald7a8h",
                LastKeepAlive = DateTime.Now,
                Secret = "sa87hdas9asn98"
            });
            gameList.AddGame(new Lobby.FullGameInfo
            {
                Name = "sdfds",
                HostAddress = "322",
                HostPort = 456,
                IsPrivate = false,
                PublicId = "vbgf",
                LastKeepAlive = DateTime.MaxValue,
                Secret = "6jhdf"
            });

            app.AddView(@"^/game/list/$", ctx => gameList.RequestGetGames(ctx));
            app.AddView(@"^/game/add/$", ctx => gameList.RequestAddGame(ctx));
            app.AddView(@"^/game/remove/$", ctx => gameList.RequestRemoveGame(ctx));
            app.AddView(@"^/game/keepalive/$", ctx => gameList.RequestKeepalive(ctx));
            app.RunHttpServer("http://*:9162/");
        }
    }
}
