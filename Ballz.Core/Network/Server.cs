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
    class Server
    {
        static Server()
        {
            ObjectSync.Sync.RegisterClass<Entity>(() => new Entity());
            ObjectSync.Sync.RegisterClass<Ball>(() => new Ball());
            ObjectSync.Sync.RegisterClass<Shot>(() => new Shot());
            ObjectSync.Sync.RegisterClass<Message>(() => new Message());
            ObjectSync.Sync.RegisterClass<NetworkMessage>(() => new NetworkMessage());
            ObjectSync.Sync.RegisterClass<InputMessage>(() => new InputMessage());
        }

        private static int nextId = 1;
        TcpListener listener;
        private readonly Network network;
        private readonly List<Connection> connections = new List<Connection>();


        public Server(Network net)
        {
            network = net;
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
        }

        private void CreateTeams(GameSettings gameSettings)
        {
            Debug.Assert(gameSettings.Teams.Count == 0);
            var counter = 0;
            for (var index = 0; index < NumberOfClients() + 1 /* +1 for ourself */; index++)
            {
                var player = new Player { Name = "Player" + counter };
                var team = new Team { ControlledByAI = false, Name = "Team1", NumberOfBallz = 1, player = player };
                gameSettings.Teams.Add(team);
                ++counter;
            }
        }

	    public int NumberOfClients()
	    {
	        return connections.Count;
	    }

        public void Listen(int port)
        {
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            // start lobby first
            network.GameState = Network.GameStateT.InLobby;
            this.UpdateLobbyList();
        }

        public void UpdateLobbyList()
        {
            Ballz.The().NetworkLobbyConnectedClients.Name = "Myself";
            foreach(var c in connections)
            {
                Ballz.The().NetworkLobbyConnectedClients.Name += ", " + c.Id;
            }
        }

        DateTime LastUpdate = DateTime.Now;

        public void Update(GameTime time)
        {
            // new clients
            if (listener.Pending())
            {
                if (network.GameState == Network.GameStateT.InLobby)
                {
                    // add new player in lobby
                    var client = listener.AcceptTcpClient();
                    var connection = new Connection(client, nextId++);
                    connection.ObjectReceived += OnData;
                    connections.Add(connection);
                    network.RaiseMessageEvent(NetworkMessage.MessageType.NewClient);
                    // update lobby list
                    this.UpdateLobbyList();
                }
                else
                {
                    //TODO: Re-add Players that lost connection
                }
            }
            // receive data
            foreach (var c in connections)
            {
                c.ReadUpdates();
            }
            
            if(network.GameState == Network.GameStateT.InGame && Ballz.The().Match.State == SessionState.Running)
			{
                var now = DateTime.Now;
                if ((now - LastUpdate).TotalSeconds > 0.1)
                {
                    foreach (var e in Ballz.The().Match.World.Entities)
                    {
                        Broadcast(e);
                    }
                    LastUpdate = DateTime.Now;
                }
			}
            //TODO: Implement
        }

        private void OnData(object sender, object data)
        {

			// Input Message
			if (Ballz.The().Match != null && data.GetType() == typeof(InputMessage))
			{
                var playerId = (sender as Connection).RemotePlayerId;
                var player = Ballz.The().Match.Players.First(); //PlayerByNumber(playerId);
                Ballz.The().Input.InjectInputMessage((InputMessage)data, player);
            }
        }

        public void Broadcast(object data)
        {
            //Console.WriteLine($"Broadcasting {data}");
            foreach (var c in connections)
            {
                c.Send(data);
            }
        }

        public void HandleMessage(object sender, Message message)
        {
            //TODO: handle Messages
        }
    }
}
