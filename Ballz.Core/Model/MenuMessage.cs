using System;

namespace Ballz
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

