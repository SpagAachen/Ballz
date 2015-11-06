namespace Ballz.Network
{
    using Microsoft.Xna.Framework;
    using Messages;

    class Client
    {
        private Network network = null;

        private Connection ConnectionToServer = null;

        public Client(Network net)
        {
            this.network = net;
        }

        public void ConnectToServer(string host, int port)
        {
            this.ConnectionToServer = new Connection(host, port, 0);
        }

        public void Update(GameTime time)
        {
            //TODO: Implement
        }

        public void HandleMessage(object sender, Message message)
        {
            //TODO: handle Messages
        }
    }

}
