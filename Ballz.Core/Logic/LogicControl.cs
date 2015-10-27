using System;
using System.Collections.Generic;
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
        //private GameMenu ActiveMenu;
        private readonly Stack<GameMenu> activeMenu = new Stack<GameMenu>();
        private GameState state;
        /*foreach(var subMenu in menuToPrepare.Items)
         {
            ++index;
            if (subMenu.Selectable) //use the first selectable menuToPrepare as active one ... 
            {
               ActiveMenu.Push(subMenu);
               ActiveMenu.Peek().Current.Selected = true;
               SelectionIndex.Push(index);
               SelectionLimits.Push (Menu.Items.FindLastIndex(delegate(GameMenu gm) {return gm.Selectable;}));
               break;
            }
         }*/

        public LogicControl()
        {
            var menu = GameMenu.Default;
            //push the root menuToPrepare
            activeMenu.Push(menu);
            PrepareMenu(menu);
            state = GameState.MenuState;
        }

        private void PrepareMenu(GameMenu menuToPrepare)
        {
            //if we have MenuItems select the first one
            if (menuToPrepare.Items.Count <= 0)
                return;

            //find the first selectable Menu item
            var menuItem = menuToPrepare.Items.First;
            while (menuItem.Next != null && !menuItem.Value.Selectable)
            {
                menuItem = menuItem.Next;
            }
            //set the found MenuItem as selectedItem
            var currentMenu = activeMenu.Pop();
            currentMenu.SelectedItem = menuItem;
            activeMenu.Push(currentMenu);
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
        }

        private void GameLogic(InputMessage msg)
        {
            switch (msg.Kind)
            {
                case InputMessage.MessageType.ControlsBack:
                    state = GameState.MenuState;
                    RaiseMessageEvent(new Message(Messages.Message.MessageType.LogicMessage));
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
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void MenuLogic(InputMessage msg)
        {
            GameMenu top;
            switch (msg.Kind)
            {
                case InputMessage.MessageType.ControlsAction:
                    var activatedMenu = activeMenu.Peek().SelectedItem;
                    if (activatedMenu != null)
                    {
                        if (activatedMenu.Value.Name == "Quit")
                            Ballz.The().Exit();
                        if (activatedMenu.Value.Name == "Play")
                        {
                            state = GameState.SimulationState;
                            RaiseMessageEvent(new Message(Messages.Message.MessageType.LogicMessage));
                            // todo: implement LogicMessage class and use it here
                            return;
                        }
                        PrepareMenu(activatedMenu.Value);
                        activeMenu.Push(activatedMenu.Value);
                        RaiseMessageEvent(new MenuMessage(activeMenu.Peek()));
                    }
                    break;
                case InputMessage.MessageType.ControlsBack:
                    if (activeMenu.Count == 1) // exit if we are in main menuToPrepare
                        Ballz.The().Exit();
                    else
                        activeMenu.Pop();
                    RaiseMessageEvent(new MenuMessage(activeMenu.Peek()));
                    break;
                case InputMessage.MessageType.ControlsUp:
                    top = activeMenu.Pop();
                    if (top.SelectedItem != null)
                    {
                        top.SelectedItem = top.SelectedItem.Previous ?? top.Items.Last;
                        RaiseMessageEvent(new MenuMessage(top));
                    }
                    activeMenu.Push(top);
                    break;
                case InputMessage.MessageType.ControlsDown:
                    top = activeMenu.Pop();
                    if (top.SelectedItem != null)
                    {
                        top.SelectedItem = top.SelectedItem.Next ?? top.Items.First;
                        RaiseMessageEvent(new MenuMessage(top));
                    }
                    activeMenu.Push(top);
                    break;
                case InputMessage.MessageType.ControlsLeft:
                    break;
                case InputMessage.MessageType.ControlsRight:
                    break;
                case InputMessage.MessageType.RawInput:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private enum GameState
        {
            MenuState,
            SimulationState
        };
    }
}