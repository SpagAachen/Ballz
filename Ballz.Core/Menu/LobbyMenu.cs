using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ballz
{
    class LobbyMenu : Gui.Composite
    {
        public LobbyMenu(bool isHost) : base("Lobby", true)
        {
            AddItem(new Gui.Label("Player in Lobby:"));
            AddItem(new Gui.Label("Player1", fontSize: 0.33f));
            AddItem(new Gui.Label("Player2", fontSize: 0.33f));

            if (isHost)
            {
                AddItem(new Gui.Label("Start Game", true));
            }
            else
            {
                AddItem(new Gui.Label("Waiting for Host to start the Game"));
            }

            AddItem(new Gui.Label("Leave Game", true));
        }
    }
}
