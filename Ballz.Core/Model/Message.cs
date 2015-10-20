using System;

namespace Ballz
{
	/// <summary>
	/// The Message class is used for message passing via events by LogicControl, InputTranslator etc.
	/// </summary>
	public class Message
	{
		public delegate void MessageEventHandler(object sender, Message _eventArgs);

		public enum MessageType
		{
			LogicMessage,
			PhysicsMessage
		};
		public Message (MessageType _type)
		{
			type = _type;
		}


		public MessageType type {
			get;
			private set;
		}
	}
}

