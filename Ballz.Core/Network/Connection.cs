namespace Ballz.Network
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Sockets;
	using System.Runtime.Serialization;
	using System.Runtime.Serialization.Formatters.Binary;

    /// <summary>
    /// Provides an abstraction of the protocol layer. Use it to establish a network connection with another game instance.
    /// </summary>
    class Connection
    {
        public readonly int Id;
        private readonly TcpClient tcpClient;
		BinaryFormatter formatter = new BinaryFormatter();

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
		/// Serializes on object and send it to another game instance.
		/// </summary>
		/// <param name="obj">The object to send</param>
		public void Send(object obj)
		{
			formatter.Serialize(tcpClient.GetStream(), obj);
		}

		/// <summary>
		/// Receive objects if any is available
		/// </summary>
		/// <returns>A list of objects received</returns>
		public List<object> Receive()
		{
			var s = tcpClient.GetStream();
			var res = new List<object>();
			// new data available?
			if (!s.DataAvailable)
				return res;
			while (s.Position != s.Length) {
				var obj = (object)formatter.Deserialize (s); 
				res.Add (obj);
			}
			return res;
		}


		/*
        /// <summary>
        /// Send data
        /// </summary>
        /// <param name="data"></param>
        private void SendRaw(string data)
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
		*/

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
