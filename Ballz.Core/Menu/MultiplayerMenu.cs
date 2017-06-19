using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ballz.Gui;

namespace Ballz
{
    class MultiplayerMenu: Gui.MenuPanel
    {
        public MultiplayerMenu() : base("Multiplayer")
        {
            AddItem(new MenuButton("Join Game", ()=>OpenMenu(new GameListMenu())));
            AddItem(new MenuButton("Join by IP", ()=>OpenMenu(new JoinByIpMenu())));
            AddItem(new MenuButton("Host Game", () => OpenMenu(new HostGameMenu(true))));
            AddItem(new BackButton());

            //// - connect to server
            //var networkConnectToMenu = new Composite("Connect to", true);
            //var networkHostInput = new InputBox("Host Name: ", true);
            //networkHostInput.Value = "localhost";
            //networkConnectToMenu.AddItem(networkHostInput);
            //var networkConnectToLabel = new Label("Connect", true);
            ////networkConnectToLabel.OnSelect += () => Network.ConnectToServer(networkHostInput.Value, 13337);
            //networkConnectToMenu.AddItem(networkConnectToLabel);
            //// - start server
            //var networkServerMenu = new Composite("Start Server", true);

            //networkServerMenu.OnSelect += () =>
            //{
            //    //Network.StartServer(13337); //TODO: port input
            //};
            //// network lobby
            //networkServerMenu.AddItem(NetworkLobbyConnectedClients);
            //var networkServerMenuStartGame = new Label("Start Game", true);
            //networkServerMenuStartGame.OnSelect += () =>
            //{
            //    var currGameSettings = new GameSettings
            //    {
            //        //TODO: Let server select game mode
            //        GameMode =
            //                                       SessionFactory.SessionFactory.AvailableFactories
            //                                       .ElementAt(0)
            //    };
            //    //Network.StartNetworkGame(currGameSettings);
            //};
            //networkServerMenu.AddItem(networkServerMenuStartGame);
            //TODO: abort button - close server etc.

            //// - add items
            //AddItem(networkConnectToMenu);
            //AddItem(networkServerMenu);
            //AddItem(new Back());
        }
    }
}
