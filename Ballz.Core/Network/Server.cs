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
    using SessionFactory;
    using Lobby;
    using Newtonsoft.Json;
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

        public NetServer Peer { get; private set; }

        ObjectSynchronizer Sync = null;

        Dictionary<long, PlayerConnection> PlayersByConnection = new Dictionary<long, PlayerConnection>();
        Dictionary<int, PlayerConnection> PlayersById = new Dictionary<int, PlayerConnection>();

        public event EventHandler<LobbyPlayerList> PlayerListChanged;

        public double TicksPerSecond = 30;

        public Stopwatch WorldSyncTimer = new Stopwatch();

        public bool GameRunning = false;

        public FullGameInfo GameInfo { get; private set; }

        public Server(FullGameInfo info)
        {
            GameInfo = info;
            var config = new NetPeerConfiguration(Network.ApplicationIdentifier);
            config.Port = Network.DefaultPort; // TODO: make port configurable
            config.ConnectionTimeout = Network.ConnectionTimeoutSeconds;
            config.AcceptIncomingConnections = true;
            config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
            Peer = new NetServer(config);
            Sync = new ObjectSynchronizer(Peer);
            Sync.NewObjectReceived += OnData;
        }

        public void Start()
        {
            Peer.Start();
            HandleNewPlayer(null, new LobbyPlayerGreeting { PlayerName = Ballz.The().Settings.PlayerName });
        }

        public void StartNetworkGame(MatchSettings gameSettings)
        {
            gameSettings.Teams = new List<Team>();
            var idCounter = 0;
            foreach (var playerCon in PlayersByConnection.Values)
            {
                var team = new Team
                {
                    Id = idCounter++,
                    Name = playerCon.PlayerName,
                    ControlledByAI = false,
                    NumberOfBallz = 1,
                };

                gameSettings.Teams.Add(team);

                playerCon.MatchPlayerId = team.Id;
                PlayersById[team.Id] = playerCon;
            }

            gameSettings.Teams.Add(new Team
            {
                Id = idCounter++,
                Name = "None",
                ControlledByAI = false,
                NumberOfBallz = 1,
            });

            // Create map etc.
            gameSettings.GameMode.InitializeSession(Ballz.The(), gameSettings);
            //// Broadcast gameSettings incl. map to clients
            var serializedSettings = gameSettings.Serialize();
            foreach(var playerCon in PlayersByConnection.Values)
            {
                var startInfo = new GameStartInfo
                {
                    YourPlayerId = playerCon.MatchPlayerId,
                    Settings = serializedSettings
                };

                SendToPlayer(playerCon.MatchPlayerId, startInfo);
            }

            // Start our game session
            Ballz.The().Logic.StartGame(gameSettings);

            Ballz.The().Match.World.StaticGeometry.TerrainModified += OnTerrainModified;
            Ballz.The().Match.World.EntityRemoved += OnEntityRemoved;

            SendWorldState();
            WorldSyncTimer.Start();
            GameRunning = true;
        }

        private void OnEntityRemoved(object sender, Entity e)
        {
            //TODO: Broadcast entity removal
        }

        private void OnTerrainModified(object sender, Terrain.TerrainModification modification)
        {
            Broadcast(modification);
        }
        
	    public int NumberOfClients()
	    {
	        return Peer.ConnectionsCount;
	    }
        
        DateTime LastUpdate = DateTime.Now;

        public event EventHandler<NetConnection> NewConnection;
        
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

                        if(!GameRunning && status == NetConnectionStatus.Connected)
                        {
                            Sync.AddConnection(im.SenderConnection);
                            NewConnection?.Invoke(this, im.SenderConnection);
                        }

                        break;
                    case NetIncomingMessageType.DiscoveryRequest:
                        // Respond to discovery requests with a json-serialized game info
                        // But only send the non-secret parts
                        var publicInfo = GameInfo.PublicInfo();
                        var serializedInfo = JsonConvert.SerializeObject(publicInfo);
                        var response = Peer.CreateMessage();
                        response.Write(serializedInfo);
                        Peer.SendDiscoveryResponse(response, im.SenderEndPoint);
                        break;
                    case NetIncomingMessageType.Data:
                        Sync.ReadMessage(im);
                        break;
                }

                Peer.Recycle(im);
            }

            if(GameRunning && WorldSyncTimer.ElapsedMilliseconds > Network.WorldSyncIntervalMs)
            {
                // Send full world state on every frame.
                //Todo: Only send updates for world synchronization
                SendWorldState();
                WorldSyncTimer.Restart();
            }
        }

        private void SendWorldState()
        {
            var entities = Ballz.The().Match.World.Entities;
            foreach(var e in entities)
            {
                Broadcast(e);
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
            PlayersByConnection[remoteId] = new PlayerConnection
            {
                Connection = sender,
                PlayerName = player.PlayerName
            };
            
            var lobbyPlayerList = new LobbyPlayerList { PlayerNames = PlayersByConnection.Values.Select(p=>p.PlayerName).ToArray() };
            PlayerListChanged?.Invoke(this, lobbyPlayerList);
            Broadcast(lobbyPlayerList);
        }

        private void HandleRemoteInput(NetConnection sender, InputMessage msg)
        {
            var player = PlayersByConnection[sender.RemoteUniqueIdentifier].MatchPlayer;
            if (player != null)
                Ballz.The().Input.InjectInputMessage(msg, player);
        }

        public void Broadcast(object data, bool reliable = true)
        {
            Sync.SendObject(data, reliable);
        }

        public void SendToPlayer(int playerId, object data, bool reliable = true)
        {
            Sync.SendObject(data, reliable);
        }

        public void HandleMessage(object sender, Message message)
        {
            //TODO: handle Messages
        }
    }
}
