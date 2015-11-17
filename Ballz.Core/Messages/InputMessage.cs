namespace Ballz.Messages
{
    public class InputMessage : Message
    {
        public new enum MessageType
        {
            ControlsConsole,
            ControlsUp,
            ControlsDown,
            ControlsLeft,
            ControlsRight,
            ControlsAction,
            ControlsBack,
            RawBack,
            RawInput
        }

        public char? Key{ get; private set;}

        public InputMessage(MessageType type, char? value) : base(Message.MessageType.InputMessage)
        {
            Kind = type;
            Key = value;
        }

        public new MessageType Kind { get; private set; }
    }
}