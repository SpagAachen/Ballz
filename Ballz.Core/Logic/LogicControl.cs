using System;
using System.Collections.Generic;
using System.Linq;
using Ballz.Input;
using Ballz.Menu;
using Ballz.Messages;

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

        public LogicControl()
        {
            var menu = DefaultMenu();
            //push the root menuToPrepare
            activeMenu.Push(menu); //TODO: uncast
            RegisterMenuEvents(menu);

            state = GameState.MenuState;
        }

        private Composite DefaultMenu()
        {
            // options menu
            var optionsMenu = new Composite("Options", true);
            optionsMenu.AddItem(new Label("Not Implemented", false));

            // multiplayer menu
            var networkMenu = new Composite("Multiplayer", true);
            // - connect to server
            var networkConnectToMenu = new Composite("Connect to", true);
            networkConnectToMenu.AddItem(new InputBox("Host Name: ", true));
            // - start server
            var networkServerMenu = new Composite("Start server", true);
            networkServerMenu.AddItem(new Label("Not Implemented", false));
            // - add items
            networkMenu.AddItem(networkConnectToMenu);
            networkMenu.AddItem(networkServerMenu);
            networkMenu.AddItem(new Back());

            // main menu
            var mainMenu = new Composite("Main Menu");

            var play = new Label("Play",true);
            play.OnSelect += () =>
            {
                state = GameState.SimulationState;
                RaiseMessageEvent(new LogicMessage(LogicMessage.MessageType.GameMessage));
            };
          
            mainMenu.AddItem(play);
            mainMenu.AddItem(optionsMenu);
            mainMenu.AddItem(networkMenu);

            var quit = new Label("Quit", true);
            quit.OnSelect += () => Ballz.The().Exit();
            mainMenu.AddItem(quit);

            return mainMenu;
        }

        private void RegisterMenuEvents(Item menu)
        {
            menu.BindCompositeHandler(c =>
            {
                activeMenu.Push(c);
                RaiseMessageEvent(new MenuMessage(activeMenu.Peek()));
            });

            menu.BindBackHandler(b =>
            {
                activeMenu.Pop();
                RaiseMessageEvent(new MenuMessage(activeMenu.Peek()));
            });
            
            menu.BindInputBoxHandler(ib =>
            {
                rawInput = true;
                RaiseMessageEvent(new MenuMessage(ib));
            });
            
        }

        public event EventHandler<Message> Message;

        protected virtual void RaiseMessageEvent(Message msg)
        {
            Message?.Invoke(this, msg);
        }

        public void HandleInputMessage(object sender, Message message)
        {
            if (message.Kind != Messages.Message.MessageType.InputMessage)
                return;

            if (((InputMessage)message).Kind == InputMessage.MessageType.ControlsConsole)
                RaiseMessageEvent(new LogicMessage(LogicMessage.MessageType.PerformanceMessage));

            switch (state)
            {
                case GameState.MenuState:
                    MenuLogic((InputMessage) message);
                    break;
                case GameState.SimulationState:
                    GameLogic((InputMessage) message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            CheckInputMode((InputTranslator)sender);
        }

        private void GameLogic(InputMessage msg)
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

        private void MenuLogic(InputMessage msg)
        {
            Composite top = activeMenu.Peek();
            switch (msg.Kind)
            {
                case InputMessage.MessageType.ControlsAction:
                    top.SelectedItem?.Activate();
                    break;
                case InputMessage.MessageType.ControlsBack:
                    if (activeMenu.Count == 1) // exit if we are in main menuToPrepare
                        Ballz.The().Exit();     //TODO: this is rather ugly find a nice way to terminate the programm like sending a termination message
                    else
                    {
                        if (rawInput)
                            rawInput = false;
                        else
                            activeMenu.Pop();
                    }
                    RaiseMessageEvent(new MenuMessage(activeMenu.Peek()));
                    break;
                case InputMessage.MessageType.ControlsUp:
                    if (top.SelectedItem != null)
                    {
                        top.SelectPrevious();
                        RaiseMessageEvent(new MenuMessage(top));
                    }
                    break;
                case InputMessage.MessageType.ControlsDown:
                    if (top.SelectedItem != null)
                    {
                        top.SelectNext();
                        RaiseMessageEvent(new MenuMessage(top));
                    }
                    break;
                case InputMessage.MessageType.ControlsLeft:
                    break;
                case InputMessage.MessageType.ControlsRight:
                    break;
                case InputMessage.MessageType.RawInput:
                    if (msg.Key != null)
                        top.SelectedItem.HandleRawKey(msg.Key.Value);
                    break;                    
                case InputMessage.MessageType.RawBack:
                    top.SelectedItem.HandleBackspace();
                    break;
                default:
                    //throw new ArgumentOutOfRangeException();
                    break;
            }
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
            MenuState,
            SimulationState
        };
    }
}