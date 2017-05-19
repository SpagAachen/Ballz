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
            AddItem(new MenuButton("Options", () => OpenMenu(new OptionsMenu())));
            AddItem(new MenuButton("Exit", () => Ballz.The().Quit()));

            //// options menu
            //var optionsMenu = new MenuPanel("Options", true);
            ////optionsMenu.AddItem(new Label("Not Implemented", false));
            //optionsMenu.AddItem(new CheckBox("FullScreen: ", GameSettings.Fullscreen));
            //optionsMenu.AddItem(new Choice<Settings.Resolution>("Resolution: ", GameSettings.ScreenResolution, GetResolutions()));
            //optionsMenu.AddItem(new SpinBox("Multisampling: ", GameSettings.MSAASamples, 1, 16));
            //optionsMenu.AddItem(new SpinBox("MasterVolume: ", GameSettings.MasterVolume, 0, 100));
            //InputBox ipb = new InputBox("PlayerName: ", true);
            //ipb.Setting = GameSettings.PlayerName;
            //ipb.internalValue = false;
            //optionsMenu.AddItem(ipb);
            //optionsMenu.AddItem(new CheckBox("Friendly Fire: ", GameSettings.FriendlyFire, true));
            //Label apply = new Label("Apply", true);
            //apply.OnSelect += () =>
            //{
            //    File.Delete("Settings.xml");
            //    FileStream stream = new FileStream("Settings.xml", FileMode.OpenOrCreate);
            //    StoreSettings(stream);
            //    stream.Close();
            //    Graphics.IsFullScreen = GameSettings.Fullscreen.Value;
            //    Graphics.PreferredBackBufferWidth = GameSettings.ScreenResolution.Value.Width;
            //    Graphics.PreferredBackBufferHeight = GameSettings.ScreenResolution.Value.Height;
            //    //might be redundant as MSAA seems only to be changed before game is created (restart required)
            //    if (GameSettings.MSAASamples.Value > 1)
            //    {
            //        Graphics.PreferMultiSampling = true;
            //        Graphics.PreparingDeviceSettings -= msaaSettingsHandler;
            //        msaaSettingsHandler = (object sender, PreparingDeviceSettingsEventArgs args) => {
            //            args.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = GameSettings.MSAASamples.Value;
            //        };
            //        Graphics.PreparingDeviceSettings += msaaSettingsHandler;
            //    }
            //    Graphics.ApplyChanges();
            //};
            //optionsMenu.AddItem(apply);

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
