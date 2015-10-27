using Ballz.Menu;

namespace Ballz.Messages
{
    public class MenuMessage : Message
    {
        public MenuMessage(GameMenu value) : base(MessageType.MenuMessage)
        {
            Value = value;
        }

        public GameMenu Value { get; private set; }
    }
}