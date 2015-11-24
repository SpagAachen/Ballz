using Ballz.Messages;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Ballz.Input
{
    /// <summary>
    ///     Input translator takes care of all physical inputs with regards to specified keymappings etc.
    ///     It translates these inputs to corresponding Game Messages.
    /// </summary>
    public class InputTranslator : GameComponent
    {
        private bool subscribed = false;
        private InputMode mMode;

        private KeyboardState previousState;
        private KeyboardState currentState;

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
            Mode = InputMode.PROCESSED;
            previousState = Keyboard.GetState();
            currentState = previousState;
        }

        void RawHandler(Object sender, TextInputEventArgs eventArgs)
        {
            OnInput(InputMessage.MessageType.RawInput, null, eventArgs.Character);
        }

        private void OnInput(InputMessage.MessageType inputMessage, bool? pressed = null, char? key = null)
        {
            Input?.Invoke(this, new InputMessage(inputMessage, pressed, key)); //todo: use object pooling and specify message better
        }

        public override void Update(GameTime gameTime)
        {
            currentState = Keyboard.GetState();
            if (currentState != previousState)
            {
                if (Mode == InputMode.PROCESSED)
                {
                    processInput();
                }
                else
                {
                    processRawInput();
                }
                previousState = currentState;
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
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Mode = InputMode.PROCESSED;
                OnInput(InputMessage.MessageType.ControlsBack,true);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Back))
            {
                OnInput(InputMessage.MessageType.RawBack);
            }
        }

        /// <summary>
        /// find out which keys got changed from array a to array b.
        /// the comparison is one sided thus if in b a key was added the returned list will be empty.
        /// if in a a key was added, the list will contain this key
        /// </summary>
        /// <returns>The keys.</returns>
        /// <param name="a">The alpha component.</param>
        /// <param name="b">The blue component.</param>
        private List<Keys> changedKeys(Keys[] a , Keys[] b)
        {
            List<Keys> result = new List<Keys>();
            foreach (var keyA in a)
            {
                bool keyChanged = true;
                foreach (var keyB in b)
                {
                    if (keyA == keyB)
                    {
                        keyChanged = false;
                    }
                }
                if (keyChanged)
                    result.Add(keyA);
            }
            return result;
        }

        private void processInput()
        {
            //Keys that are pressed now but where not pressed previously are keys that got pressed
            List<Keys> pressedKeys = changedKeys(currentState.GetPressedKeys(), previousState.GetPressedKeys());
            //keys that where pressed previously but are not currently are Keys that got released
            List<Keys> releasedKeys = changedKeys(previousState.GetPressedKeys(), currentState.GetPressedKeys());

            emitKeyMessages(pressedKeys, true);
            emitKeyMessages(releasedKeys, false);
        }

        private void emitKeyMessages(List<Keys> keyList, bool pressed)
        {
            foreach(Keys theKey in keyList)
            {
                switch (theKey)
                {
                    case Keys.OemTilde:
                        OnInput(InputMessage.MessageType.ControlsConsole, pressed);
                        break;
                    case Keys.Escape:
                        OnInput(InputMessage.MessageType.ControlsBack, pressed);
                        break;
                    case Keys.Enter:
                        OnInput(InputMessage.MessageType.ControlsAction, pressed);
                        break;
                    case Keys.Up:
                        OnInput(InputMessage.MessageType.ControlsUp, pressed);
                        break;
                    case Keys.Down:
                        OnInput(InputMessage.MessageType.ControlsDown, pressed);
                        break;
                    case Keys.Left:
                        OnInput(InputMessage.MessageType.ControlsLeft, pressed);
                        break;
                    case Keys.Right:
                        OnInput(InputMessage.MessageType.ControlsRight, pressed);
                        break;
                }
            }
        }
    }
}