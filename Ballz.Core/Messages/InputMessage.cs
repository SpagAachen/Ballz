namespace Ballz.Messages
{
   public class InputMessage : Message
   {

      new public MessageType Kind {
         get;
         private set;
      }

      new public enum MessageType
      {
         ControlsUp,
         ControlsDown,
         ControlsLeft,
         ControlsRight,
         ControlsAction,
         ControlsBack,
         RawInput
      };

      public InputMessage (MessageType type) : base(Message.MessageType.InputMessage)
      {
         Kind = type;
      }
   }
}

