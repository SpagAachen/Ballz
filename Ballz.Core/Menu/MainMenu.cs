using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ballz.Gui;

namespace Ballz.Menu
{
    class MainMenu: MenuPanel
    {
        public MainMenu(): base("Ballz")
        {
            AddItem(new MenuButton("SinglePlayer", () => OpenMenu(new SinglePlayerMenu())));
            AddItem(new MenuButton("Multiplayer", () => OpenMenu(new MultiplayerMenu())));
            AddItem(new MenuButton("Settings", () => OpenMenu(new OptionsMenu())));
            AddItem(new MenuButton("Exit", () => Ballz.The().Quit()));

            //// multiplayer menu
            //var multiplayerMenu = new MultiplayerMenu();

            //// main menu
            //var mainMenu = new MenuPanel("Main Menu");

            //var continueLabel = new Label("Continue", true);
            //continueLabel.OnSelect += () =>
            //{
            //    Logic.ContinueGame();
            //};
            //continueLabel.Visible = false;
            //continueLabel.Selectable = false;
            //mainMenu.AddItem(continueLabel);

            //var singlePlayerMenu = new MenuPanel("Singleplayer", true);
            //{
            //    var currGameSettings = new GameSettings();
            //    // hard-coded game settings
            //    //TODO(MS): Make a "build team, game-settings whatsoever" menu and store it in currGameSettings
            //    {
            //        // Player 1
            //        {
            //            var team1 = new Team
            //            {
            //                ControlledByAI = false,
            //                Name = "Germoney",
            //                Country = "Germoney",
            //                NumberOfBallz = 2
            //            };
            //            currGameSettings.Teams.Add(team1);
            //        }
            //        // Player 2
            //        {
            //            var team2 = new Team
            //            {
            //                ControlledByAI = false,
            //                Name = "Murica",
            //                Country = "Murica",
            //                NumberOfBallz = 2
            //            };
            //            currGameSettings.Teams.Add(team2);
            //        }
            //    }

            //    // Select GameMode
            //    foreach (var factory in SessionFactory.SessionFactory.AvailableFactories)
            //    {
            //        var factoryLabel = new Label(factory.Name, true);
            //        factoryLabel.OnSelect += () =>
            //        {
            //            if (Match == null)
            //            {
            //                continueLabel.Visible = true;
            //                continueLabel.Selectable = true;
            //            }

            //            currGameSettings.GameMode = factory;
            //            Logic.StartGame(currGameSettings);
            //        };
            //        singlePlayerMenu.AddItem(factoryLabel);
            //    }
            //}

            //mainMenu.BackgroundTexture = Logo;
            //mainMenu.AddItem(singlePlayerMenu);
            //mainMenu.AddItem(multiplayerMenu);
            //mainMenu.AddItem(optionsMenu);

            //var controlsMenu = new MenuPanel("Controls", true);
            //controlsMenu.AddItem(new BackButton());
            //controlsMenu.BackgroundTexture = Content.Load<Texture2D>("Textures/Controls");
            //mainMenu.AddItem(controlsMenu);

            //var quit = new Label("Quit", true);
            //quit.OnSelect += Exit;
            //mainMenu.AddItem(quit);

            //mainMenu.SelectNext();
        }
    }
}
