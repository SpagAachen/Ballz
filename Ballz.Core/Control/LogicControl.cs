using System;

namespace Ballz
{
	/// <summary>
	/// Logic control Processes Messages and other system reactions with regard to the current gamestate.
	/// It uses Message events to inform relevant classes.
	/// </summary>
	public class LogicControl
	{
		public LogicControl ()
		{
		}

		public event Message.MessageEventHandler Message;

		protected virtual void raiseMessageEvent()
		{
			if (Message != null)
				Message (this, new Ballz.Message (Ballz.Message.MessageType.LogicMessage));//example message
		}

		public void handleInputMessage(object _sender, Message _message)
		{
			throw new NotImplementedException ();
		}
	}
}

