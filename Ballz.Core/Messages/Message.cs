namespace Ballz.Messages
{
   using System;


   /// <summary>
   /// The Message class is used for message passing via events by LogicControl, InputTranslator etc.
   /// </summary>
   public class Message
   {

      public enum MessageType
      {
         LogicMessage,
         PhysicsMessage,
         MenuMessage,
         InputMessage
      };

      public Message (MessageType _type)
      {
         Kind = _type;
      }


      public MessageType Kind {
         get;
         private set;
      }
   }
}

