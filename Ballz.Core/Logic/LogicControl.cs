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
        private Ballz Game;

        public LogicControl(Ballz game)
        {
            Game = game;

            Composite menu = Game.MainMenu;// = DefaultMenu();
            //push the root menuToPrepare
            activeMenu.Push(menu); //TODO: uncast
            RegisterMenuEvents(menu);

            state = GameState.MenuState;
        }

        public void startGame(bool random = false)
        {
            state = GameState.SimulationState;

            Game.Match.start(random);
            RaiseMessageEvent(new LogicMessage(LogicMessage.MessageType.GameMessage));
        }
     
        private void RegisterMenuEvents(Item menu)
        {
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
                    if(!i.Selectable || !i.Active && !i.ActiveChanged)
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
                    //startGame ();
				break;
			}
		}

        public void HandleInputMessage(object sender, Message message)
        {
            if (message.Kind != Messages.Message.MessageType.InputMessage)
                return;

            if (((InputMessage)message).Kind == InputMessage.MessageType.ControlsConsole && ((InputMessage)message).Pressed.Value)
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
            if (msg.Pressed.Value)
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
            if(msg.Kind == InputMessage.MessageType.RawInput ||  msg.Kind == InputMessage.MessageType.RawBack || msg.Pressed.Value)
            {
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
                            {
                                if (top.SelectedItem != null)
                                    top.SelectedItem.DeActivate();
                                else
                                    top.DeActivate();
                            }
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
                        (top.SelectedItem as IChooseable)?.selectPrevious();
                    break;
                case InputMessage.MessageType.ControlsRight:
                        (top.SelectedItem as IChooseable)?.selectNext();
                    break;
                case InputMessage.MessageType.RawInput:
                    if (msg.Key != null)
                        (top.SelectedItem as IRawInputConsumer)?.HandleRawKey(msg.Key.Value);
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