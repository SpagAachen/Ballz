using Ballz.Gui;

namespace Ballz.Messages
{
    public class MenuMessage : Message
    {
        public MenuMessage(MenuPanel value) : base(MessageType.MenuMessage)
        {
            Value = value;
        }

        public MenuPanel Value { get; private set; }
    }
}