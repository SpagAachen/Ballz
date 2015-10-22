using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Ballz
{
	/// <summary>
	/// Input translator takes care of all physical inputs with regards to specified keymappings etc.
	/// It translates these inputs to corresponding Game Messages.
	/// </summary>
	public class InputTranslator : GameComponent
	{
		public InputTranslator (Game _game) : base(_game)
		{
		}

		public event Message.MessageEventHandler Translate;

		protected virtual void raiseTranslateEvent(Message.MessageType _type)
		{
			if (Translate != null)
				Translate (this, new Message (_type));//emit the message of given type
		}

		public void update(GameTime _time)
		{
			// For Mobile devices, this logic will close the Game when the Back button is pressed
			// Exit() is obsolete on iOS
			#if !__IOS__
			if (GamePad.GetState (PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
				Keyboard.GetState ().IsKeyDown (Keys.Escape)) {
				raiseTranslateEvent(Message.MessageType.ShutDownMessage);
			}
			#endif
			// TODO: Add your update logic here	
		}

	}
}

