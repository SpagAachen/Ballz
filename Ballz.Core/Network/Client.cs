namespace Ballz.Network
{
    using GameSession.World;
    using Messages;
    using Microsoft.Xna.Framework;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;

    using global::Ballz.GameSession.Logic;

    class Client
    {
        private Network network = null;

        public int NumberOfPlayers { get; private set; } = -1;

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
                var data = connectionToServer.ReceiveData();
                OnData(data);
            }

            //TODO: Implement
        }

        private void OnData(object data)
        {
            // Entities
            var entities = data as IEnumerable<Entity>;
            if (entities != null)
            {
                foreach (var e in entities)
                {
                    var ourE = Ballz.The().World.EntityById(e.ID);
                    ourE.Position = e.Position;
                    ourE.Velocity = e.Velocity;
                    ourE.Rotation = e.Rotation;
                }

                return;
            }
            // network message
            var netMsg = data as NetworkMessage;
            if (netMsg != null)
            {
                switch (netMsg.Kind)
                {
                    case NetworkMessage.MessageType.NumberOfPlayers:
                        NumberOfPlayers = (int)netMsg.Data;
                        break;
                    case NetworkMessage.MessageType.StartGame:
                        Debug.Assert(netMsg.Data != null, "Received invalid game-settings");
                        ParseGameSettings((GameSettings)netMsg.Data);
                        break;
                    default:
                        Console.WriteLine("Unknown netMsg received: " + netMsg.Kind.ToString());
                        break;
                }

                return;
            }

            if (data != null)
                Console.WriteLine("Unknown object received: " + data.ToString());
            else
                Console.WriteLine("Empty data");
        }

        private void ParseGameSettings(GameSettings settings)
        {
            Ballz.The().Logic.StartGame(settings);
        }

        public void HandleInputMessage(InputMessage message)
        {
            switch (message.Kind)
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
