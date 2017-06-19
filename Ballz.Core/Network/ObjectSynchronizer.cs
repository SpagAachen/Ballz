using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ballz.Network
{
    public class SynchronizingInfo
    {
        public static Dictionary<Type, SynchronizingInfo> InfoByType = new Dictionary<Type, SynchronizingInfo>();
        public static Dictionary<Int16, SynchronizingInfo> InfoByTypeId = new Dictionary<short, SynchronizingInfo>();

        public static void Register(SynchronizingInfo info)
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
    }

    public class ObjectSynchronizer
    {
        NetPeer Peer;
        List<NetConnection> Connections;

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

        public void SendObject(object obj, bool reliableTransfer = false)
        {
            var msg = Peer.CreateMessage();
            WriteHeader(msg, MessageType.Object);

            var type = obj.GetType();

            var sync = SynchronizingInfo.InfoByType[type];
            
            // Write the 16-bit type id
            var typeId = sync.TypeId;
            msg.Write(typeId);

            // Write the 32-bit object id
            var objId = sync.ObjectToId(obj);
            msg.Write(objId);

            // Write the actual object contents
            msg.WriteAllFields(obj);

            Peer.SendMessage(msg, Connections, reliableTransfer ? NetDeliveryMethod.ReliableOrdered : NetDeliveryMethod.UnreliableSequenced, reliableTransfer ? 0 : 1);
        }

        protected void ReadObject(NetIncomingMessage msg)
        {
            var typeId = msg.ReadInt16();
            var objId = msg.ReadInt64();

            var sync = SynchronizingInfo.InfoByTypeId[typeId];

            if (sync.IsIdentifiable)
            {
                var obj = sync.IdToObject(objId);
                var isNew = obj == null;

                if (isNew)
                {
                    obj = sync.ObjectConstructor();
                }

                msg.ReadAllFields(obj);

                if (isNew)
                {
                    NewObjectReceived?.Invoke(this, obj);
                }
                else
                {
                    ObjectUpdateReceived?.Invoke(this, obj);
                }
            }
            else
            {
                var obj = sync.ObjectConstructor();
                msg.ReadAllFields(obj);
                NewObjectReceived?.Invoke(this, obj);
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
