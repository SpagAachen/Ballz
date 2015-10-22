namespace Ballz
{
   using System;
   using Microsoft.Xna.Framework;
   using Microsoft.Xna.Framework.Input;


   /// <summary>
   /// Input translator takes care of all physical inputs with regards to specified keymappings etc.
   /// It translates these inputs to corresponding Game Messages.
   /// </summary>
   public class InputTranslator : GameComponent
   {
      public BallzGame Thegame{ get; }

      public InputTranslator (BallzGame _game) : base (_game)
      {
         Thegame = _game;
      }

      public override void Update (GameTime gameTime)
      {
         // For Mobile devices, this logic will close the Game when the Back button is pressed
         // Exit() is obsolete on iOS
         #if !__IOS__
         if (GamePad.GetState (PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
             Keyboard.GetState ().IsKeyDown (Keys.Escape)) {
            Thegame.onInput (Message.MessageType.ShutDownMessage);
         }
         #endif
         // TODO: Add your update logic here	
         base.Update (gameTime);
      }
	
   }
}

