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

        public bool? Pressed{ get; private set;}

        public InputMessage(MessageType type, bool? pressed, char? value) : base(Message.MessageType.InputMessage)
        {
            Kind = type;
            Key = value;
            Pressed = pressed;
        }

        public new MessageType Kind { get; private set; }
    }
}