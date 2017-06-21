using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RailPhase;
using Newtonsoft.Json;
using System.IO;
using Lidgren.Network;

using Ballz.Lobby;

namespace Ballz.GlobalLobby
{
    public class GameList
    {
        public Dictionary<string, FullGameInfo> Games = new Dictionary<string, FullGameInfo>();
        public int MaxLobbySize = 50000;
        public int MaxRequestBodySize = 10000;

        public TimeSpan Timeout = TimeSpan.FromSeconds(5.0);
        public DateTime LastTimeoutCleaning = DateTime.MinValue;
        public void RemoveOldEntries()
        {
            var now = DateTime.Now;
            // Only remove old entries once a second
            if ((now - LastTimeoutCleaning).TotalSeconds > 1.0)
            {
                var minKeepaliveTime = now - Timeout;
                lock (Games)
                {
                    var oldGames = Games.Values.Where(g => g.LastKeepAlive < minKeepaliveTime).Select(g => g.PublicId).ToArray();
                    foreach (var key in oldGames)
                    {
                        Games.Remove(key);
                    }
                }
                LastTimeoutCleaning = now;
            }
        }

        public void AddGame(FullGameInfo game)
        {
            lock (Games)
            {
                if (Games.Count > MaxLobbySize)
                    throw new InvalidOperationException("Too many games in list.");

                Games[game.PublicId] = game;
            }
        }

        public void RequestGetGames(Context ctx)
        {
            RemoveOldEntries();

            ctx.Response.ContentType = "text/json";

            PublicGameInfo[] gameList;

            lock (Games)
            {
                gameList = Games.Values.Select(g => g.PublicInfo()).ToArray();
            }

            ctx.WriteResponse(JsonConvert.SerializeObject(gameList));
        }

        public void RequestAddGame(NetIncomingMessage msg)
        {
            var data = msg.ReadString();
            var game = JsonConvert.DeserializeObject<FullGameInfo>(data);

            var hostAddress = msg.SenderEndPoint.Address.ToString();
            var hostPort = msg.SenderEndPoint.Port;

            game.HostAddress = hostAddress;
            game.HostPort = hostPort;
            game.LastKeepAlive = DateTime.Now;
            AddGame(game);
        }
        
        public void RequestRemoveGame(Context ctx)
        {
            var id = ctx.Request.QueryString["id"];
            var secret = ctx.Request.QueryString["secret"];

            // will throw if id is invalid
            var game = Games[id];

            if (game.Secret == secret)
            {
                Games.Remove(id);
            }
            else
            {
                throw new InvalidDataException("Incorrect Secret");
            }

            ctx.Response.ContentType = "text/json";
            ctx.WriteResponse("true");
        }


        public void RequestKeepalive(Context ctx)
        {
            var id = ctx.Request.QueryString["id"];
            var secret = ctx.Request.QueryString["secret"];

            // will throw if id is invalid
            var game = Games[id];
            
            if(game.Secret == secret)
            {
                game.LastKeepAlive = DateTime.Now;
            }
            else
            {
                throw new InvalidDataException("Incorrect Secret");
            }

            ctx.Response.ContentType = "text/json";
            ctx.WriteResponse("true");
        }
    }
}
