using Ballz.Messages;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace Ballz.Input
{
    /// <summary>
    ///     Input translator takes care of all physical inputs with regards to specified keymappings etc.
    ///     It translates these inputs to corresponding Game Messages.
    /// </summary>
    public class InputTranslator : GameComponent
    {
        private bool down;

        public InputTranslator(Ballz game) : base(game)
        {
            down = false;
            Thegame = game;
        }

        public Ballz Thegame { get; set; }

        public event EventHandler<InputMessage> Input;

        private void OnInput(InputMessage.MessageType inputMessage)
        {
            Input?.Invoke(this, new InputMessage(inputMessage)); //todo: use object pooling and specify message better
        }

        public override void Update(GameTime gameTime)
        {
            // For Mobile devices, this logic will close the Game when the Back button is pressed
            // Exit() is obsolete on iOS
#if !__IOS__
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape) && !down)
            {
                down = true;
                OnInput(InputMessage.MessageType.ControlsBack);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Enter) && !down)
            {
                down = true;
                OnInput(InputMessage.MessageType.ControlsAction);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Up) && !down)
            {
                down = true;
                OnInput(InputMessage.MessageType.ControlsUp);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Down) && !down)
            {
                down = true;
                OnInput(InputMessage.MessageType.ControlsDown);
            }
            if (Keyboard.GetState().GetPressedKeys().Length == 0)
            {
                down = false;
            }
#endif
            // TODO: Add your update logic here	
            base.Update(gameTime);
        }
    }
}