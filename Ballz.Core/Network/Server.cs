namespace Ballz.Network
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using Messages;
    using Microsoft.Xna.Framework;
    using System.Net.Sockets;

    using global::Ballz.GameSession.Logic;

    using Microsoft.Xna.Framework.Graphics;
    using GameSession.World;
    using System.Linq;
    using GameSession;
    using Lidgren.Network;
    using System.Threading;

    public class PlayerConnection
    {
        public string PlayerName;
        public int MatchPlayerId;
        public Player MatchPlayer;
        public NetConnection Connection;
    }

    public class Server
    {
        static Server()
        {

        }

        private static int nextId = 1;
        
        NetServer Peer;
        ObjectSynchronizer Sync = null;

        Dictionary<long,PlayerConnection> ConnectedPlayers = new Dictionary<long, PlayerConnection>();

        public event EventHandler<LobbyPlayerList> PlayerListChanged;

        public double TicksPerSecond = 30;

        public Server()
        {
            var config = new NetPeerConfiguration(Network.ApplicationIdentifier);
            config.Port = Network.DefaultPort; // TODO: make port configurable
            config.ConnectionTimeout = Network.ConnectionTimeoutSeconds;
            config.AcceptIncomingConnections = true;
            Peer = new NetServer(config);
            Sync = new ObjectSynchronizer(Peer);
            Sync.NewObjectReceived += OnData;
        }

        public void Start()
        {
            Peer.Start();
            HandleNewPlayer(null, new LobbyPlayerGreeting { PlayerName = Environment.MachineName });
        }

        public void StartNetworkGame(MatchSettings gameSettings)
        {
            // create teams
            CreateTeams(gameSettings);
            // Create map etc.
            gameSettings.GameMode.InitializeSession(Ballz.The(), gameSettings);
            //// Broadcast gameSettings incl. map to clients
            Broadcast(new NetworkMessage(NetworkMessage.MessageType.NumberOfPlayers, this.NumberOfClients() + 1)); // +1 for ourselfs
            Broadcast(new NetworkMessage(NetworkMessage.MessageType.StartGame, gameSettings));
            // Start our game session
            Ballz.The().Logic.StartGame(gameSettings);

            Ballz.The().Match.World.StaticGeometry.TerrainModified += OnTerrainModified;
            Ballz.The().Match.World.EntityRemoved += OnEntityRemoved;
        }

        private void OnEntityRemoved(object sender, Entity e)
        {
            Broadcast(new NetworkMessage(NetworkMessage.MessageType.EntityRemoved, e));
        }

        private void OnTerrainModified(object sender, Terrain.TerrainModification modification)
        {
            Broadcast(modification);
        }

        private void CreateTeams(MatchSettings gameSettings)
        {
            Debug.Assert(gameSettings.Teams.Count == 0);
            var counter = 0;

            {
                counter++;
                var team = new Team { ControlledByAI = false, Name = "Team1", NumberOfBallz = 1, Country = "Germoney" };
                gameSettings.Teams.Add(team);
            }
            
            //foreach(var client in connections)
            //{
            //    counter++;
            //    var team = new Team { ControlledByAI = false, Name = $"Team{counter}", NumberOfBallz = 1, Country = "Murica" };

            //    client.ClientPlayerId = team.Id;
            //    client.Send(new NetworkMessage(NetworkMessage.MessageType.YourPlayerId, client.ClientPlayerId));

            //    gameSettings.Teams.Add(team);
            //}
        }

	    public int NumberOfClients()
	    {
	        return Peer.ConnectionsCount;
	    }
        
        DateTime LastUpdate = DateTime.Now;

        public event EventHandler<NetConnection> NewConnection;

        public string[] GetConnectionNames() => Peer.Connections.Select(c => c.RemoteUniqueIdentifier.ToString()).ToArray();

        public void Update(GameTime time)
        {
            NetIncomingMessage im;
            while((im = Peer.ReadMessage()) != null)
            {
                switch (im.MessageType)
                {
                    case NetIncomingMessageType.StatusChanged:
                        NetConnectionStatus status = (NetConnectionStatus)im.ReadByte();

                        string reason = im.ReadString();

                        if(status == NetConnectionStatus.Connected)
                        {
                            Sync.AddConnection(im.SenderConnection);
                            NewConnection?.Invoke(this, im.SenderConnection);
                        }

                        break;
                    case NetIncomingMessageType.Data:
                        Sync.ReadMessage(im);
                        break;
                }

                Peer.Recycle(im);
            }
            
        }

        internal void Stop()
        {
            Peer.Shutdown("Graceful Shutdown");
        }

        private void OnData(object sender, object data)
        {
            if (!(sender is NetConnection))
                throw new ArgumentException("Sender is not a NetConnection");
            
            var connection = sender as NetConnection;
            
            if (data is LobbyPlayerGreeting)
            {
                HandleNewPlayer(connection, data as LobbyPlayerGreeting);
            }

            //// Keyboad/Mouse Input from clients
            if (data is InputMessage)
            {
                HandleRemoteInput(connection, data as InputMessage);
            }

        }

        private void HandleNewPlayer(NetConnection sender, LobbyPlayerGreeting player)
        {
            long remoteId = sender == null ? -1 : sender.RemoteUniqueIdentifier;
            ConnectedPlayers[remoteId] = new PlayerConnection
            {
                Connection = sender,
                PlayerName = player.PlayerName
            };
            
            var lobbyPlayerList = new LobbyPlayerList { PlayerNames = ConnectedPlayers.Values.Select(p=>p.PlayerName).ToArray() };
            PlayerListChanged?.Invoke(this, lobbyPlayerList);
            Broadcast(lobbyPlayerList);
        }

        private void HandleRemoteInput(NetConnection sender, InputMessage msg)
        {
            var player = ConnectedPlayers[sender.RemoteUniqueIdentifier].MatchPlayer;
            if (player != null)
                Ballz.The().Input.InjectInputMessage(msg, player);
        }

        public void Broadcast(object data, bool reliable = true)
        {
            Sync.SendObject(data, reliable);
        }

        public void HandleMessage(object sender, Message message)
        {
            //TODO: handle Messages
        }
    }
}
