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

        PublicGameInfo[] GameListData = null;
        SelectList GameList = new SelectList(new Vector2(0, 350));
        Button JoinButton = new Gui.MenuButton("Join", anchor: Anchor.BottomRight, size: new Vector2(0.45f, -1));

        public GameListMenu() : base("Join Online Game")
        {
            Size = new Vector2(800, 0);
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

            AddItem(StatusLabel);
            AddItem(GameList);
            var panel = new Panel(new Vector2(0, 150));

            JoinButton.OnClick += (e) => JoinSelectedGame();
            panel.AddChild(JoinButton);
            panel.AddChild(new Gui.BackButton(Anchor.BottomLeft, new Vector2(0.45f, -1)));
            AddItem(panel);
        }

        string SelectedGameId = "";

        public override void Update()
        {
            base.Update();
            Lobby?.Update();
        }

        public void UpdateGameList(object sender, PublicGameInfo[] games)
        {
            StatusLabel.Text = "Successfully updated game list.";

            var selection = GameList.SelectedValue;
            var scrollPos = GameList.ScrollPosition;
            var isFocussed = GameList.IsFocused;

            GameListData = games;

            GameList.ClearItems();
            if (games.Length == 0)
            {
                GameList.AddItem("[No games found]");
            }
            else
            {
                foreach (PublicGameInfo g in games)
                {
                    GameList.AddItem(g.Name);
                }
            }

            try
            {
                GameList.ScrollPosition = scrollPos;
                if (isFocussed)
                {
                    GeonBit.UI.UserInterface.ActiveEntity = GameList;
                }
                GameList.SelectedValue = selection;
            }
            catch(System.Exception e)
            {
                GameList.SelectedIndex = 0;
            }
        }

        public void JoinSelectedGame()
        {
            if (GameListData == null || GameListData.Length < GameList.SelectedIndex+1)
                return;

            PublicGameInfo selectedGame = GameListData[GameList.SelectedIndex];

            var overlay = MessageOverlay.ShowWaitMessage("Connecting to Game...", onCancel: () => { Ballz.The().Network.Disconnect(); });

            IPAddress host = null;
            if(!IPAddress.TryParse(selectedGame.HostAddress, out host))
            {
                return;
            }

            Ballz.The().Network.ConnectToServer(host, selectedGame.HostPort, onSuccess: () => {
                Ballz.The().Logic.OpenMenu(new LobbyMenu(isHost: false));
                overlay.Hide();
            });
        }
    }
}
