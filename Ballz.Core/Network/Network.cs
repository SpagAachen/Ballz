namespace Ballz.Network
{
    using Messages;
    using Microsoft.Xna.Framework;
    using System;

    using global::Ballz.GameSession.Logic;
    using global::Ballz.GameSession.World;

    using ObjectSync;

    /// <summary>
    /// Network takes care of all network related stuff independent of an existing game session.
    /// TODO
    /// - Synch map (initial is done though)
    /// - Synch entities
    /// - Synch keystrokes
    /// </summary>
    public class Network : GameComponent
    {
        public enum StateT { None, Client, Server }

        public enum GameStateT { None, InLobby, InGame }

        private Server server;

        private Client client;

        public event EventHandler<Message> Message;

        /// <summary>
        /// The state of the game: Unconnected/None, Client or Server
        /// </summary>
        public StateT State { get; private set; } = StateT.None;

        public GameStateT GameState { get; set; } = GameStateT.None;

        public void RaiseMessageEvent(NetworkMessage.MessageType msg)
        {
            Message?.Invoke(this, new NetworkMessage(msg));
        }

        public int GetNumberOfPlayers()
        {
            if (State == StateT.Client) return client.NumberOfPlayers;
            if (State == StateT.Server) return server.NumberOfClients() + 1; // +1 for ourselfs
            return -1;
        }

        public void StartServer(int port)
        {
            if (State != StateT.None)
                Disconnect();
            State = StateT.Server;
            server = new Server(this);
            server.Listen(port);
            RaiseMessageEvent(NetworkMessage.MessageType.ServerStarted);
            Console.WriteLine("Started server on port " + port);
            //TODO: Implement
        }

        public void StartNetworkGame(GameSettings gameSettings)
        {
            if (State == StateT.Server)
            {
                server.StartNetworkGame(gameSettings);
                GameState = GameStateT.InGame;
            }
        }

        public bool ConnectToServer(string hostname, int port)
        {
            if (State != StateT.None)
                Disconnect();
            State = StateT.Client;
            client = new Client(this);
            RaiseMessageEvent(NetworkMessage.MessageType.ConnectingToServer);

            try
            {
                client.ConnectToServer(hostname, port); // blocking atm
            }
            catch(Exception e)
            {
                RaiseMessageEvent(NetworkMessage.MessageType.ConnectionErrorOccured);
                Console.WriteLine("Unable to connect");
                MessageOverlay.ShowAlert("Error while connecting", e.Message);
                return false;
            }

            RaiseMessageEvent(NetworkMessage.MessageType.ConnectedToServer);
            Console.WriteLine("Connected to server");
            return true;
        }

        public void Disconnect()
        {
            if (State == StateT.None) return;
            State = StateT.None;
            client = null;
            server = null;
            RaiseMessageEvent(NetworkMessage.MessageType.Disconnected);
            //TODO: Implement
        }

        public Network(Game game) : base(game)
        {
            Sync.RegisterClass(() => new Entity());
        }

        public override void Update(GameTime time)
        {
            switch (State)
            {
                case StateT.None:
                    return;
                case StateT.Client:
                    client.Update(time);
                    break;
                case StateT.Server:
                    server.Update(time);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void HandleMessage(object sender, Message message)
        {
            switch (State)
            {
                case StateT.None:
                    //TODO: Implement
                    break;
                case StateT.Client:
                    client.HandleMessage(sender, message);
                    break;
                case StateT.Server:
                    server.HandleMessage(sender, message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
