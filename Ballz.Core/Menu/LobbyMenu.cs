using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ballz.Lobby;
using GeonBit.UI.Entities;

namespace Ballz
{
    class LobbyMenu : Gui.MenuPanel
    {
        LobbyClient Lobby = null;

        string GameName;
        bool IsHost;
        bool IsPrivate;

        public LobbyMenu(bool isHost, string gameName, bool isPrivate) : base("Lobby")
        {
            GameName = gameName;
            IsHost = isHost;
            IsPrivate = isPrivate;

            Close += (s,e) =>
            {
                Lobby?.CloseHostedGameAsync();
                Lobby?.Dispose();
                Lobby = null;
            };

            Open += (s, e) =>
            {
                Lobby = new LobbyClient();
                Lobby.HostGame(gameName, isPrivate);

                AddItem(new Label("Player in Lobby:"));
                //AddItem(new Label("Player1", fontSize: 0.33f));
                //AddItem(new Label("Player2", fontSize: 0.33f));

                if (isHost)
                {
                    AddItem(new Label("Start Game"));
                }
                else
                {
                    AddItem(new Label("Waiting for Host to start the Game"));
                }

                AddItem(new Label("Leave Game"));
            };
        }

        public override void Update()
        {
            base.Update();

            Lobby?.Update();
        }
    }
}
