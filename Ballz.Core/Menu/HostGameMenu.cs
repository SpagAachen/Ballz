using GeonBit.UI.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ballz
{
    class HostGameMenu : Gui.MenuPanel
    {
        public HostGameMenu(bool isMultiplayer) : base(isMultiplayer ? "Host Game" : "Singleplayer Game")
        {
            if (isMultiplayer)
            {
                //AddItem(new Gui.InputBox("Public Name", true));
            }
            AddItem(new Label("Map: Desert"));
            AddItem(new Label("Turn-based: Yes"));
            if (isMultiplayer)
            {
                var startGameLabel = new Label("Open Game");
                //startGameLabel.OnSelect += () =>
                //{
                //    Ballz.The().Logic.OpenMenu(new LobbyMenu(true, "Testgame!!!!!!!!!!!!!!!!", false));
                //};
                AddItem(startGameLabel);
            }
            else
            {
                AddItem(new Label("Start Game"));
            }
            AddItem(new Gui.BackButton());
        }
    }
}
