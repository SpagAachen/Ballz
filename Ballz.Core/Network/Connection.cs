using Ballz.GameSession.World;
using Ballz.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

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

        byte[] readBuffer;
        int readDataLength = -1;
        int readDataLengthMissing = -1;
        bool unfinishedObject = false;

        public List<Type> MessageTypes = new List<Type>();

        public int GetMessageTypeId(Type type)
        {
            var res = MessageTypes.IndexOf(type);
            Debug.Assert(res >= 0, "Unknown type!");
            return res;
        }

        public Type GetTypeByMessageTypeId(int id)
        {
            Debug.Assert(id >= 0, "Invalid id!");
            return MessageTypes[id];
        }

        Task<object> receiveTask;

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

            MessageTypes.Add(typeof(List<Entity>));
            MessageTypes.Add(typeof(InputMessage));
            MessageTypes.Add(typeof(NetworkMessage));

            BeginReceive();
        }

        protected void BeginReceive()
        {
            if (receiveTask != null)
                throw new InvalidOperationException("Already receiving");

            receiveTask = new Task<object>(() =>
            {
                byte[] msgLengthBuf = new byte[4];
                connectionStream.Read(msgLengthBuf, 0, 4);
                int msgLength = BitConverter.ToInt32(msgLengthBuf, 0);
                Console.WriteLine("Received " + msgLength + "bytes");

                byte[] msgTypeBuf = new byte[4];
                connectionStream.Read(msgTypeBuf, 0, 4);
                int msgType = BitConverter.ToInt32(msgTypeBuf, 0);

                if (msgType < 0) return null;

                byte[] data = new byte[msgLength];

                connectionStream.Read(data, 0, msgLength);

                var json = UTF8Encoding.UTF8.GetString(data);
                var type = GetTypeByMessageTypeId(msgType);
                return JsonConvert.DeserializeObject(json, type);
            });
            receiveTask.Start();
        }

        /// <summary>
        /// Serializes on object and send it to another game instance.
        /// </summary>
        /// <param name="obj">The object to send</param>
        public void Send(object obj)
        {
            //try
            //{
                var netStr = tcpClient.GetStream();
                // serialize object and get size of it
                var json = JsonConvert.SerializeObject(obj);
                var data = UTF8Encoding.UTF8.GetBytes(json);
                // write size of object into tcp stream
                var userDataLen = BitConverter.GetBytes(data.Length);
                netStr.Write(userDataLen, 0, 4);

                var typeId = GetMessageTypeId(obj.GetType());

                netStr.Write(BitConverter.GetBytes(typeId), 0, 4);

                netStr.Write(data, 0, data.Length);
                Console.WriteLine("Serialized " + data.Length + "bytes");
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine("Network: Warning: Failed to send some data: " + e.ToString());
            //}
        }

        /// <summary>
        /// Returns true iff data is ready to read.
        /// </summary>
        /// <returns></returns>
        public bool DataAvailable()
        {
            return receiveTask.IsCompleted;
        }

        public object ReceiveData()
        {
            object result = null;
            if (receiveTask == null)

                throw new InvalidOperationException("No receiving is in progress");
            try
            {
                receiveTask.Wait();
                result = receiveTask.Result;
            }
            catch (Exception e)
            {
                System.Console.Out.WriteLine("Network: Warning: Failed to receive data. Error: " + e.ToString());
            }
            finally
            {
                receiveTask = null;
            }

            BeginReceive();
            return result;
        }
    }
}
