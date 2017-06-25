using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace Ballz.Network
{
    public class SynchronizingInfo
    {
        public static Dictionary<Type, SynchronizingInfo> InfoByType = new Dictionary<Type, SynchronizingInfo>();
        public static Dictionary<Int16, SynchronizingInfo> InfoByTypeId = new Dictionary<short, SynchronizingInfo>();

        public static void RegisterClass<T>(
            Action<NetOutgoingMessage, object> customSerializer = null,
            Action<NetIncomingMessage, object> customDeserializer = null)
            where T : class, new()
        {
            SynchronizingInfo.RegisterSyncInfo(new SynchronizingInfo
            {
                Type = typeof(T),
                IsIdentifiable = false,
                ObjectConstructor = () => new T(),
                CustomSerializer = customSerializer,
                CustomDeserializer = customDeserializer
            });
            ObjectSync.Sync.RegisterClass<T>(() => new T());
        }

        public static void RegisterIdentifiable<T>(
            Func<long,object> idToObject,
            Func<object,long> objectToId,
            Action<NetOutgoingMessage, object> customSerializer = null,
            Action<NetIncomingMessage, object> customDeserializer = null)
            where T : class, new()
        {
            SynchronizingInfo.RegisterSyncInfo(new SynchronizingInfo
            {
                Type = typeof(T),
                IsIdentifiable = true,
                ObjectConstructor = () => new T(),
                IdToObject = idToObject,
                ObjectToId = objectToId,
                CustomSerializer = customSerializer,
                CustomDeserializer = customDeserializer
            });
            ObjectSync.Sync.RegisterClass<T>(() => new T());
        }

        private static void RegisterSyncInfo(SynchronizingInfo info)
        {
            InfoByType[info.Type] = info;
            InfoByTypeId[info.TypeId] = info;
        }

        static Int16 TypeIdCounter = 0;

        public Int16 TypeId = TypeIdCounter++;
        public Type Type;
        public Func<object> ObjectConstructor;

        public bool IsIdentifiable = false;
        public Func<Int64, object> IdToObject = null;
        public Func<object, Int64> ObjectToId = null;

        public Action<NetOutgoingMessage, object> CustomSerializer = null;
        public Action<NetIncomingMessage, object> CustomDeserializer = null;
    }

    public class ObjectSynchronizer
    {
        NetPeer Peer;
        List<NetConnection> Connections = new List<NetConnection>();

        public enum MessageType:byte
        {
            None = 0,
            Object = 1,
            TypeId = 2
        }
        
        public void AddConnection(NetConnection connection)
        {
            Connections.Add(connection);
        }

        public event EventHandler<object> NewObjectReceived;
        public event EventHandler<object> ObjectUpdateReceived;

        protected void WriteHeader(NetOutgoingMessage msg, MessageType messageType)
        {
            msg.Write((byte)messageType);
        }

        public ObjectSynchronizer(NetPeer peer)
        {
            Peer = peer;
        }

        public void SendObject(object obj, bool reliableTransfer = false, NetConnection connection = null)
        {
            var connections = Connections;
            if(connection != null)
            {
                connections = new List<NetConnection>() { connection };
            }

            if (Connections.Count == 0)
                return;

            var msg = Peer.CreateMessage();
            WriteHeader(msg, MessageType.Object);

            var type = obj.GetType();

            var sync = SynchronizingInfo.InfoByType[type];
            
            // Write the 16-bit type id
            var typeId = sync.TypeId;
            msg.Write(typeId);

            // Write the 32-bit object id
            var objId = sync.IsIdentifiable ? sync.ObjectToId(obj) : 0;
            msg.Write(objId);

            // Write the actual object contents
            if(sync.CustomSerializer != null)
            {
                sync.CustomSerializer(msg, obj);
            }
            else
            {
                string serialized = JsonConvert.SerializeObject(obj);
                msg.Write(serialized);
            }
            
            Peer.SendMessage(msg, Connections, reliableTransfer ? NetDeliveryMethod.ReliableOrdered : NetDeliveryMethod.UnreliableSequenced, reliableTransfer ? 0 : 1);
        }
        
        protected void ReadObject(NetIncomingMessage msg)
        {
            var typeId = msg.ReadInt16();
            var objId = msg.ReadInt64();

            var sync = SynchronizingInfo.InfoByTypeId[typeId];

            object obj = null;

            if (sync.IsIdentifiable)
            {
                obj = sync.IdToObject(objId);
                
            }

            bool isNew = obj == null;
            
            if(sync.CustomDeserializer != null)
            {
                if (isNew)
                {
                    obj = sync.ObjectConstructor();
                }
                sync.CustomDeserializer(msg, obj);
            }
            else
            {
                var deserialized = JsonConvert.DeserializeObject(msg.ReadString(), sync.Type);

                if (isNew)
                {
                    obj = deserialized;
                }
                else
                {
                    ObjectSync.Sync.SyncState(deserialized, obj);
                }
            }


            if (isNew)
            {
                NewObjectReceived?.Invoke(msg.SenderConnection, obj);
            }
            else
            {
                ObjectUpdateReceived?.Invoke(msg.SenderConnection, obj);
            }
        }
        
        public void ReadMessage(NetIncomingMessage msg)
        {
            var msgType = (MessageType)msg.ReadByte();
            
            switch(msgType)
            {
                case MessageType.Object:
                    ReadObject(msg);
                    break;
                default:
                    throw new InvalidOperationException("Invalid message type received");
            }
        }
    }
}
