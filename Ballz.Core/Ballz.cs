using System;
using System.IO;
using System.Xml.Serialization;

using Ballz.GameSession.Physics;
using Ballz.GameSession.Renderer;
using Ballz.GameSession.World;
using Ballz.Input;
using Ballz.Logic;
using Ballz.Messages;
using Ballz.Renderer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Ballz.Menu;

namespace Ballz
{
    #region Using Statements

    

    #endregion

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

        public GameSession.Session Match;

        public Settings.ProgrammSettings Settings;

        public Menu.Composite MainMenu;

        private Ballz()
        {
            Graphics = new GraphicsDeviceManager(this);
            initSettings();
            Content.RootDirectory = "Content";
            Graphics.IsFullScreen = Settings.Fullscreen;
            Graphics.PreferredBackBufferHeight = Settings.ScreenHeight;
            Graphics.PreferredBackBufferWidth = Settings.ScreenWidth;


            // create the Game Components
            var menuRendering = new MenuRenderer(this);
            //var physics = new PhysicsControl(this);
            var input = new InputTranslator(this);
            var network = new Network.Network(this);

            Components.Add(input);
            //Components.Add(physics);
            Components.Add(network);
            Components.Add(menuRendering);
            Components.Add(new PerformanceRenderer(this));

            MainMenu = DefaultMenu();
            Logic = new LogicControl(this);

            Services.AddService<LogicControl>(Logic);
            Services.AddService<InputTranslator>(input);

            Match = new GameSession.Session(this);
            Components.Add(Match);

            //add eventhandlers to events
            input.Input += Logic.HandleInputMessage;
            //input.Input += physics.HandleMessage;
            input.Input += network.HandleMessage;

            //Logic.Message += physics.HandleMessage;
            Logic.Message += network.HandleMessage;
            //Logic.Message += gameRendering.HandleMessage;
            Logic.Message += menuRendering.HandleMessage;
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
        private void initSettings()
        {
            try
            {
                System.IO.FileStream stream = new System.IO.FileStream("Settings.xml", FileMode.Open);
                //found an existing Settings file try to deserialize it
                loadSettings(stream);
                stream.Close();
            }
            catch (Exception)
            {
                //no settings file was found, create one.
                System.IO.FileStream stream = new System.IO.FileStream("Settings.xml", FileMode.OpenOrCreate);
                Settings = new Settings.ProgrammSettings();
                storeSettings(stream);
                stream.Close();
            }           
        }

        private void loadSettings(FileStream stream)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Settings.ProgrammSettings));
            Settings = (Settings.ProgrammSettings)serializer.Deserialize(stream);
        }

        private void storeSettings(FileStream stream)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Settings.ProgrammSettings));
            serializer.Serialize(stream, Settings);
        }

        private Composite DefaultMenu()
        {
            // options menu
            var optionsMenu = new Composite("Options", true);
            //optionsMenu.AddItem(new Label("Not Implemented", false));
            optionsMenu.AddItem(new CheckBox("FullScreen: ", Settings));
            Label apply = new Label("Apply", true);
            apply.OnSelect += () => 
                {
                    System.IO.FileStream stream = new System.IO.FileStream("Settings.xml", FileMode.OpenOrCreate);
                    storeSettings(stream);
                    stream.Close();
                    Graphics.IsFullScreen = Settings.Fullscreen;
                    Graphics.ApplyChanges();
                };
            optionsMenu.AddItem(apply);

            // multiplayer menu
            var networkMenu = new Composite("Multiplayer", true);
            // - connect to server
            var networkConnectToMenu = new Composite("Connect to", true);
            networkConnectToMenu.AddItem(new InputBox("Host Name: ", true));
            // - start server
            var networkServerMenu = new Composite("Start server", true);
            networkServerMenu.AddItem(new Label("Not Implemented", false));
            // - add items
            networkMenu.AddItem(networkConnectToMenu);
            networkMenu.AddItem(networkServerMenu);
            networkMenu.AddItem(new Back());

            // main menu
            var mainMenu = new Composite("Main Menu");

            var play = new Label("Play",true);
            play.OnSelect += () =>
                {
                    Logic.startGame();
                };

            mainMenu.AddItem(play);
            mainMenu.AddItem(optionsMenu);
            mainMenu.AddItem(networkMenu);

            var quit = new Label("Quit", true);
            quit.OnSelect += () => Exit();
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
            Graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            base.Draw(gameTime);
        }
    }
}