using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ballz.Lobby;

namespace Ballz
{
    class LobbyMenu : Gui.Composite
    {
        LobbyClient Lobby = null;

        string GameName;
        bool IsHost;
        bool IsPrivate;

        public LobbyMenu(bool isHost, string gameName, bool isPrivate) : base("Lobby", true)
        {
            GameName = gameName;
            IsHost = isHost;
            IsPrivate = isPrivate;

            OnUnSelect += () =>
            {
                Lobby?.CloseHostedGameAsync();
                Lobby?.Dispose();
                Lobby = null;
            };

            Lobby = new LobbyClient();
            Lobby.HostGame(gameName, isPrivate);

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

        public override void Update()
        {
            base.Update();

            Lobby?.Update();
        }
    }
}
