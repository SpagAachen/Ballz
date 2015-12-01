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
			// Entities
			if (data.GetType() == typeof(List<SEntity>))
			{
				var entities = (List<SEntity>)data;
				foreach (var e in entities)
				{
					var ourE = Ballz.The().World.EntityById(e.ID);
					ourE.Position = Utils.VectorExtensions.ToXna(e.Position);
					ourE.Velocity = Utils.VectorExtensions.ToXna(e.Velocity);
					ourE.Rotation = e.Rotation;
				}
			}
			else
				Console.WriteLine("Unknown object received: " + data.ToString());
		}

		public void HandleInputMessage(InputMessage message)
		{
			switch(message.Kind)
			{
				// Intentional fallthrough
				case InputMessage.MessageType.ControlsAction:
				case InputMessage.MessageType.ControlsUp:
				case InputMessage.MessageType.ControlsDown:
				case InputMessage.MessageType.ControlsLeft:
				case InputMessage.MessageType.ControlsRight:
					connectionToServer.Send(message);
					break;
			}
		}

        public void HandleMessage(object sender, Message message)
        {
			if (message.Kind == Message.MessageType.InputMessage)
			{
				HandleInputMessage((InputMessage)message);
			}
        }
    }

}
