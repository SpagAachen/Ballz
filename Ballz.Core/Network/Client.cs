namespace Ballz.Network
{
    using Microsoft.Xna.Framework;
    using Messages;
	using System;

    class Client
    {
        private Network network = null;

        private Connection connectionToServer = null;

        public Client(Network net)
        {
            network = net;
        }

        public void ConnectToServer(string host, int port)
        {
            connectionToServer = new Connection(host, port, 0);
        }

        public void Update(GameTime time)
        {
			if (connectionToServer.DataAvailable())
			{
				var data = connectionToServer.Receive();
				foreach (var d in data)
					onData(d);
			}

            //TODO: Implement
        }

		private void onData(string data)
		{
			Console.WriteLine("Received data from SERVER: " + data);
		}

        public void HandleMessage(object sender, Message message)
        {
            //TODO: handle Messages
        }
    }

}
