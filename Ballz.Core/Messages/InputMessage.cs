namespace Ballz.Messages
{
    public class InputMessage : Message
    {
        public new enum MessageType
        {
            ControlsUp,
            ControlsDown,
            ControlsLeft,
            ControlsRight,
            ControlsAction,
            ControlsBack,
            RawInput
        }

        public InputMessage(MessageType type) : base(Message.MessageType.InputMessage)
        {
            Kind = type;
        }

        public new MessageType Kind { get; private set; }
    }
}