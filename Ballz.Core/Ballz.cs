using Ballz.GameSession.World;
using Ballz.Input;
using Ballz.Logic;
using Ballz.Menu;
using Ballz.Renderer;
using Ballz.Sound;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Ballz
{
    /// <summary>
    ///     This is the main type for your game.
    /// </summary>
    public class Ballz : Game
    {
        //SpriteBatch spriteBatch;
        private static Ballz _instance;

        public GraphicsDeviceManager Graphics { get; set; }

        public LogicControl Logic { get; set; }

        public World World { get; set; }

        public Network.Network Network { get; set; }

        public GameSession.Session Match { get; set; }

        public Settings.ProgrammSettings GameSettings { get; set; }

        public Label NetworkLobbyConnectedClients { get; set; } = new Label("test", true);

        public Composite MainMenu { get; set; }

        public Camera Camera { get; set; }

        private Ballz()
        {
            Graphics = new GraphicsDeviceManager(this);
            InitSettings();
            Content.RootDirectory = "Content";
            Graphics.IsFullScreen = GameSettings.Fullscreen.Value;
            Graphics.PreferredBackBufferHeight = GameSettings.ScreenResolution.Value.Height;
            Graphics.PreferredBackBufferWidth = GameSettings.ScreenResolution.Value.Width;
            Window.AllowUserResizing = true;
            IsFixedTimeStep = false;
            
            Camera = new Camera();
            // create the Game Components
            var menuRendering = new MenuRenderer(this, DefaultMenu());
            //var physics = new PhysicsControl(this);
            var input = new InputTranslator(this);
            Network = new Network.Network(this);

            Components.Add(input);
            //Components.Add(physics);
            Components.Add(Network);
            Components.Add(menuRendering);
            Components.Add(new PerformanceRenderer(this));

            MainMenu = DefaultMenu();
            Logic = new LogicControl(this);

            Services.AddService(Logic);
            Services.AddService(input);

            Services.AddService(new SoundControl(this));

            //add eventhandlers to events
            input.Input += Logic.HandleInputMessage;
            //input.Input += physics.HandleMessage;
            input.Input += Network.HandleMessage;

            //Logic.Message += physics.HandleMessage;
            Logic.Message += Network.HandleMessage;
            //Logic.Message += gameRendering.HandleMessage;
            Logic.Message += menuRendering.HandleMessage;

            Network.Message += Logic.HandleNetworkMessage;
        }

        public static Ballz The()
        {
            if (_instance != null)
                return _instance;
            _instance = new Ballz();
            return _instance;
        }

        /// <summary>
        /// Inits the settings, by deserializing an existing Settings file.
        /// If no setingsFile is found, the default Settings are provided and serialized.
        /// </summary>
        private void InitSettings()
        {
            try
            {
                FileStream stream = new FileStream("Settings.xml", FileMode.Open);
                //found an existing Settings file try to deserialize it
                try
                {
                    LoadSettings(stream);
                    SanitizeSettings();
                }
                catch (Exception) // Loading failed so throw away the old xml
                {
                    stream.Close();
                    File.Delete("Settings.xml");
                    FileStream theStream = new FileStream("Settings.xml", FileMode.OpenOrCreate);
                    GameSettings = new Settings.ProgrammSettings();
                    StoreSettings(theStream);
                }

                stream.Close();
            }
            catch (Exception)
            {
                //no settings file was found, create one.
                FileStream theStream = new FileStream("Settings.xml", FileMode.OpenOrCreate);
                GameSettings = new Settings.ProgrammSettings();
                StoreSettings(theStream);
            }
        }

        private void SanitizeSettings()
        {
            if (!GetResolutions().Contains(GameSettings.ScreenResolution.Value))
                throw new Exception("Settings.xml holds bogus values");
        }

        private List<Settings.Resolution> GetResolutions()
        {
            List<Settings.Resolution> result = new List<Settings.Resolution>();
            DisplayModeCollection dmc = GraphicsAdapter.DefaultAdapter.SupportedDisplayModes;
            foreach (DisplayMode dm in dmc)
            {
                Settings.Resolution resolution = new Settings.Resolution(dm.Width, dm.Height);
                if (!result.Contains(resolution))
                    result.Add(resolution);
            }

            return result;
        }

        private void LoadSettings(FileStream stream)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Settings.ProgrammSettings));
            GameSettings = (Settings.ProgrammSettings)serializer.Deserialize(stream);
        }

        private void StoreSettings(FileStream stream)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Settings.ProgrammSettings));
            serializer.Serialize(stream, GameSettings);
        }

        private Composite DefaultMenu()
        {
            // options menu
            var optionsMenu = new Composite("Options", true);
            //optionsMenu.AddItem(new Label("Not Implemented", false));
            optionsMenu.AddItem(new CheckBox("FullScreen: ", GameSettings.Fullscreen));
            optionsMenu.AddItem(new Choice<Settings.Resolution>("Resolution: ", GameSettings.ScreenResolution, GetResolutions()));
            Label apply = new Label("Apply", true);
            apply.OnSelect += () =>
            {
                File.Delete("Settings.xml");
                FileStream stream = new FileStream("Settings.xml", FileMode.OpenOrCreate);
                StoreSettings(stream);
                stream.Close();
                Graphics.IsFullScreen = GameSettings.Fullscreen.Value;
                Graphics.PreferredBackBufferWidth = GameSettings.ScreenResolution.Value.Width;
                Graphics.PreferredBackBufferHeight = GameSettings.ScreenResolution.Value.Height;
                Graphics.ApplyChanges();
            };
            optionsMenu.AddItem(apply);

            // multiplayer menu
            var networkMenu = new Composite("Multiplayer", true);
            // - connect to server
            var networkConnectToMenu = new Composite("Connect to", true);
            var networkHostInput = new InputBox("Host Name: ", true);
            networkHostInput.Value = "localhost";
            networkConnectToMenu.AddItem(networkHostInput);
            var networkConnectToLabel = new Label("Connect", true);
            networkConnectToLabel.OnSelect += () => Network.ConnectToServer(networkHostInput.Value, 13337);
            networkConnectToMenu.AddItem(networkConnectToLabel);
            // - start server
            var networkServerMenu = new Composite("Start Server", true);

            networkServerMenu.OnSelect += () =>
            {
                Network.StartServer(13337); //TODO: port input
            };
            // network lobby
            networkServerMenu.AddItem(NetworkLobbyConnectedClients);
            var networkServerMenuStartGame = new Label("Start Game", true);
            networkServerMenuStartGame.OnSelect += () =>
            {
                Logic.StartGame(new SessionFactory.Worms());
                Network.GameStarted();
            };
            networkServerMenu.AddItem(networkServerMenuStartGame);
            //TODO: abort button - close server etc.

            // - add items
            networkMenu.AddItem(networkConnectToMenu);
            networkMenu.AddItem(networkServerMenu);
            networkMenu.AddItem(new Back());

            // main menu
            var mainMenu = new Composite("Main Menu");

            var continueLabel = new Label("Continue", true);
            continueLabel.OnSelect += () =>
            {
                Logic.ContinueGame();
            };
            mainMenu.AddItem(continueLabel);

            Composite startGame = new Composite("Start New Game", true);
            foreach (var factory in SessionFactory.SessionFactory.AvailableFactories)
            {
                var factoryLabel = new Label(factory.Name, true);
                factoryLabel.OnSelect += () =>
                {
                    if (Match == null)
                    {
                        continueLabel.Visible = true;
                        continueLabel.Selectable = true;
                    }

                    Logic.StartGame(factory);
                };
                startGame.AddItem(factoryLabel);
            }

            mainMenu.AddItem(startGame);
            mainMenu.AddItem(optionsMenu);
            mainMenu.AddItem(networkMenu);

            mainMenu.SelectNext();
            continueLabel.Visible = false;
            continueLabel.Selectable = false;

            var quit = new Label("Quit", true);
            quit.OnSelect += Exit;
            mainMenu.AddItem(quit);

            return mainMenu;
        }

        /// <summary>
        ///     Allows the game to perform any initialization it needs to before starting to run.
        ///     This is where it can query for any required services and load any non-graphic
        ///     related content.  Calling base.Initialize will enumerate through any components
        ///     and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        ///     LoadContent will be called once per game and is the place to load
        ///     all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            Content.RootDirectory = "Content";

            //TODO: use this.Content to load your game content here
        }

        /// <summary>
        ///     Allows the game to run logic such as updating the world,
        ///     checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        /// <summary>
        ///     This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            Graphics.GraphicsDevice.Clear(Color.White);

            base.Draw(gameTime);
        }
    }
}