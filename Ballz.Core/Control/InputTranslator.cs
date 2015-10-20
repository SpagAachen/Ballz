using System;
using Microsoft.Xna.Framework;

namespace Ballz
{
	/// <summary>
	/// Input translator takes care of all physical inputs with regards to specified keymappings etc.
	/// It translates these inputs to corresponding Game Messages.
	/// </summary>
	public class InputTranslator
	{
		public InputTranslator ()
		{
			throw new NotImplementedException ();
		}

		public event Message.MessageEventHandler Translate;

		protected virtual void raiseTranslateEvent()
		{
			if (Translate != null)
				Translate (this, new Message (Message.MessageType.LogicMessage));//example message
		}

		public void update(GameTime _time)
		{
			throw new NotImplementedException ();
		}

	}
}

