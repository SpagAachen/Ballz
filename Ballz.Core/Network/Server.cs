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
            this.network = net;
        }

        public void Listen(int port)
        {
            this.listener = new TcpListener(IPAddress.Any, port);
            this.listener.Start();
        }

        public void Update(GameTime time)
        {
            // new clients
            if (this.listener.Pending())
            {
                var client = this.listener.AcceptTcpClient();
                this.connections.Add(new Connection(client, nextId++));
                this.network.RaiseMessageEvent(NetworkMessage.MessageType.NewClient);
            }
            // receive data
            foreach (var c in this.connections)
            {
                if (c.DataAvailable())
                {
                    var data = c.Receive();
                    foreach (var d in data)
                        this.onData(d, c.Id);
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
