using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ballz.Lobby;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using System.Net;

namespace Ballz
{
    class GameListMenu : Gui.MenuPanel
    {
        LobbyClient Lobby = null;

        Label StatusLabel = new Label("Updating Game List");

        PanelTabs OnlineLocalTabs = new PanelTabs();

        PublicGameInfo[] OnlineGameListData = null;
        SelectList OnlineGameList = new SelectList(new Vector2(0, 350));
        Button JoinOnlineButton = new Gui.MenuButton("Join");
        PublicGameInfo[] LocalGameListData = null;
        SelectList LocalGameList = new SelectList(new Vector2(0, 350));
        Button JoinLocalButton = new Gui.MenuButton("Join");

        public GameListMenu() : base("Join Game")
        {
            Size = new Vector2(800, 0);
            Open += (s,e) =>
            {
                Lobby = new LobbyClient();
                Lobby.UpdatedOnlineGameList += UpdateOnlineGameList;
                Lobby.UpdatedLocalGameList += UpdateLocalGameList;
            };
            Close += (s, e) =>
            {
                Lobby?.Dispose();
                Lobby = null;
            };

            var panel = new Panel(new Vector2(0, -1), PanelSkin.None);

            var onlineTab = OnlineLocalTabs.AddTab("Online", PanelSkin.None);
            onlineTab.panel.AddChild(new Header("Join Online Game"));
            onlineTab.panel.AddChild(OnlineGameList);
            onlineTab.panel.AddChild(StatusLabel);
            JoinOnlineButton.OnClick += (e) => JoinSelectedGame(online: true);
            onlineTab.panel.AddChild(JoinOnlineButton);
            onlineTab.panel.AddChild(new Gui.BackButton());

            var localTab = OnlineLocalTabs.AddTab("Local", PanelSkin.None);
            localTab.panel.AddChild(new Header("Join Local Game"));
            localTab.panel.AddChild(LocalGameList);
            JoinLocalButton.OnClick += (e) => JoinSelectedGame(online: false);
            localTab.panel.AddChild(JoinLocalButton);
            localTab.panel.AddChild(new Gui.BackButton());

            panel.AddChild(OnlineLocalTabs);

            AddChild(panel);
        }

        string SelectedGameId = "";

        public override void Update()
        {
            base.Update();
            Lobby?.Update();
        }

        public void UpdateSelectList(SelectList list, PublicGameInfo[] games)
        {
            var selection = list.SelectedValue;
            var scrollPos = list.ScrollPosition;
            var isFocussed = list.IsFocused;
            
            list.ClearItems();
            if (games.Length == 0)
            {
                list.AddItem("[No games found]");
            }
            else
            {
                foreach (PublicGameInfo g in games)
                {
                    list.AddItem(g.Name);
                }
            }

            try
            {
                list.ScrollPosition = scrollPos;
                if (isFocussed)
                {
                    GeonBit.UI.UserInterface.ActiveEntity = list;
                }
                list.SelectedValue = selection;
            }
            catch (System.Exception e)
            {
                list.SelectedIndex = 0;
            }
        }
        public void UpdateOnlineGameList(object sender, PublicGameInfo[] games)
        {
            StatusLabel.Text = "Successfully updated game list.";
            OnlineGameListData = games;
            UpdateSelectList(OnlineGameList, games);
        }

        public void UpdateLocalGameList(object sender, PublicGameInfo[] games)
        {
            LocalGameListData = games;
            UpdateSelectList(LocalGameList, games);
        }

        public void JoinSelectedGame(bool online)
        {
            if (OnlineGameListData == null || OnlineGameListData.Length < OnlineGameList.SelectedIndex+1)
                return;

            PublicGameInfo selectedGame;
            if(online)
            {
                selectedGame = OnlineGameListData[OnlineGameList.SelectedIndex];
            }
            else
            {
                selectedGame = LocalGameListData[LocalGameList.SelectedIndex];
            }
            
            IPAddress host = null;
            if(!IPAddress.TryParse(selectedGame.HostAddress, out host))
            {
                return;
            }

            var overlay = MessageOverlay.ShowWaitMessage("Connecting to Game...", onCancel: () => { Ballz.The().Network.Disconnect(); });

            Ballz.The().Network.ConnectToServer(host, selectedGame.HostPort, onSuccess: () => {
                Ballz.The().Logic.OpenMenu(new LobbyMenu(isHost: false));
                overlay.Hide();
            });
        }
    }
}
