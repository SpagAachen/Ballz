namespace Ballz.Network
{
    using Messages;
    using Microsoft.Xna.Framework;
    using System;

    /// <summary>
    /// Network takes care of all network related stuff independent of an existing game session.
    /// </summary>
    public class Network : GameComponent
    {
        public enum StateT { None, Client, Server };
        private Server server;
        private Client client;
        public event EventHandler<Message> Message;

        /// <summary>
        /// The state of the game: Unconnected/None, Client or Server
        /// </summary>
        public StateT State { get; private set; } = StateT.None;

        public void RaiseMessageEvent(NetworkMessage.MessageType msg)
        {
            Message?.Invoke(this, new NetworkMessage(msg));
        }

        public void StartServer(int port)
        {
            if (State != StateT.None)
                Disconnect();
            State = StateT.Server;
            server = new Server(this);
            server.Listen(port);
            RaiseMessageEvent(NetworkMessage.MessageType.ServerStarted);
            //TODO: Implement
        }

        public void ConnectToServer(string hostname, int port)
        {
            if (State != StateT.None)
                Disconnect();
            State = StateT.Client;
            client = new Client(this);
			RaiseMessageEvent(NetworkMessage.MessageType.ConnectingToServer);
            client.ConnectToServer(hostname, port); // blocking atm
			RaiseMessageEvent(NetworkMessage.MessageType.ConnectedToServer);
            //TODO: Implement
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
