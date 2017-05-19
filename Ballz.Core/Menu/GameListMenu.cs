using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ballz.Lobby;
using GeonBit.UI.Entities;

namespace Ballz
{
    class GameListMenu : Gui.MenuPanel
    {
        LobbyClient Lobby = null;
        public GameListMenu() : base("Join Online Game")
        {
            Open += (s,e) =>
            {
                Lobby = new LobbyClient();
                Lobby.UpdatedGameList += UpdateGameList;
            };
            Close += (s, e) =>
            {
                Lobby?.Dispose();
                Lobby = null;
            };

            AddItem(new Label("Loading Games..."));
            AddItem(new Gui.BackButton());
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
                //AddItem(new Label(g.Name,));
            }

            if(games.Length == 0)
            {
                //AddItem(new Gui.Label("No games found."));
            }

            AddItem(new Gui.BackButton());
        }
    }
}
