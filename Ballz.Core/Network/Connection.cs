namespace Ballz.Network
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Sockets;

    /// <summary>
    /// Provides an abstraction of the protocol layer. Use it to establish a network connection with another game instance.
    /// </summary>
    class Connection
    {
        public readonly int Id;
        private readonly TcpClient tcpClient;
        private StreamReader reader = null;
        private StreamWriter writer = null;

        /// <summary>
        /// Initializes a new instance of the Connection class and connects to the specified port on the specified host.
        /// </summary>
        /// <param name="host">The DNS name of the remote host to which you intend to connect. </param>
        /// <param name="port">The port number of the remote host to which you intend to connect. </param>
        /// <param name="id">Id for this connection</param>
        public Connection(string host, int port, int id)
        {
            tcpClient = new TcpClient(host, port);
            Id = id;
        }

        /// <summary>
        /// Initializes a new instance of the Connection class and uses the already established connection.
        /// </summary>
        /// <param name="connection">An already established connection.</param>
        /// <param name="id">Id for this connection</param>
        public Connection(TcpClient connection, int id)
        {
            tcpClient = connection;
            Id = id;
        }

        /// <summary>
        /// Send data
        /// </summary>
        /// <param name="data"></param>
        public void Send(string data)
        {
            var s = tcpClient.GetStream();
            using (writer = new StreamWriter(tcpClient.GetStream()))
            {
                if (s.CanWrite)
                {
                    writer.WriteLine(data);
                    writer.Flush();
                }
                else
                {
                    s.Close();
                    tcpClient.Close();
                    throw new InvalidOperationException("Unable to write data to stream");
                }
            }
        }

        /// <summary>
        /// Receive data if any is available
        /// </summary>
        /// <returns></returns>
        public List<string> Receive()
        {
            var s = tcpClient.GetStream();
            var res = new List<string>();
            // new lines available?
            if (!s.DataAvailable)
                return res;
            using (reader = new StreamReader(tcpClient.GetStream()))
            {
                while (reader.Peek() >= 0)
                    res.Add(reader.ReadLine());
            }
            return res;
        }

        /// <summary>
        /// Returns true iff data is ready to read.
        /// </summary>
        /// <returns></returns>
        public bool DataAvailable()
        {
            return tcpClient.GetStream().DataAvailable;
        }
    }
}
