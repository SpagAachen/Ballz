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
    using GameSession;

    class Client
    {
        ObjectSynchronizer Sync;
        public NetClient Peer { get; private set; }
        
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
                            SendToServer(new LobbyPlayerGreeting { PlayerName = Ballz.The().Settings.PlayerName }); // TODO: Use actual player name
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

            if(data is GameStartInfo)
            {
                HandleMatchStart(data as GameStartInfo);
            }

            if(data is Session.NetSessionState)
            {
                Ballz.The().Match.ApplyState(data as Session.NetSessionState);
            }

            // Entities
            var entity = data as Entity;
            if (entity != null)
            {
                // Same entity already exists?
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
                    //case NetworkMessage.MessageType.YourPlayerId:
                    //    //connectionToServer.ClientPlayerId = (int)netMsg.Data;
                    //    break;
                    //case NetworkMessage.MessageType.EntityRemoved:
                    //    var e = netMsg.Data as Entity;
                    //    var localEntity = Ballz.The().Match.World.EntityById(e.ID);
                    //    Ballz.The().Match.World.RemoveEntity(localEntity);
                    //    localEntity.Dispose();
                    //    break;
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

        private void HandleMatchStart(GameStartInfo startInfo)
        {
            var settings = MatchSettings.Deserialize(startInfo.Settings);
            Ballz.The().Network.StartNetworkGame(settings, startInfo.YourPlayerId);
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
                    SendToServer(message);
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
