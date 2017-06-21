using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ballz.Lobby;
using GeonBit.UI.Entities;
using Ballz.Gui;
using Ballz.GameSession.Logic;

namespace Ballz
{
    class LobbyMenu : Gui.MenuPanel
    {
        LobbyClient Lobby = null;
        
        bool IsHost;
        
        SelectList PlayerList;
        MatchSettings MatchSettings;

        public LobbyMenu(bool isHost, MatchSettings settings = null) : base("Lobby")
        {
            if(isHost && settings == null)
            {
                throw new ArgumentNullException("settings must not be null when isHost is true");
            }

            IsHost = isHost;
            MatchSettings = settings;

            Close += (s,e) =>
            {
                if (isHost)
                {
                    Lobby?.CloseHostedGameAsync();
                    Lobby?.Dispose();
                    Lobby = null;
                }

                Ballz.The().Network.PlayerListChanged -= UpdatePlayerList;
            };

            Open += (s, e) =>
            {
                Ballz.The().Network.PlayerListChanged += UpdatePlayerList;

                UpdatePlayerList(this, Ballz.The().Network.PlayerList);

                if (isHost)
                {
                    Lobby = new LobbyClient();
                    var gameInfo = Lobby.HostGame(MatchSettings.GameName, MatchSettings.IsPrivate);
                    Ballz.The().Network.StartServer(gameInfo);
                }
            };

            AddItem(new Label("Players in Lobby:"));
            PlayerList = new SelectList();
            PlayerList.LockSelection = true;
            AddItem(PlayerList);

            if (isHost)
            {
                var startGameBtn = new Button("Start Game");
                startGameBtn.OnClick += (e) =>
                {
                    Ballz.The().Network.StartNetworkGame(MatchSettings, 0);
                };
                AddItem(startGameBtn);
            }
            else
            {
                AddItem(new Label("Waiting for Host to start the Game"));
            }

            AddItem(new BackButton(text: "Leave Game"));

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
