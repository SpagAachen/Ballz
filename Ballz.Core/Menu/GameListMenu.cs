using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ballz.Lobby;

namespace Ballz
{
    class GameListMenu : Gui.Composite
    {
        LobbyClient Lobby = null;
        public GameListMenu() : base("Join Online Game", true)
        {
            OnSelect += () =>
            {
                Lobby = new LobbyClient();
                Lobby.UpdatedGameList += UpdateGameList;
            };
            OnUnSelect += () =>
            {
                Lobby?.Dispose();
                Lobby = null;
            };

            AddItem(new Gui.Label("Loading Games..."));
            AddItem(new Gui.Back());
        }

        string SelectedGameId = "";

        public override void Update()
        {
            base.Update();
            Lobby?.Update();
        }

        public void UpdateGameList(object sender, PublicGameInfo[] games)
        {
            Members.Clear();

            foreach(PublicGameInfo g in games)
            {
                AddItem(new Gui.Label(g.Name, true));
            }

            if(games.Length == 0)
            {
                AddItem(new Gui.Label("No games found."));
            }

            AddItem(new Gui.Back());
        }
    }
}
