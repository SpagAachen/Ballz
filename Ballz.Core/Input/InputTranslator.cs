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
        private bool subscribed = false;
        private InputMode mMode;
        public InputMode Mode
        {
            get
            {
                return mMode;
            }
            set
            {
                if (value == InputMode.RAW && !subscribed)
                {
                    Game.Window.TextInput += RawHandler;
                    subscribed = true;
                }
                else
                {
                    if (Mode != value && subscribed)
                    {
                        Game.Window.TextInput -= RawHandler;
                        subscribed = false;
                    }
                }
                mMode = value;
            }
        }

        public enum InputMode
        {
            RAW,
            PROCESSED
        }

        public event EventHandler<InputMessage> Input;

        public InputTranslator(Ballz game) : base(game)
        {
            down = false;
            Mode = InputMode.PROCESSED;
        }

        void RawHandler(Object sender, TextInputEventArgs eventArgs)
        {
            OnInput(InputMessage.MessageType.RawInput,eventArgs.Character);
        }

        private void OnInput(InputMessage.MessageType inputMessage, char? key = null)
        {
            Input?.Invoke(this, new InputMessage(inputMessage, key)); //todo: use object pooling and specify message better
        }

        public override void Update(GameTime gameTime)
        {
            if (Mode == InputMode.PROCESSED)
            {
                processInput();
            }
            else
            {
                processRawInput();
            }

            //avoid accidentally emitting multiple keystrokes
            if (Keyboard.GetState().GetPressedKeys().Length == 0)
            {
                down = false;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// Processes the raw input and emits corresponding Events.
        /// 
        /// TODO: add GamePad Support for raw inputs.
        /// </summary>
        private void processRawInput()
        {
            //the back key is supposed to switch back to processed InputMode
            //note that the RAW inputs themselves are processed by the RawHandler function.
            if (Keyboard.GetState().IsKeyDown(Keys.Escape) && !down)
            {
                Mode = InputMode.PROCESSED;
                down = true;
                OnInput(InputMessage.MessageType.ControlsBack);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Back) && !down)
            {
                down = true;
                OnInput(InputMessage.MessageType.RawBack);
            }
        }

        private void processInput()
        {
            // For Mobile devices, this logic will close the Game when the Back button is pressed
            // Exit() is obsolete on iOS
            #if !__IOS__
            if(Keyboard.GetState().IsKeyDown(Keys.OemTilde) && ! down)
            {
                down = true;
                OnInput(InputMessage.MessageType.ControlsConsole);
            }
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
            if (Keyboard.GetState().IsKeyDown(Keys.Left) && !down)
            {
                down = true;
                OnInput(InputMessage.MessageType.ControlsLeft);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Right) && !down)
            {
                down = true;
                OnInput(InputMessage.MessageType.ControlsRight);
            }
#endif
        }
    }
}