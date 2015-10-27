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
                //just some plain dummy menu entries
                var optionsMenu = new GameMenu("Options", true);
                optionsMenu.Items.AddLast(new GameMenu("Not Implemented", false));

                var mainMenu = new GameMenu("Main Menu");
                mainMenu.Items.AddLast(new GameMenu("Play", true));
                mainMenu.Items.AddLast(optionsMenu);
                mainMenu.Items.AddLast(new GameMenu("Quit", true));

                mainMenu.SelectedItem = mainMenu.Items.First;
                return mainMenu;
            }
        }

        public string Name { get; private set; }

        public LinkedList<GameMenu> Items { get; set; }

        public LinkedListNode<GameMenu> SelectedItem { get; set; }

        public bool Selectable { get; private set; }
    }
}