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
        static Client()
        {
            ObjectSync.Sync.RegisterClass<Entity>(() => new Entity());
            ObjectSync.Sync.RegisterClass<Ball>(() => new Ball());
            ObjectSync.Sync.RegisterClass<Shot>(() => new Shot());
            ObjectSync.Sync.RegisterClass<Message>(() => new Message());
            ObjectSync.Sync.RegisterClass<NetworkMessage>(() => new NetworkMessage());
            ObjectSync.Sync.RegisterClass<InputMessage>(() => new InputMessage());
        }

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
            connectionToServer.ObjectReceived += OnData;
        }

        public void Update(GameTime time)
        {
            connectionToServer.ReadUpdates();
        }

        private void OnData(object sender, object data)
        {
            // Entities
            var entity = data as Entity;
            if (entity != null)
            {
                var localEntity = Ballz.The().Match.World.EntityById(entity.ID);
                if (localEntity != null)
                    ObjectSync.Sync.SyncState(entity, localEntity);
                else
                    Ballz.The().Match.World.AddEntity(entity);
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
                    case NetworkMessage.MessageType.YourPlayerId:
                        connectionToServer.ClientPlayerId = (int)netMsg.Data;
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
            var localPlayer = Ballz.The().Match.PlayerById(connectionToServer.ClientPlayerId);
            localPlayer.IsLocal = true;
            Ballz.The().Match.LocalPlayers = new List<Player>() { localPlayer };
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
                case InputMessage.MessageType.ControlsJump:
                case InputMessage.MessageType.ControlsNextWeapon:
                case InputMessage.MessageType.ControlsPreviousWeapon:
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
