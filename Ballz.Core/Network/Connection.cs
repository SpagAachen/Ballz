using Ballz.GameSession.World;
using Ballz.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ObjectSync;

namespace Ballz.Network
{

    /// <summary>
    /// Provides an abstraction of the protocol layer. Use it to establish a network connection with another game instance.
    /// </summary>
    class Connection
    {
        public readonly int Id;
        private readonly TcpClient tcpClient;
        NetworkStream connectionStream;
        StreamSync streamSync;

        public int RemotePlayerId = 1;

        byte[] readBuffer;
        int readDataLength = -1;
        int readDataLengthMissing = -1;
        bool unfinishedObject = false;

        bool IsConnected { get { return tcpClient.Connected; } }

        public event EventHandler<object> ObjectReceived;
        
        /// <summary>
        /// Initializes a new instance of the Connection class and connects to the specified port on the specified host.
        /// </summary>
        /// <param name="host">The DNS name of the remote host to which you intend to connect. </param>
        /// <param name="port">The port number of the remote host to which you intend to connect. </param>
        /// <param name="id">Id for this connection</param>
        public Connection(string host, int port, int id) :
            this(new TcpClient(host, port), id)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Connection class and uses the already established connection.
        /// </summary>
        /// <param name="connection">An already established connection.</param>
        /// <param name="id">Id for this connection</param>
        public Connection(TcpClient connection, int id)
        {
            Id = id;
            tcpClient = connection;
            connectionStream = tcpClient.GetStream();
            streamSync = new StreamSync(connectionStream);
            streamSync.BeginReceive();
        }

        /// <summary>
        /// Serializes on object and send it to another game instance.
        /// </summary>
        /// <param name="obj">The object to send</param>
        public void Send(object obj)
        {
            streamSync.WriteUpdate(obj);
            connectionStream.Flush();
        }

        public void ReadUpdates()
        {
            streamSync.ApplyReceivedUpdates((obj) => {
                /*var entity = obj as Entity;
                if(entity != null)
                {
                    return Ballz.The().Match.World.EntityById(entity.ID);
                }*/

                ObjectReceived?.Invoke(this, obj);

                return null;
            });
        }
    }
}
