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

    class Server
    {
        static Server()
        {

        }

        private static int nextId = 1;
        
        NetServer Net;
        ObjectSynchronizer Sync = null;

        public double TicksPerSecond = 30;

        public Server()
        {
            var config = new NetPeerConfiguration("SpagAachen.Ballz");
            config.Port = 16116; // TODO: make port configurable
            Net = new NetServer(config);
            Sync = new ObjectSynchronizer(Net);
        }

        public void StartNetworkGame(GameSettings gameSettings)
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

        private void CreateTeams(GameSettings gameSettings)
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
	        return Net.ConnectionsCount;
	    }

        public void Listen()
        {
            Net.Start();
        }
        

        DateTime LastUpdate = DateTime.Now;

        public void Update(GameTime time)
        {
            NetIncomingMessage msg;
            while((msg = Net.ReadMessage()) != null)
            {
                switch(msg.MessageType)
                {
                    case NetIncomingMessageType.Data:
                        Sync.ReadMessage(msg);
                        break;
                }

                Net.Recycle(msg);
            }

            //// new clients
            //if (listener.Pending())
            //{
            //    if (network.GameState == Network.GameStateT.InLobby)
            //    {
            //        // add new player in lobby
            //        var client = listener.AcceptTcpClient();
            //        var connection = new Connection(client, nextId++);
            //        connection.ObjectReceived += OnData;
            //        connections.Add(connection);
            //        network.RaiseMessageEvent(NetworkMessage.MessageType.NewClient);
            //        // update lobby list
            //        this.UpdateLobbyList();
            //    }
            //    else
            //    {
            //        //TODO: Re-add Players that lost connection
            //    }
            //}
            //// receive data
            //foreach (var c in connections)
            //{
            //    c.ReadUpdates();
            //}

            //if (network.GameState == Network.GameStateT.InGame && Ballz.The().Match.State == SessionState.Running)
            //{
            //    var now = DateTime.Now;
            //    if ((now - LastUpdate).TotalSeconds > 1.0 / TicksPerSecond)
            //    {
            //        foreach (var e in Ballz.The().Match.World.Entities)
            //        {
            //            Broadcast(e);
            //        }
            //        LastUpdate = DateTime.Now;
            //    }

            //}
        }
            
        private void OnData(object sender, object data)
        {
			// Input Message
			//if (Ballz.The().Match != null && data.GetType() == typeof(InputMessage))
			//{
   //             var playerId = (sender as Connection).ClientPlayerId;
   //             var player = Ballz.The().Match.PlayerById(playerId);
   //             if(player != null)
   //                 Ballz.The().Input.InjectInputMessage((InputMessage)data, player);
   //         }

        }

        public void Broadcast(object data)
        {
            Sync.SendObject(data);
        }

        public void HandleMessage(object sender, Message message)
        {
            //TODO: handle Messages
        }
    }
}
