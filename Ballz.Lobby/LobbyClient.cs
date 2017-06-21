using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using Lidgren.Network;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Ballz.Lobby
{
    public class LobbyClient: IDisposable
    {
#if __DEBUG_LOBBY_LOCALHOST
        public const string GlobalLobbyScheme = "http";
        public const string GlobalLobbyHost = "localhost";
        public const int GlobalLobbyPort = 18080;
#else
        public const string GlobalLobbyScheme = "https";
        public const string GlobalLobbyHost = "lobby.lb2.eu";
        public const int GlobalLobbyPort = 443;
#endif

        static RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();
        static string GetRandomString()
        {
            var data = new byte[16];
            random.GetBytes(data);
            return BitConverter.ToString(data).Replace("-", string.Empty);
        }

        FullGameInfo HostedGame;
        Task<PublicGameInfo[]> GameListTask;
        DateTime LastKeepalive = DateTime.MinValue;
        DateTime LastGameListRefresh = DateTime.MinValue;

        NetClient LocalDiscoveryPeer;
        Stopwatch LocalDiscoveryTimer = new Stopwatch();
        Dictionary<string, PublicGameInfo> LocalGamesById = new Dictionary<string, PublicGameInfo>();

        public IEnumerable<PublicGameInfo> LocalGames { get { return LocalGamesById.Values; } }

        public event EventHandler<PublicGameInfo[]> UpdatedOnlineGameList = null;
        public event EventHandler<PublicGameInfo[]> UpdatedLocalGameList = null;

        public LobbyClient()
        {
        }

        public void StartLocalDiscovery()
        {
			var peerConfig = new NetPeerConfiguration("SpagAachen.Ballz");
			peerConfig.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
			LocalDiscoveryPeer = new NetClient(peerConfig);
			LocalDiscoveryPeer.Start();
			LocalDiscoveryTimer.Start();
		}

        public FullGameInfo MakeGameInfo(string name, bool isPrivate)
        {
            var id = GetRandomString();
            var secret = GetRandomString();

            var game = new FullGameInfo
            {
                Name = name,
                IsPrivate = isPrivate,
                PublicId = id,
                Secret = secret
            };

            return game;
        }
        public void OpenGame(FullGameInfo game, NetPeer serverPeer)
        {
			if (HostedGame != null)
				throw new InvalidOperationException("Already hosting a game");

			HostedGame = game;
            OpenHostedGame(serverPeer);
            LastKeepalive = DateTime.Now;
        }
        
        public void Update()
        {
            var now = DateTime.Now;
            if (HostedGame != null)
            {
                if ((now - LastKeepalive).TotalSeconds > 2.0)
                {
                    SendKeepaliveAsync();
                    LastKeepalive = now;
                }
            }
            if(UpdatedOnlineGameList != null)
            {
                // If a new list of games arrived, send an event
                if (GameListTask != null && GameListTask.Status == TaskStatus.RanToCompletion)
                {
                    var games = GameListTask.Result;
                    UpdatedOnlineGameList?.Invoke(this, games);
                    GameListTask = null;
                }

                // Refresh the game list if we never did so before, or if the last refresh was more than 5 seconds ago
                if (GameListTask == null && (now - LastGameListRefresh).TotalSeconds > 5.0)
                {
                    GameListTask = GetGameListAsync();
                    LastGameListRefresh = now;
                }
            }

            if (LocalDiscoveryPeer != null)
            {
                bool newLocalGames = false;
                NetIncomingMessage msg;
                while ((msg = LocalDiscoveryPeer.ReadMessage()) != null)
                {
                    switch (msg.MessageType)
                    {
                        case NetIncomingMessageType.ErrorMessage:
                            var error = msg.ReadString();
                            Console.WriteLine("Lidgren Error: " + error);
                            break;
                        case NetIncomingMessageType.DiscoveryResponse:
                            var gameInfoSerialized = msg.ReadString();
                            var gameInfo = JsonConvert.DeserializeObject<PublicGameInfo>(gameInfoSerialized);
                            gameInfo.HostAddress = msg.SenderEndPoint.Address.ToString();
                            gameInfo.HostPort = msg.SenderEndPoint.Port;
                            LocalGamesById[gameInfo.PublicId] = gameInfo;
                            newLocalGames = true;
                            break;

                    }
                    LocalDiscoveryPeer.Recycle(msg);
                }

                if (newLocalGames)
                {
                    UpdatedLocalGameList?.Invoke(this, LocalGamesById.Values.ToArray());
                }

                if (LocalDiscoveryTimer.ElapsedMilliseconds > 1000)
                {
                    LocalDiscoveryPeer.DiscoverLocalPeers(16116);
                    LocalDiscoveryTimer.Restart();
                }
            }
        }

        public void OpenHostedGame(NetPeer serverPeer)
        {
            Console.WriteLine("Open Game");
            var msg = serverPeer.CreateMessage();
            var data = JsonConvert.SerializeObject(HostedGame);
            msg.Write(data);
            serverPeer.SendUnconnectedMessage(msg, GlobalLobbyHost, 43117);
        }

        public async Task CloseHostedGameAsync()
        {
            var requestParams = new Dictionary<string, string> { { "id", HostedGame.PublicId }, { "secret", HostedGame.Secret } };
            var result = RequestToLobbyAsync("/game/remove", requestParams, usePostMethod: true);
            var response = await result;
            if (response != "true")
                throw new WebException("Closing Game request to global lobby was unsuccessful.");
        }

        public async Task SendKeepaliveAsync()
        {
            var requestParams = new Dictionary<string, string> { { "id", HostedGame.PublicId }, { "secret", HostedGame.Secret } };
            var result = RequestToLobbyAsync("/game/keepalive/", requestParams, usePostMethod: true);
            var response = await result;
            if (response != "true")
                throw new WebException("Sending Keepalive request to global lobby was unsuccessful.");
        }

        public async Task<PublicGameInfo[]> GetGameListAsync()
        {
            var result = RequestToLobbyAsync("/game/list/");
            var response = await result;
            return JsonConvert.DeserializeObject<PublicGameInfo[]>(response);
        }
        
        public async Task<string> RequestToLobbyAsync(string path, Dictionary<string, string> getParams = null, byte[] postBody = null, bool usePostMethod = false)
        {
            Task<HttpResponseMessage> result;
            var client = new HttpClient();
            var uri = new UriBuilder(GlobalLobbyScheme, GlobalLobbyHost, GlobalLobbyPort, path);
                
            if(getParams != null)
            {
                var paramArray = getParams
                                .Select(kvp => Uri.EscapeUriString(kvp.Key) + "=" + Uri.EscapeUriString(kvp.Value))
                                .ToArray();

                uri.Query = string.Join("&", paramArray);
            }
                
            if (usePostMethod)
            {
                var content = new ByteArrayContent(postBody ?? new byte[0]);
                result = client.PostAsync(uri.Uri, content);
            }
            else
            {
                result = client.GetAsync(uri.Uri);
            }

            var response = await result;

            if (!response.IsSuccessStatusCode)
                throw new System.Net.WebException($"Lobby HTTP Request to path {path} failed with status code {response.StatusCode.ToString()}.");

            var responseBody = await response.Content.ReadAsStringAsync();

            return responseBody;
        }

#region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //GameListTask?.Dispose();
                    GameListTask = null;

                    UpdatedOnlineGameList = null;
                    HostedGame = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }
        
        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
#endregion

    }
}
