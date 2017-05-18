using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ballz
{
    class HostGameMenu : Gui.Composite
    {
        public HostGameMenu(bool isMultiplayer) : base(isMultiplayer ? "Host Game" : "Singleplayer Game", true)
        {
            if (isMultiplayer)
            {
                AddItem(new Gui.InputBox("Public Name", true));
            }
            AddItem(new Gui.Label("Map: Desert", true));
            AddItem(new Gui.Label("Turn-based: Yes", true));
            if (isMultiplayer)
            {
                var startGameLabel = new Gui.Label("Open Game", true);
                startGameLabel.OnSelect += () =>
                {
                    Ballz.The().Logic.OpenMenu(new LobbyMenu(true, "Testgame!!!!!!!!!!!!!!!!", false));
                };
                AddItem(startGameLabel);
            }
            else
            {
                AddItem(new Gui.Label("Start Game"));
            }
            AddItem(new Gui.Back());
        }
    }
}
