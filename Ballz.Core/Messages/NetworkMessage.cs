
namespace Ballz.Messages
{
    using System;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Utils;
    
    public class NetworkMessage : Message
    {
        public new enum MessageType
        {
            Invalid,
            ConnectedToServer,
            Disconnected,
        }

        public NetworkMessage()
            : base(Message.MessageType.NetworkMessage)
        {
            Kind = MessageType.Invalid;
        }

        public NetworkMessage(MessageType type) : base(Message.MessageType.NetworkMessage)
        {
            Kind = type;
        }
        
        public new MessageType Kind { get; private set; }
        
        public override string ToString()
        {
            return $"{{NetworkMessage Kind:{Kind}}}";
        }
    }
}