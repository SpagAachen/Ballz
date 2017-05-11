using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Ballz.Lobby
{
    public class LobbyClient
    {
        public const string GlobalLobbyHost = "localhost";
        public const int GlobalLobbyPort = 18080;

        static RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();
        static string GetRandomString()
        {
            var data = new byte[16];
            random.GetBytes(data);
            return Convert.ToBase64String(data);
        }

        FullGameInfo HostedGame;
        Task GameOpeningTask;
        Task<PublicGameInfo[]> GameListTask;
        DateTime LastKeepalive = DateTime.MinValue;
        DateTime LastGameListRefresh = DateTime.MinValue;

        public event EventHandler<PublicGameInfo[]> UpdatedGameList = null;

        public void HostGame(string name, bool isPrivate)
        {
            if (HostedGame != null)
                throw new InvalidOperationException("Already hosting a game");

            var id = GetRandomString();
            var secret = GetRandomString();

            var game = new FullGameInfo
            {
                Name = name,
                IsPrivate = isPrivate,
                PublicId = id,
                Secret = secret
            };

            HostedGame = game;
            GameOpeningTask = OpenHostedGameAsync();
            LastKeepalive = DateTime.Now;
        }

        public void Update()
        {
            var now = DateTime.Now;
            if (HostedGame != null)
            {
                if (GameOpeningTask.IsCompleted && (now - LastKeepalive).TotalSeconds > 2.0)
                {
                    // If registering the game at the lobby failed, try to open it again.
                    if ((GameOpeningTask.IsFaulted || GameOpeningTask.IsCanceled))
                    {
                        GameOpeningTask = OpenHostedGameAsync();
                    }
                    else
                    {
                        SendKeepaliveAsync();
                    }

                    LastKeepalive = now;
                }
            }
            if(UpdatedGameList != null)
            {
                // If a new list of games arrived, send an event
                if (GameListTask != null && GameListTask.IsCompleted)
                {
                    var games = GameListTask.Result;
                    UpdatedGameList?.Invoke(this, games);
                }

                // Refresh the game list if we never did so before, or if the last refresh was more than 5 seconds ago
                if (GameListTask == null || (now - LastGameListRefresh).TotalSeconds > 5.0)
                {
                    GameListTask = GetGameListAsync();
                    LastGameListRefresh = now;
                }
            }
        }

        public async Task OpenHostedGameAsync()
        {
            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(HostedGame));
            var result = RequestToLobbyAsync("/game/add", postBody: data, usePostMethod: true);
            HostedGame = null;
            var response = await result;
            if (response != "true")
                throw new WebException("Opening Game request to global lobby was unsuccessful.");
        }

        public async Task CloseHostedGameAsync()
        {
            var requestParams = new Dictionary<string, string> { { "id", HostedGame.PublicId }, { "secret", HostedGame.Secret } };
            var result = RequestToLobbyAsync("/game/remove", requestParams, usePostMethod: true);
            HostedGame = null;
            var response = await result;
            if (response != "true")
                throw new WebException("Closing Game request to global lobby was unsuccessful.");
        }

        public async Task SendKeepaliveAsync()
        {
            var requestParams = new Dictionary<string, string> { { "id", HostedGame.PublicId }, { "secret", HostedGame.Secret } };
            var result = RequestToLobbyAsync("/game/keepalive", requestParams, usePostMethod: true);
            HostedGame = null;
            var response = await result;
            if (response != "true")
                throw new WebException("Sending Keepalive request to global lobby was unsuccessful.");
        }

        public async Task<PublicGameInfo[]> GetGameListAsync()
        {
            var result = RequestToLobbyAsync("/game/list");
            var response = await result;
            return JsonConvert.DeserializeObject<PublicGameInfo[]>(response);
        }
        
        public async Task<string> RequestToLobbyAsync(string path, Dictionary<string, string> getParams = null, byte[] postBody = null, bool usePostMethod = false)
        {
            Task<HttpResponseMessage> result;
            using (HttpClient client = new HttpClient())
            {
                var uri = new UriBuilder("http", GlobalLobbyHost, GlobalLobbyPort, path);
                
                if(getParams != null)
                {
                    uri.Query = getParams.Aggregate("?", (s, kvp) => s + Uri.EscapeUriString(kvp.Key) + "=" + Uri.EscapeUriString(kvp.Value));
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
            }

            var response = await result;
            if (!response.IsSuccessStatusCode)
                throw new System.Net.WebException($"Lobby HTTP Request to path {path} failed with status code {response.StatusCode.ToString()}.");

            var responseBody = await response.Content.ReadAsStringAsync();

            return responseBody;
        }

    }
}
