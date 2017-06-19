namespace Ballz.Network
{
    using Messages;
    using Microsoft.Xna.Framework;
    using System;

    using global::Ballz.GameSession.Logic;
    using global::Ballz.GameSession.World;

    using ObjectSync;
    using System.Threading.Tasks;

    /// <summary>
    /// Network takes care of all network related stuff independent of an existing game session.
    /// TODO
    /// - Synch map (initial is done though)
    /// - Synch entities
    /// - Synch keystrokes
    /// </summary>
    public class Network : GameComponent
    {

        public enum NetworkRole { None, Client, Server }

        public enum NetworkGameState { None, Connecting, InLobby, InGame }

        private Server server;

        private Client client;

        public event EventHandler<Message> Message;
        public event EventHandler<object> DataReceived;
        public event EventHandler<LobbyPlayerList> PlayerListChanged;

        Task StartupTask = null;

        /// <summary>
        /// The state of the game: Unconnected/None, Client or Server
        /// </summary>
        public NetworkRole Role { get; private set; } = NetworkRole.None;

        public NetworkGameState GameState { get; set; } = NetworkGameState.None;

        public void RaiseMessageEvent(NetworkMessage.MessageType msg)
        {
            Message?.Invoke(this, new NetworkMessage(msg));
        }

        public int GetNumberOfPlayers()
        {
            if (Role == NetworkRole.Client) return client.NumberOfPlayers;
            if (Role == NetworkRole.Server) return server.NumberOfClients() + 1; // +1 for ourselfs
            return -1;
        }

        public void StartServer()
        {
            if (Role != NetworkRole.None)
                Disconnect();
            Role = NetworkRole.Server;
            GameState = NetworkGameState.InLobby;
            server = new Server();
            server.Listen();
            RaiseMessageEvent(NetworkMessage.MessageType.ServerStarted);
        }

        public void StartNetworkGame(GameSettings gameSettings)
        {
            if (Role == NetworkRole.Server)
            {
                server.StartNetworkGame(gameSettings);
                GameState = NetworkGameState.InGame;
            }
        }

        public void ConnectToServer(string hostname, int port, Action onSuccess = null, Action onFail = null)
        {
            if (StartupTask != null)
                throw new Exception("Trying to connect to server while network is already trying to start something else");

            if (Role != NetworkRole.None)
                Disconnect();

            Role = NetworkRole.Client;
            client = new Client(this);
            RaiseMessageEvent(NetworkMessage.MessageType.ConnectingToServer);

            GameState = NetworkGameState.Connecting;

            StartupTask = new Task(() =>
            {
                try
                {
                    client.ConnectToServer(hostname, port); // blocking atm
                }
                catch (Exception e)
                {
                    RaiseMessageEvent(NetworkMessage.MessageType.ConnectionErrorOccured);
                    MessageOverlay.ShowAlert("Error while connecting", e.Message);
                    GameState = NetworkGameState.None;
                    onFail?.Invoke();
                    return;
                }

                GameState = NetworkGameState.InLobby;
                RaiseMessageEvent(NetworkMessage.MessageType.ConnectedToServer);
                StartupTask = null;
                onSuccess?.Invoke();
            });
            StartupTask.Start();
        }

        public void Disconnect()
        {
            if (Role == NetworkRole.None) return;
            Role = NetworkRole.None;
            client = null;
            server = null;
            RaiseMessageEvent(NetworkMessage.MessageType.Disconnected);
            //TODO: Implement
        }

        public Network(Game game) : base(game)
        {
            SynchronizingInfo.Register(new SynchronizingInfo
            {
                Type = typeof(LobbyPlayerList),
                IsIdentifiable = false,
                ObjectConstructor = () => new LobbyPlayerList()
            });
        }

        public override void Update(GameTime time)
        {
            switch (Role)
            {
                case NetworkRole.None:
                    return;
                case NetworkRole.Client:
                    client.Update(time);
                    break;
                case NetworkRole.Server:
                    server.Update(time);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
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
                    client.HandleMessage(sender, message);
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
