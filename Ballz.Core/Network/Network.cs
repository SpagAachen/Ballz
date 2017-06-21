namespace Ballz.Network
{
    using Messages;
    using Microsoft.Xna.Framework;
    using System;

    using global::Ballz.GameSession.Logic;
    using global::Ballz.GameSession.World;

    using System.Threading.Tasks;
    using System.Net;
    using System.Diagnostics;
    using System.Threading;
    using System.Linq;
    using Lobby;
    /// <summary>
    /// Network takes care of all network related stuff independent of an existing game session.
    /// TODO
    /// - Synch map (initial is done though)
    /// - Synch entities
    /// - Synch keystrokes
    /// </summary>
    public partial class Network : GameComponent
    {
        public const int DefaultPort = 16116;
        public const string ApplicationIdentifier = "SpagAachen.Ballz";
        public const float ConnectionTimeoutSeconds = 5.0f;
        public const float WorldSyncIntervalMs = 30.0f;

        public enum NetworkRole { None, Client, Server }

        public enum NetworkGameState { None, Connecting, InLobby, InGame }

        private Server server = null;

        private Client client = null;

        public event EventHandler<Message> Message;
        public event EventHandler<object> DataReceived;

        public LobbyPlayerList PlayerList { get; private set; } = new LobbyPlayerList { PlayerNames = new string[0] };

        public Lidgren.Network.NetPeer NetworkPeer { get {
                if (server != null)
                    return server.Peer;
                else if (client != null)
                    return client.Peer;
                else
                    return null;
            }
        }
        
        public event EventHandler<LobbyPlayerList> PlayerListChanged;

        Task StartupTask = null;

        /// <summary>
        /// The state of the game: Unconnected/None, Client or Server
        /// </summary>
        public NetworkRole Role { get; private set; } = NetworkRole.None;

        public NetworkGameState GameState { get; set; } = NetworkGameState.None;
        
        public Network(Game game) : base(game)
        {
            RegisterTypes();

            PlayerListChanged += (s, list) => {
                PlayerList = list;
            };
        }

        public void RaiseMessageEvent(NetworkMessage.MessageType msg)
        {
            Message?.Invoke(this, new NetworkMessage(msg));
        }
        
        public void StartServer(FullGameInfo gameInfo)
        {
            if (Role != NetworkRole.None)
                Disconnect();
            Role = NetworkRole.Server;
            GameState = NetworkGameState.InLobby;
            server = new Server(gameInfo);
            server.PlayerListChanged += PlayerListChanged;
            server.Start();
        }

        public void StartNetworkGame(MatchSettings gameSettings, int localPlayerId)
        {
            if (Role == NetworkRole.Server)
            {
                server.StartNetworkGame(gameSettings);
                
            }
            else if (Role == NetworkRole.Client)
            {
                gameSettings.GameMode.InitializeSession(Ballz.The(), gameSettings);
                Ballz.The().Logic.StartGame(gameSettings, true, localPlayerId);
            }

            GameState = NetworkGameState.InGame;
        }

        public void ConnectToServer(IPAddress hostname, int port, Action onSuccess = null, Action onFail = null)
        {
            if (StartupTask != null)
                throw new Exception("Trying to connect to server while network is already trying to start something else");

            if (Role != NetworkRole.None)
                Disconnect();

            Role = NetworkRole.Client;
            GameState = NetworkGameState.Connecting;

            client = new Client(new IPEndPoint(hostname, port));

            client.Connected += (s, e) =>
            {
                GameState = NetworkGameState.InLobby;
                RaiseMessageEvent(NetworkMessage.MessageType.ConnectedToServer);
                onSuccess?.Invoke();
            };

            client.PlayerListChanged += (s, e) => { PlayerListChanged?.Invoke(s, e); };

            client.Start();
        }
        
        public void Disconnect()
        {
            if(client != null)
            {
                client.Stop();
                client.PlayerListChanged -= PlayerListChanged;
                client = null;
            }

            if(server != null)
            {
                server.Stop();
                server.PlayerListChanged -= PlayerListChanged;
                server = null;
            }

            Role = NetworkRole.None;
            GameState = NetworkGameState.None;
            RaiseMessageEvent(NetworkMessage.MessageType.Disconnected);
        }


        public override void Update(GameTime time)
        {
            switch (Role)
            {
                case NetworkRole.Client:
                    client?.Update(time);
                    break;
                case NetworkRole.Server:
                    server?.Update(time);
                    break;
            }
        }

        public void HandleMessage(object sender, Message message)
        {
            switch (Role)
            {
                case NetworkRole.None:
                    //TODO: Implement
                    break;
                case NetworkRole.Client:
                    client.HandleGameMessage(sender, message);
                    break;
                case NetworkRole.Server:
                    server.HandleMessage(sender, message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
