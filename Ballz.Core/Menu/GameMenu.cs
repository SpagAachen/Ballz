using System.Collections.Generic;

namespace Ballz.Menu
{
    public class GameMenu : LinkedList<GameMenu>
    {
        public enum ItemType
        {
            LABEL,
            INPUTFIELD
        }

        public GameMenu(string name, bool selectable = false, ItemType menuType = ItemType.LABEL)
        {
            Name = name;
            Selectable = selectable;
            Items = new LinkedList<GameMenu>();
            SelectionType = menuType;
        }

        public static GameMenu Default
        {
            get
            {
                // options menu
                var optionsMenu = new GameMenu("Options", true);
                optionsMenu.Items.AddLast(new GameMenu("Not Implemented", false));

                // multiplayer menu
                var networkMenu = new GameMenu("Multiplayer", true);
                // - connect to server
                var networkConnectToMenu = new GameMenu("Connect to", true);
                networkConnectToMenu.Items.AddLast(new GameMenu("Host Name: ", true, ItemType.INPUTFIELD));
                // - start server
                var networkServerMenu = new GameMenu("Start server", true);
                networkServerMenu.Items.AddLast(new GameMenu("Not Implemented", false));
                // - add items
                networkMenu.Items.AddLast(networkConnectToMenu);
                networkMenu.Items.AddLast(networkServerMenu);
                networkMenu.Items.AddLast(new GameMenu("Back", true));

                // main menu
                var mainMenu = new GameMenu("Main Menu");
                mainMenu.Items.AddLast(new GameMenu("Play", true));
                mainMenu.Items.AddLast(optionsMenu);
                mainMenu.Items.AddLast(networkMenu);
                mainMenu.Items.AddLast(new GameMenu("Quit", true));

                mainMenu.SelectedItem = mainMenu.Items.First;
                return mainMenu;
            }
        }

        //The name
        public string Name { get; private set; }

        public LinkedList<GameMenu> Items { get; set; }

        public LinkedListNode<GameMenu> SelectedItem { get; set; }

        public bool Selectable { get; private set; }

        public ItemType SelectionType {get; private set;}

        public string Value{ get; set;} = "";

        public string DisplayName 
        {
            get{ return Name + Value; }
        }
    }
}