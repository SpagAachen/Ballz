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

    class Server
    {
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
            CreateTeams();
            Broadcast(new NetworkMessage(NetworkMessage.MessageType.NumberOfPlayers, this.NumberOfClients()));
            Broadcast(new NetworkMessage(NetworkMessage.MessageType.StartGame));
        }

        private void CreateTeams()
        {
            var currGameSettings = Ballz.The().Match.GameSettings;
            Debug.Assert(currGameSettings.Teams.Count == 0);
            var counter = 0;
            foreach (var c in connections)
            {
                var player = new Player
                {
                    Name = "Player" + counter
                };
                var team = new Team { ControlledByAI = false, Name = "Team1", NumberOfBallz = 1, player = player };
                currGameSettings.Teams.Add(team);
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

        public void Update(GameTime time)
        {
            // new clients
            if (listener.Pending())
            {
                if (network.GameState == Network.GameStateT.InLobby)
                {
                    // add new player in lobby
                    var client = listener.AcceptTcpClient();
                    connections.Add(new Connection(client, nextId++));
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
                if (c.DataAvailable())
                {
                    var data = c.ReceiveData();
                    this.OnData(data, c.Id);
                }
            }

			// TEST
			{
				//Broadcast(Ballz.The().World.Entities);
			}
            //TODO: Implement
        }

        private void OnData(object data, int sender)
        {
			//Console.WriteLine("Received data from " + sender + ": " + data.ToString()); // Debug
			// Input Message
			if (data.GetType() == typeof(InputMessage))
			{
				
			}
        }

        public void Broadcast(object data)
        {
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
