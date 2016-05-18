using System;
using System.Collections.Generic;
using System.Linq;
using Ballz.Input;
using Ballz.Menu;
using Ballz.Messages;
using Ballz.Sound;

namespace Ballz.Logic
{
    /// <summary>
    ///     Logic control Processes Messages and other system reactions with regard to the current gamestate.
    ///     It uses Message events to inform relevant classes.
    /// </summary>
    public class LogicControl
    {
        private readonly Stack<Composite> activeMenu = new Stack<Composite>();
        private GameState state;
        private bool rawInput;
        private Ballz Game;

        public LogicControl(Ballz game)
        {
            Game = game;
            state = GameState.Unknown;
        }

        public void SetMainMenu(Composite menu)
        {
            activeMenu.Clear();
            activeMenu.Push(menu); //TODO: uncast
            RegisterMenuEvents(menu);
            state = GameState.MenuState;
        }

        public void StartGame(GameSession.Logic.GameSettings settings, bool remoteControlled = false, int localPlayerId = -1)
        {
            // Go back to main menu so it will show when the user enters the menu later
            MenuGoBack();
            // Select the "Continue" entry
            activeMenu.Peek().SelectIndex(0);

            state = GameState.SimulationState;
            if (Game.Match != null)
                Game.Match.Dispose();

            Game.Match = settings.GameMode.StartSession(Game, settings, remoteControlled, localPlayerId);
            Game.Match.Start();
            RaiseMessageEvent(new LogicMessage(LogicMessage.MessageType.GameMessage));
        }

        public void ContinueGame()
        {
            state = GameState.SimulationState;
            RaiseMessageEvent(new LogicMessage(LogicMessage.MessageType.GameMessage));
        }

        private void RegisterMenuEvents(Item menu)
        {
            if (menu == null)
                return;

            menu.BindSelectHandler<Composite>(c =>
            {
                activeMenu.Push(c);
                RaiseMessageEvent(new MenuMessage(activeMenu.Peek()));
            });

            menu.BindSelectHandler<Back>(b =>
            {
                activeMenu.Pop();
                RaiseMessageEvent(new MenuMessage(activeMenu.Peek()));
            });

            menu.BindSelectHandler<InputBox>(ib =>
            {
                rawInput = true;
                RaiseMessageEvent(new MenuMessage(ib));
            });

            menu.BindUnSelectHandler<Item>(i =>
            {
                if (!i.Selectable || !i.Active && !i.ActiveChanged)
                {
                    activeMenu.Pop();
                    RaiseMessageEvent(new MenuMessage(activeMenu.Peek()));
                }
            });
        }

        public event EventHandler<Message> Message;

        protected virtual void RaiseMessageEvent(Message msg)
        {
            Message?.Invoke(this, msg);
        }

        public void HandleNetworkMessage(object sender, Message message)
        {
            if (message.Kind != Messages.Message.MessageType.NetworkMessage)
                return;
            var msg = (NetworkMessage)message;
            switch (msg.Kind)
            {
                case NetworkMessage.MessageType.ConnectedToServer:
                    //TODO: show lobby!!!!<<<<<<<<<<<<<<<<<<<<<<<<<
                    //Game is started by a message from server
                    break;
            }
        }

        public void HandleInputMessage(object sender, Message message)
        {
            if (message.Kind != Messages.Message.MessageType.InputMessage)
                return;

            if (((InputMessage)message).Kind == InputMessage.MessageType.ControlsConsole && ((InputMessage)message).Pressed)
                RaiseMessageEvent(new LogicMessage(LogicMessage.MessageType.PerformanceMessage));

            switch (state)
            {
                case GameState.MenuState:
                    MenuLogic((InputMessage)message);
                    break;
                case GameState.SimulationState:
                    GameLogic((InputMessage)message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            CheckInputMode((InputTranslator)sender);
        }

        private void GameLogic(InputMessage msg)
        {
            if (msg.Pressed)
            {
                switch (msg.Kind)
                {
                    case InputMessage.MessageType.ControlsBack:
                        state = GameState.MenuState;
                        RaiseMessageEvent(new LogicMessage(LogicMessage.MessageType.GameMessage));
                        //todo: implement LogicMessage and use it here
                        break;
                    case InputMessage.MessageType.ControlsUp:
                        break;
                    case InputMessage.MessageType.ControlsDown:
                        break;
                    case InputMessage.MessageType.ControlsLeft:
                        break;
                    case InputMessage.MessageType.ControlsRight:
                        break;
                    case InputMessage.MessageType.ControlsAction:
                        break;
                    case InputMessage.MessageType.RawInput:
                        break;
                    default:
                        //throw new ArgumentOutOfRangeException();
                        break;
                }
            }
        }

        private void MenuLogic(InputMessage msg)
        {
            Composite top = activeMenu.Peek();
            if (msg.Kind == InputMessage.MessageType.RawInput || msg.Kind == InputMessage.MessageType.RawBack || msg.Pressed)
            {
                switch (msg.Kind)
                {
                    case InputMessage.MessageType.ControlsAction:
                        top.SelectedItem?.Activate();
                        Ballz.The().Services.GetService<SoundControl>().PlaySound(SoundControl.AcceptSound);
                        break;
                    case InputMessage.MessageType.ControlsBack:
                        Ballz.The().Services.GetService<SoundControl>().PlaySound(SoundControl.DeclineSound);
                        if (activeMenu.Count == 1) // exit if we are in main menuToPrepare
                            Ballz.The().Exit();     //TODO: this is rather ugly find a nice way to terminate the programm like sending a termination message
                        else
                        {
                            if (rawInput)
                                rawInput = false;
                            else
                            {
                                MenuGoBack();
                            }
                        }

                        RaiseMessageEvent(new MenuMessage(activeMenu.Peek()));
                        break;
                    case InputMessage.MessageType.ControlsUp:
                        if (top.SelectedItem != null)
                        {
                            Ballz.The().Services.GetService<SoundControl>().PlaySound(SoundControl.SelectSound);
                            top.SelectPrevious();
                            RaiseMessageEvent(new MenuMessage(top));
                        }

                        break;
                    case InputMessage.MessageType.ControlsDown:
                        if (top.SelectedItem != null)
                        {
                            Ballz.The().Services.GetService<SoundControl>().PlaySound(SoundControl.SelectSound);
                            top.SelectNext();
                            RaiseMessageEvent(new MenuMessage(top));
                        }

                        break;
                    case InputMessage.MessageType.ControlsLeft:
                        (top.SelectedItem as IChooseable)?.SelectPrevious();
                        break;
                    case InputMessage.MessageType.ControlsRight:
                        (top.SelectedItem as IChooseable)?.SelectNext();
                        break;
                    case InputMessage.MessageType.RawInput:
                        if (msg.Key != null)
                            (top.SelectedItem as IRawInputConsumer)?.HandleRawKey(msg.Key);
                        break;
                    case InputMessage.MessageType.RawBack:
                        (top.SelectedItem as IRawInputConsumer)?.HandleBackspace();
                        break;
                    default:
                        //throw new ArgumentOutOfRangeException();
                        break;
                }
            }
        }

        private void MenuGoBack()
        {
            var top = activeMenu.Peek();
            if (top.SelectedItem != null)
                top.SelectedItem.DeActivate();
            else
                top.DeActivate();
        }

        /// <summary>
        /// Checks the input mode.
        /// TODO: refactor the Menu logic to a menuLogic class or use a partial class definition as this file seems to become messy
        /// </summary>
        void CheckInputMode(InputTranslator translator)
        {
            if (rawInput)
                translator.Mode = InputTranslator.InputMode.RAW;
        }

        private enum GameState
        {
            Unknown,
            MenuState,
            SimulationState
        }
    }
}