using System;

namespace Ballz.Messages
{
   public class InputMessage : Message
   {

      new public MessageType Kind {
         get;
         private set;
      }

      public enum MessageType
      {
         ControlsUp,
         ControlsDown,
         ControlsLeft,
         ControlsRight,
         ControlsAction,
         ControlsBack,
         RawInput
      };

      public InputMessage (MessageType _type) : base(Message.MessageType.InputMessage)
      {
         Kind = _type;
      }
   }
}

