namespace Ballz.Logic
{
    using Menu;
    using Messages;
    using System;
    using System.Collections.Generic;


    /// <summary>
    /// Logic control Processes Messages and other system reactions with regard to the current gamestate.
    /// It uses Message events to inform relevant classes.
    /// </summary>
    public class LogicControl
   {
      GameState state;
      private GameMenu Menu;
      //private GameMenu ActiveMenu;
      private Stack<GameMenu> ActiveMenu = new Stack<GameMenu> ();

      private enum GameState
      {
         menuState,
         simulationState}

      ;

      private void prepareMenu (GameMenu menu)
      {
         //if we have MenuItems select the first one
         if (menu.Items.Count > 0) {
            //find the first selectable Menu item
            LinkedListNode<GameMenu> MenuItem = menu.Items.First;
            while (MenuItem.Next != null && !MenuItem.Value.Selectable) {
               MenuItem = MenuItem.Next;  
            } 
            //set the found MenuItem as selectedItem
            GameMenu currentMenu = ActiveMenu.Pop ();
            currentMenu.SelectedItem = MenuItem;
            ActiveMenu.Push (currentMenu);
         }
      }
      /*foreach(var subMenu in menu.Items)
         {
            ++index;
            if (subMenu.Selectable) //use the first selectable menu as active one ... 
            {
               ActiveMenu.Push(subMenu);
               ActiveMenu.Peek().Current.Selected = true;
               SelectionIndex.Push(index);
               SelectionLimits.Push (Menu.Items.FindLastIndex(delegate(GameMenu gm) {return gm.Selectable;}));
               break;
            }
         }*/

      public LogicControl ()
      {
         Menu = GameMenu.Default;
         //push the root menu
         ActiveMenu.Push (Menu);
         prepareMenu (Menu);
         state = GameState.menuState;
      }

      public event EventHandler<Message> Message;

      protected virtual void raiseMessageEvent (global::Ballz.Messages.Message msg)
      {
         if (Message != null)
            Message (this, msg);
      }

      public void handleInputMessage (object _sender, Message _message)
      {
         if (_message.Kind == global::Ballz.Messages.Message.MessageType.InputMessage) 
         {
            if (state == GameState.menuState) {
                    menuLogic((InputMessage)_message);
            } 
            else 
            {
               if (state == GameState.simulationState) 
               {
                        gameLogic((InputMessage)_message);
               }
            }
         }
      }

      private void gameLogic(InputMessage msg)
      {
         switch (msg.Kind) 
         {
         case InputMessage.MessageType.ControlsBack:
            state = GameState.menuState;
                    raiseMessageEvent(new global::Ballz.Messages.Message (global::Ballz.Messages.Message.MessageType.LogicMessage));//todo: implement LogicMessage and use it here
            break;
         }
      }

      private void menuLogic (InputMessage msg)
      {
         GameMenu top;
         switch (msg.Kind) {
         case InputMessage.MessageType.ControlsAction:
            LinkedListNode<GameMenu> activatedMenu = ActiveMenu.Peek ().SelectedItem;
            if (activatedMenu != null) {
               if (activatedMenu.Value.Name == "Quit")
                  Ballz.The ().Exit ();
               if (activatedMenu.Value.Name == "Play") {
                  state = GameState.simulationState;
                            raiseMessageEvent(new global::Ballz.Messages.Message (global::Ballz.Messages.Message.MessageType.LogicMessage)); // todo: implement LogicMessage class and use it here
                  return;
               }
               prepareMenu (activatedMenu.Value);
               ActiveMenu.Push (activatedMenu.Value);
                        raiseMessageEvent(new global::Ballz.Messages.MenuMessage (ActiveMenu.Peek ()));
            }
            break;
         case InputMessage.MessageType.ControlsBack:
            if (ActiveMenu.Count == 1) // exit if we are in main menu
                  Ballz.The ().Exit ();
            else
               ActiveMenu.Pop ();
                    raiseMessageEvent(new global::Ballz.Messages.MenuMessage (ActiveMenu.Peek ()));
            break;
         case InputMessage.MessageType.ControlsUp:
            top = ActiveMenu.Pop ();
            if (top.SelectedItem != null) 
            {
               top.SelectedItem = top.SelectedItem.Previous ?? top.Items.Last;
                        raiseMessageEvent(new global::Ballz.Messages.MenuMessage (top));
            }
            ActiveMenu.Push (top);
            break;
         case InputMessage.MessageType.ControlsDown:
            top = ActiveMenu.Pop ();
            if (top.SelectedItem != null) 
            {
               top.SelectedItem = top.SelectedItem.Next ?? top.Items.First;
                        raiseMessageEvent(new global::Ballz.Messages.MenuMessage (top));
            }
            ActiveMenu.Push (top);
            break;
         }
      }         
   }
}

