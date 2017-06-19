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

        SelectList PlayerList;

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

                Ballz.The().Network.PlayerListChanged -= UpdatePlayerList;
                Ballz.The().Network.Disconnect();
            };

            Open += (s, e) =>
            {
                Lobby = new LobbyClient();
                Lobby.HostGame(gameName, isPrivate);

                Ballz.The().Network.PlayerListChanged += UpdatePlayerList;

                if (isHost)
                {
                    Ballz.The().Network.StartServer();
                }
            };

            AddItem(new Label("Players in Lobby:"));
            PlayerList = new SelectList();
            PlayerList.LockSelection = true;
            AddItem(PlayerList);

            if (isHost)
            {
                AddItem(new Button("Start Game"));
            }
            else
            {
                AddItem(new Label("Waiting for Host to start the Game"));
            }

            AddItem(new Button("Leave Game"));
        }

        public override void Update()
        {
            base.Update();

            Lobby?.Update();
        }

        public void UpdatePlayerList(object sender, LobbyPlayerList list)
        {
            PlayerList.ClearItems();
            foreach(var name in list.PlayerNames)
            {
                PlayerList.AddItem(name);
            }
        }
    }
}
