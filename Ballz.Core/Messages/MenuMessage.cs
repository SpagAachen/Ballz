using Ballz.Menu;

namespace Ballz.Messages
{
    public class MenuMessage : Message
    {
        public MenuMessage(Item value) : base(MessageType.MenuMessage)
        {
            Value = value;
        }

        public Item Value { get; private set; }
    }
}