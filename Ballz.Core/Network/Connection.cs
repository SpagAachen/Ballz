namespace Ballz.Network
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Sockets;
	using System.Runtime.Serialization;
	using System.Runtime.Serialization.Formatters.Binary;
	using System.Diagnostics;

    /// <summary>
    /// Provides an abstraction of the protocol layer. Use it to establish a network connection with another game instance.
    /// </summary>
    class Connection
    {
        public readonly int Id;
        private readonly TcpClient tcpClient;
		BinaryFormatter formatter = new BinaryFormatter();
		byte[] readBuffer;
		int readDataLength = -1;
		int readDataLengthMissing = -1;
		bool unfinishedObject = false;

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
			try
			{
				var netStr = tcpClient.GetStream ();
				// serialize object and get size of it
				var memStr = new MemoryStream ();
				formatter.Serialize(memStr, obj);
				var n = memStr.Length;
				// write size of object into tcp stream
				var userDataLen = BitConverter.GetBytes((Int32)n);
				netStr.Write(userDataLen, 0, 4);
				memStr.WriteTo (netStr);
			}
			catch(Exception e)
			{}
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
			while (s.DataAvailable) {
				if (unfinishedObject) {
					// how much data can we receive? max is tcp buffer size.
					int recLen = readDataLengthMissing;
					if (readDataLengthMissing > tcpClient.ReceiveBufferSize)
						recLen = tcpClient.ReceiveBufferSize;
					// add data to buffer
					s.Read (readBuffer, readDataLength-readDataLengthMissing, recLen);
					readDataLengthMissing -= recLen;
					Debug.Assert (readDataLengthMissing >= 0, "Read more data than available");
					if (readDataLengthMissing == 0) {
						// hole object is finished
						Stream stream = new MemoryStream(readBuffer);
						var obj = (object)formatter.Deserialize (stream); 
						res.Add (obj);
					}

				} else {
					// determine length of object to receive
					byte[] readMsgLen = new byte[4];
					s.Read(readMsgLen, 0, 4);
					readDataLength = BitConverter.ToInt32(readMsgLen, 0);
					// is data split in multiple packets?
					if (readDataLength > tcpClient.ReceiveBufferSize + 4) {
						// data does not fit in one packet. allocate buffer.
						unfinishedObject = true;
						readDataLengthMissing = readDataLength;
						readBuffer = new byte[readDataLength];
						// store received part of object in buffer
						s.Read (readBuffer, 0, tcpClient.ReceiveBufferSize - 4);
					} else {
						// not split. read it directly.
						var obj = (object)formatter.Deserialize (s); 
						res.Add (obj);
					}
				}
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
