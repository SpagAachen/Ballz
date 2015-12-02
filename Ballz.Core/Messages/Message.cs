using System;

namespace Ballz.Messages
{
    /// <summary>
    ///     The Message class is used for message passing via events by LogicControl, InputTranslator etc.
    /// </summary>
    [Serializable]
    public class Message
    {
        public enum MessageType
        {
            LogicMessage,
            PhysicsMessage,
            MenuMessage,
            InputMessage,
            NetworkMessage
        }

        public Message(MessageType type)
        {
            Kind = type;
        }


        public MessageType Kind { get; private set; }
    }
}