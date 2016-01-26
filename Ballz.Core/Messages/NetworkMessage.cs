namespace Ballz.Messages
{
    public class NetworkMessage : Message
    {
        public new enum MessageType
        {
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

        public new MessageType Kind { get; private set; }
    }
}