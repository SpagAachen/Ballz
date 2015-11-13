using System.Collections.Generic;

namespace Ballz.Menu
{
    public class GameMenu : LinkedList<GameMenu>
    {
        public GameMenu(string name, bool selectable = false)
        {
            Name = name;
            Selectable = selectable;
            Items = new LinkedList<GameMenu>();
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
                networkConnectToMenu.Items.AddLast(new GameMenu("Not Implemented", false));
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
    }
}