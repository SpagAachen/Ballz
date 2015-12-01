namespace Ballz.Network
{
    using System;
    using System.Collections.Generic;
    using System.Net;

    using Messages;
    using Microsoft.Xna.Framework;
    using System.Net.Sockets;

	class Server
    {
        private static int nextId = 1;
        TcpListener listener = null;
        private readonly Network network = null;
        private readonly List<Connection> connections = new List<Connection>();

        public Server(Network net)
        {
            network = net;
        }

        public void Listen(int port)
        {
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
        }

        public void Update(GameTime time)
        {
            // new clients
            if (listener.Pending())
            {
                var client = listener.AcceptTcpClient();
                connections.Add(new Connection(client, nextId++));
                network.RaiseMessageEvent(NetworkMessage.MessageType.NewClient);
            }
            // receive data
            foreach (var c in connections)
            {
                if (c.DataAvailable())
                {
                    var data = c.Receive();
                    foreach (var d in data)
                        onData(d, c.Id);
                }
            }
            //TODO: Implement
        }

        private void onData(string data, int sender)
        {
            Console.WriteLine("Received data from " + sender + ": " + data);
        }

        public void Broadcast(string data)
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
