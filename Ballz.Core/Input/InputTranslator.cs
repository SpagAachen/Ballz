using Ballz.GameSession.Logic;
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
        private GamePadState[] previousGamePadState;
        private PlayerIndex[] gamePadPlayerIndex;
        
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
            previousGamePadState = new GamePadState[4];

            gamePadPlayerIndex = new PlayerIndex[4];
            gamePadPlayerIndex[0] = PlayerIndex.One;
            gamePadPlayerIndex[1] = PlayerIndex.Two;
            gamePadPlayerIndex[2] = PlayerIndex.Three;
            gamePadPlayerIndex[3] = PlayerIndex.Four;
            //previousGamePadState = GamePad.GetState(
        }

        void RawHandler(object sender, TextInputEventArgs eventArgs)
        {
            OnInput(InputMessage.MessageType.RawInput, false, eventArgs.Character);
        }

        private void OnInput(InputMessage.MessageType inputMessage, bool pressed = false, char key = char.MinValue, Player player = null)
        {
            Input?.Invoke(this, new InputMessage(inputMessage, pressed, key, player)); //todo: use object pooling and specify message better
        }

        public void InjectInputMessage(InputMessage message, Player player)
        {
            OnInput(message.Kind, message.Pressed, message.Key, player);
        }

        private void ProcessGamePadInput(int p)
        {       
            int sign = 1;
            GamePadState currentState = GamePad.GetState(gamePadPlayerIndex[p-1]);
            if (System.Environment.OSVersion.Platform != PlatformID.Unix || System.Environment.OSVersion.Platform != PlatformID.MacOSX)
                sign = -1;
            if (currentState.IsConnected)
            {                                    
                if (previousGamePadState[p - 1].DPad.Up == ButtonState.Released && currentState.DPad.Up == ButtonState.Pressed)
                    OnInput(InputMessage.MessageType.ControlsUp, true, char.MinValue, Ballz.The().Match?.PlayerByNumber(p));
                if (previousGamePadState[p - 1].DPad.Down == ButtonState.Released && currentState.DPad.Down == ButtonState.Pressed)
                    OnInput(InputMessage.MessageType.ControlsDown, true, char.MinValue, Ballz.The().Match?.PlayerByNumber(p));
                if (previousGamePadState[p - 1].DPad.Left == ButtonState.Released && currentState.DPad.Left == ButtonState.Pressed)
                    OnInput(InputMessage.MessageType.ControlsLeft, true, char.MinValue, Ballz.The().Match?.PlayerByNumber(p));
                if (previousGamePadState[p - 1].DPad.Right == ButtonState.Released && currentState.DPad.Right == ButtonState.Pressed)
                    OnInput(InputMessage.MessageType.ControlsRight, true, char.MinValue, Ballz.The().Match?.PlayerByNumber(p));
                if (previousGamePadState[p - 1].IsButtonUp(Buttons.B) && currentState.IsButtonDown(Buttons.B))
                    OnInput(InputMessage.MessageType.ControlsBack, true, char.MinValue, Ballz.The().Match?.PlayerByNumber(p));
                if (previousGamePadState[p - 1].IsButtonUp(Buttons.A) && currentState.IsButtonDown(Buttons.A))
                    OnInput(InputMessage.MessageType.ControlsAction, true, char.MinValue, Ballz.The().Match?.PlayerByNumber(p));
                if (previousGamePadState[p - 1].IsButtonUp(Buttons.X) && currentState.IsButtonDown(Buttons.X))
                    OnInput(InputMessage.MessageType.ControlsJump, true, char.MinValue, Ballz.The().Match?.PlayerByNumber(p));

                if(previousGamePadState[p - 1].ThumbSticks.Left.X <= 0.5 && currentState.ThumbSticks.Left.X > 0.5)
                    OnInput(InputMessage.MessageType.ControlsRight, true, char.MinValue, Ballz.The().Match?.PlayerByNumber(p));
                if(previousGamePadState[p - 1].ThumbSticks.Left.X >= -0.5 && currentState.ThumbSticks.Left.X < -0.5)
                    OnInput(InputMessage.MessageType.ControlsLeft, true, char.MinValue, Ballz.The().Match?.PlayerByNumber(p));
                if(previousGamePadState[p - 1].ThumbSticks.Left.Y*sign <= 0.5 && currentState.ThumbSticks.Left.Y*sign > 0.5)
                    OnInput(InputMessage.MessageType.ControlsDown, true, char.MinValue, Ballz.The().Match?.PlayerByNumber(p));
                if(previousGamePadState[p - 1].ThumbSticks.Left.Y*sign >= -0.5 && currentState.ThumbSticks.Left.Y*sign < -0.5)
                    OnInput(InputMessage.MessageType.ControlsUp, true, char.MinValue, Ballz.The().Match?.PlayerByNumber(p));
                if(previousGamePadState[p-1].Buttons.LeftStick == ButtonState.Released && currentState.Buttons.LeftStick == ButtonState.Pressed)
                    OnInput(InputMessage.MessageType.ControlsJump, true, char.MinValue, Ballz.The().Match?.PlayerByNumber(p));

                if(previousGamePadState[p - 1].ThumbSticks.Left.X > 0.5 && currentState.ThumbSticks.Left.X <= 0.5)
                    OnInput(InputMessage.MessageType.ControlsRight, false, char.MinValue, Ballz.The().Match?.PlayerByNumber(p));
                if(previousGamePadState[p - 1].ThumbSticks.Left.X < -0.5 && currentState.ThumbSticks.Left.X >= -0.5)
                    OnInput(InputMessage.MessageType.ControlsLeft, false, char.MinValue, Ballz.The().Match?.PlayerByNumber(p));
                if(previousGamePadState[p - 1].ThumbSticks.Left.Y*sign > 0.5 && currentState.ThumbSticks.Left.Y*sign <= 0.5)
                    OnInput(InputMessage.MessageType.ControlsDown, false, char.MinValue, Ballz.The().Match?.PlayerByNumber(p));
                if(previousGamePadState[p - 1].ThumbSticks.Left.Y*sign < -0.5 && currentState.ThumbSticks.Left.Y*sign >= -0.5)
                    OnInput(InputMessage.MessageType.ControlsUp, false, char.MinValue, Ballz.The().Match?.PlayerByNumber(p));
                if(previousGamePadState[p-1].Buttons.LeftStick == ButtonState.Pressed && currentState.Buttons.LeftStick == ButtonState.Released)
                    OnInput(InputMessage.MessageType.ControlsJump, false, char.MinValue, Ballz.The().Match?.PlayerByNumber(p));
                
                if (previousGamePadState[p - 1].DPad.Up == ButtonState.Pressed && currentState.DPad.Up == ButtonState.Released)
                    OnInput(InputMessage.MessageType.ControlsUp, false, char.MinValue, Ballz.The().Match?.PlayerByNumber(p));
                if (previousGamePadState[p - 1].DPad.Down == ButtonState.Pressed && currentState.DPad.Down == ButtonState.Released)
                    OnInput(InputMessage.MessageType.ControlsDown, false, char.MinValue, Ballz.The().Match?.PlayerByNumber(p));
                if (previousGamePadState[p - 1].DPad.Left == ButtonState.Pressed && currentState.DPad.Left == ButtonState.Released)
                    OnInput(InputMessage.MessageType.ControlsLeft, false, char.MinValue, Ballz.The().Match?.PlayerByNumber(p));
                if (previousGamePadState[p - 1].DPad.Right == ButtonState.Pressed && currentState.DPad.Right == ButtonState.Released)
                    OnInput(InputMessage.MessageType.ControlsRight, false, char.MinValue, Ballz.The().Match?.PlayerByNumber(p));
                if (previousGamePadState[p - 1].IsButtonDown(Buttons.B) && currentState.IsButtonUp(Buttons.B))
                    OnInput(InputMessage.MessageType.ControlsBack, false, char.MinValue, Ballz.The().Match?.PlayerByNumber(p));
                if (previousGamePadState[p - 1].IsButtonDown(Buttons.A) && currentState.IsButtonUp(Buttons.A))
                    OnInput(InputMessage.MessageType.ControlsAction, false, char.MinValue, Ballz.The().Match?.PlayerByNumber(p));
                if (previousGamePadState[p - 1].IsButtonDown(Buttons.X) && currentState.IsButtonUp(Buttons.X))
                    OnInput(InputMessage.MessageType.ControlsJump, false, char.MinValue, Ballz.The().Match?.PlayerByNumber(p));
            }

            previousGamePadState[p - 1] = currentState;
        }

        public override void Update(GameTime gameTime)
        {
            for (int i = 1; i < 5; i++)
            {
                ProcessGamePadInput(i);
            }

            currentState = Keyboard.GetState();
            if (currentState != previousState)
            {
                if (Mode == InputMode.PROCESSED)
                {
                    ProcessInput();
                }
                else
                {
                    ProcessRawInput();
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
        private void ProcessRawInput()
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
        private List<Keys> ChangedKeys(Keys[] a , Keys[] b)
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

        private void ProcessInput()
        {
            //Keys that are pressed now but where not pressed previously are keys that got pressed
            List<Keys> pressedKeys = ChangedKeys(currentState.GetPressedKeys(), previousState.GetPressedKeys());
            //keys that where pressed previously but are not currently are Keys that got released
            List<Keys> releasedKeys = ChangedKeys(previousState.GetPressedKeys(), currentState.GetPressedKeys());

            EmitKeyMessages(pressedKeys, true);
            EmitKeyMessages(releasedKeys, false);
        }

        private void EmitKeyMessages(List<Keys> keyList, bool pressed)
        {
            foreach(Keys theKey in keyList)
            {
                switch (theKey)
                {
                    case Keys.OemTilde:
                    case Keys.OemPipe:
                        OnInput(InputMessage.MessageType.ControlsConsole, pressed);
                        break;
                    case Keys.Escape:
                        OnInput(InputMessage.MessageType.ControlsBack, pressed);
                        break;
                    case Keys.LeftControl:
                        OnInput(InputMessage.MessageType.ControlsAction, pressed, char.MinValue, Ballz.The().Match?.PlayerByNumber(1));
                        break;
                    case Keys.Up:
                        OnInput(InputMessage.MessageType.ControlsUp, pressed, char.MinValue, Ballz.The().Match?.PlayerByNumber(1));
                        break;
                    case Keys.Down:
                        OnInput(InputMessage.MessageType.ControlsDown, pressed, char.MinValue, Ballz.The().Match?.PlayerByNumber(1));
                        break;
                    case Keys.Left:
                        OnInput(InputMessage.MessageType.ControlsLeft, pressed, char.MinValue, Ballz.The().Match?.PlayerByNumber(1));
                        break;
                    case Keys.Right:
                        OnInput(InputMessage.MessageType.ControlsRight, pressed, char.MinValue, Ballz.The().Match?.PlayerByNumber(1));
                        break;
                    case Keys.Enter:
                    case Keys.RightControl:
                        OnInput(InputMessage.MessageType.ControlsAction, pressed, char.MinValue, Ballz.The().Match?.PlayerByNumber(1));
                        break;
                    case Keys.Space:
                        OnInput(InputMessage.MessageType.ControlsJump, pressed, char.MinValue, Ballz.The().Match?.PlayerByNumber(1));
                        break;
                    case Keys.PageUp:
                        OnInput(InputMessage.MessageType.ControlsPreviousWeapon, pressed, char.MinValue, Ballz.The().Match?.PlayerByNumber(1));
                        break;
                    case Keys.PageDown:
                        OnInput(InputMessage.MessageType.ControlsNextWeapon, pressed, char.MinValue, Ballz.The().Match?.PlayerByNumber(1));
                        break;
                    case Keys.W:
                        OnInput(InputMessage.MessageType.ControlsUp, pressed, char.MinValue, Ballz.The().Match?.PlayerByNumber(2));
                        break;
                    case Keys.S:
                        OnInput(InputMessage.MessageType.ControlsDown, pressed, char.MinValue, Ballz.The().Match?.PlayerByNumber(2));
                        break;
                    case Keys.A:
                        OnInput(InputMessage.MessageType.ControlsLeft, pressed, char.MinValue, Ballz.The().Match?.PlayerByNumber(2));
                        break;
                    case Keys.D:
                        OnInput(InputMessage.MessageType.ControlsRight, pressed, char.MinValue, Ballz.The().Match?.PlayerByNumber(2));
                        break;
                    case Keys.E:
                        OnInput(InputMessage.MessageType.ControlsAction, pressed, char.MinValue, Ballz.The().Match?.PlayerByNumber(2));
                        break;
                    case Keys.Q:
                        OnInput(InputMessage.MessageType.ControlsJump, pressed, char.MinValue, Ballz.The().Match?.PlayerByNumber(2));
                        break;
                }
            }
        }
    }
}