namespace Ballz.Network
{
    using Microsoft.Xna.Framework;
    using Messages;
	using System;
	using System.Collections.Generic;

    class Client
    {
        private Network network = null;

        private Connection connectionToServer = null;

        public Client(Network net)
        {
            network = net;
        }

		/// <summary>
		/// Connects to server
		/// Blocking atm
		/// </summary>
		/// <param name="host">Host</param>
		/// <param name="port">Port</param>
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

		private void onData(object data)
		{
			var entities = (List<SEntity>)data;
			foreach (var e in entities) {
				Console.WriteLine("Received entity {0} ({1},{2})",e.ID, e.Position.X, e.Position.Y);
				var ourE = Ballz.The ().World.EntityById (e.ID);
				ourE.Position = Utils.VectorExtensions.ToXna (e.Position);
				ourE.Velocity = Utils.VectorExtensions.ToXna (e.Velocity);
				ourE.Rotation = e.Rotation;
			}

		}

        public void HandleMessage(object sender, Message message)
        {
            //TODO: handle Messages
        }
    }

}
