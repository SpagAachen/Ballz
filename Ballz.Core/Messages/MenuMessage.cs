using Ballz.Menu;
using System;

namespace Ballz.Messages
{
   public class MenuMessage : Message
   {
      public GameMenu Value
      {
         get;
         private set;
      }

      public MenuMessage (GameMenu value) : base(MessageType.MenuMessage)
      {
         Value = value;
      }
   }
}

