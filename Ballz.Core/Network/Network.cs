namespace Ballz.Network
{
    using Messages;
    using Microsoft.Xna.Framework;
    using System;

    /// <summary>
    /// Network takes care of all network related stuff independent of an existing game session.
    /// </summary>
    class Network : GameComponent
    {
        internal enum StateT { None, Client, Server };
        private Server server;
        private Client client;
        public event EventHandler<Message> Message;

        /// <summary>
        /// The state of the game: Unconnected/None, Client or Server
        /// </summary>
        public StateT State { get; private set; } = StateT.None;

        public void RaiseMessageEvent(NetworkMessage.MessageType msg)
        {
            this.Message?.Invoke(this, new NetworkMessage(msg));
        }

        public void StartServer(int port)
        {
            if (this.State != StateT.None)
                this.Disconnect();
            this.State = StateT.Server;
            this.server = new Server(this);
            this.server.Listen(port);
            this.RaiseMessageEvent(NetworkMessage.MessageType.ServerStarted);
            //TODO: Implement
        }

        public void ConnectToServer(string hostname, int port)
        {
            if (this.State != StateT.None)
                this.Disconnect();
            this.State = StateT.Client;
            this.client = new Client(this);
            this.client.ConnectToServer(hostname, port);
            this.RaiseMessageEvent(NetworkMessage.MessageType.ConnectingToServer);
            //TODO: Implement
        }

        public void Disconnect()
        {
            if (this.State == StateT.None) return;
            this.State = StateT.None;
            this.client = null;
            this.server = null;
            this.RaiseMessageEvent(NetworkMessage.MessageType.Disconnected);
            //TODO: Implement
        }

        public Network(Game game) : base(game)
        {
        }

        public override void Update(GameTime time)
        {
            switch (this.State)
            {
                case StateT.None:
                    return;
                case StateT.Client:
                    this.client.Update(time);
                    break;
               case StateT.Server:
                    this.server.Update(time);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void HandleMessage(object sender, Message message)
        {
            switch (this.State)
            {
                case StateT.None:
                    //TODO: Implement
                    break;
                case StateT.Client:
                    this.client.HandleMessage(sender, message);
                    break;
                case StateT.Server:
                    this.server.HandleMessage(sender, message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
