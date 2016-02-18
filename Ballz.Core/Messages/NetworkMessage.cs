
namespace Ballz.Messages
{
    using System;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Utils;

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
            StartGame,
            NumberOfPlayers,
            GameSettings
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

        public NetworkMessage(MessageType type, object data) : base(Message.MessageType.NetworkMessage)
        {
            Kind = type;
            Data = data;
        }

        [JsonProperty("NetworkMessageKind")]
        public new MessageType Kind { get; private set; }

        [JsonProperty("NetworkMessageData")]
        [JsonConverter(typeof(TypeInfoConverter))]
        public object Data { get; set; }
    }
}