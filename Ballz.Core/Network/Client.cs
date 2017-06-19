namespace Ballz.Network
{
    using GameSession.World;
    using Messages;
    using Microsoft.Xna.Framework;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using ObjectSync;


    using global::Ballz.GameSession.Logic;
    using Lidgren.Network;
    using System.Net;
    using System.Threading;

    class Client
    {
        ObjectSynchronizer Sync;
        NetClient Peer;
        
        public int NumberOfPlayers { get; private set; } = -1;

        public event EventHandler Connected;
        public event EventHandler Disconnected;
        public event EventHandler<LobbyPlayerList> PlayerListChanged;

        public bool IsConnected { get; private set; } = false;

        IPEndPoint Host;

        public Client(IPEndPoint host)
        {
            Host = host;
            var conf = new NetPeerConfiguration("SpagAachen.Ballz");
            conf.ConnectionTimeout = 10;
            conf.PingInterval = 2;
            Peer = new NetClient(conf);

            Sync = new ObjectSynchronizer(Peer);
            Sync.NewObjectReceived += (s, data) => OnData(data);
        }

        public void Start()
        {
            Peer.Start();
            Peer.Connect(Host);
        }
        
        public void Update(GameTime time)
        {
            NetIncomingMessage im;
            while ((im = Peer.ReadMessage()) != null)
            {
                // handle incoming message
                switch (im.MessageType)
                {
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.ErrorMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.VerboseDebugMessage:
                        Console.WriteLine($"NetMessage {im.MessageType}: {im.ReadString()}");
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        NetConnectionStatus status = (NetConnectionStatus)im.ReadByte();

                        if (status == NetConnectionStatus.Connected)
                        {
                            IsConnected = true;
                            Sync.AddConnection(im.SenderConnection);
                            Connected?.Invoke(this, null);
                            SendToServer(new LobbyPlayerGreeting { PlayerName = Environment.MachineName }); // TODO: Use actual player name
                        }
                        else
                        {
                            IsConnected = false;
                        }

                        if (status == NetConnectionStatus.Disconnected)
                        {
                            Disconnected?.Invoke(this, null);
                        }

                        break;
                    case NetIncomingMessageType.Data:
                        Sync.ReadMessage(im);
                        break;
                    default:
                        Console.WriteLine($"Unhandled message type: {im.MessageType} {im.LengthBytes} bytes");
                        break;
                }
                Peer.Recycle(im);
            }
        }

        private void OnData(object data)
        {
            if(data is LobbyPlayerList)
            {
                PlayerListChanged?.Invoke(this, data as LobbyPlayerList);
            }

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
                        break;
                    case NetworkMessage.MessageType.YourPlayerId:
                        //connectionToServer.ClientPlayerId = (int)netMsg.Data;
                        break;
                    case NetworkMessage.MessageType.EntityRemoved:
                        var e = netMsg.Data as Entity;
                        var localEntity = Ballz.The().Match.World.EntityById(e.ID);
                        Ballz.The().Match.World.RemoveEntity(localEntity);
                        localEntity.Dispose();
                        break;
                    default:
                        break;
                }

                return;
            }

            var terrainModification = data as Terrain.TerrainModification;
            if(terrainModification != null)
            {
                Ballz.The().Match.World.StaticGeometry.ApplyModification(terrainModification);
                return;
            }
        }

        public void Stop()
        {
            Peer.Shutdown("Graceful Shutdown");
        }

        public void SendToServer(object data, bool reliable = true)
        {
            Sync.SendObject(data, reliable);
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
                    //connectionToServer?.Send(message);
                    break;
            }
        }

        public void HandleGameMessage(object sender, Message message)
        {
            if (message.Kind == Message.MessageType.InputMessage)
            {
                HandleInputMessage((InputMessage)message);
            }
        }
    }
}
