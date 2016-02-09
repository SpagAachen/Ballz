using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Ballz.Messages
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class NetworkMessage : Message
    {
        public new enum MessageType
        {
            Invalid,
            Disconnected,
            ConnectingToServer,
            ConnectedToServer,
            ServerStarted,
            NewClient,
            GameStarted
        }

        public NetworkMessage(MessageType type) : base(Message.MessageType.NetworkMessage)
        {
            Kind = type;
        }

        [JsonProperty("NetworkMessageKind")]
        public new MessageType Kind { get; private set; }
    }
}