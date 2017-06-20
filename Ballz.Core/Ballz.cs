using Ballz.GameSession.World;
using Ballz.Input;
using Ballz.Logic;
using Ballz.Gui;
using Ballz.Renderer;
using Ballz.Sound;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Ballz.GameSession.Logic;

namespace Ballz
{
    using Microsoft.Xna.Framework.Input;
    using Settings;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    ///     This is the main type for your game.
    /// </summary>
    public class Ballz : Game
    {
        //SpriteBatch spriteBatch;
        static Ballz _instance;

        public List<string> Teamnames { get; private set; }

        public GraphicsDeviceManager Graphics { get; set; }

        public LogicControl Logic { get; set; }

        public InputTranslator Input { get; set; }

        public Network.Network Network { get; set; }

        public GameSession.Session Match { get; set; }

        public Settings.GameSettings Settings { get; set; }

        GuiRenderer GuiRenderer;

        public MenuPanel MainMenu { get; set; }
        
        public MessageOverlay MessageOverlay { get; set; }

        public Camera Camera { get; set; }
        private EventHandler<PreparingDeviceSettingsEventArgs> msaaSettingsHandler;

        Texture2D Logo;

        private Ballz()
        {
            Teamnames = new List<string>();
            Graphics = new GraphicsDeviceManager(this);
            InitSettings();
            Content.RootDirectory = "Content";
            if (Settings.MSAASamples > 1)
            {
                Graphics.PreferMultiSampling = true;
                msaaSettingsHandler = (object sender, PreparingDeviceSettingsEventArgs args) =>
                {
                    args.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = Settings.MSAASamples;
                };
                Graphics.PreparingDeviceSettings += msaaSettingsHandler;
            }
            Graphics.IsFullScreen = Settings.Fullscreen;
            Graphics.PreferredBackBufferHeight = Settings.ScreenResolution.Height;
            Graphics.PreferredBackBufferWidth = Settings.ScreenResolution.Width;
            Window.AllowUserResizing = true;
            IsFixedTimeStep = false;

            Camera = new Camera();
            // create the Game Components
            GuiRenderer = new GuiRenderer(this);
            //var physics = new PhysicsControl(this);
            Input = new InputTranslator(this);
            Network = new Network.Network(this);

            Components.Add(Input);
            //Components.Add(physics);
            Components.Add(Network);
            Components.Add(GuiRenderer);
            Components.Add(new PerformanceRenderer(this));

            Logic = new LogicControl(this);

            Services.AddService(Logic);
            Services.AddService(Input);

            Services.AddService(new SoundControl(this));

            //add eventhandlers to events
            Input.Input += Logic.HandleInputMessage;
            //input.Input += physics.HandleMessage;
            Input.Input += Network.HandleMessage;

            //Logic.Message += physics.HandleMessage;
            Logic.Message += Network.HandleMessage;
            //Logic.Message += gameRendering.HandleMessage;
            Logic.Message += GuiRenderer.HandleMessage;

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
            Settings = GameSettings.Load();
        }

        private void SanitizeSettings()
        {
            if (Settings.Fullscreen && !GetResolutions().Contains(Settings.ScreenResolution))
            {
                Settings.ScreenResolution = GetResolutions().First();
                StoreSettings();
                Console.WriteLine("Settings.xml holds unsupported resolution, resetting to one that should work");
            }
        }

        public List<Settings.Resolution> GetResolutions()
        {
            List<Settings.Resolution> result = new List<Settings.Resolution>();
            DisplayModeCollection dmc = GraphicsAdapter.DefaultAdapter.SupportedDisplayModes;
            foreach (DisplayMode dm in dmc)
            {
                Settings.Resolution resolution = new Settings.Resolution(dm.Width, dm.Height);
                if (!result.Contains(resolution) && resolution.Width >= 800 && resolution.Height >= 600)
                    result.Add(resolution);
            }

            return result;
        }

        public void LoadSettings()
        {
            Settings = GameSettings.Load();
        }

        public void StoreSettings()
        {
            Settings.Save();
        }

        private MenuPanel DefaultMenu()
        {
            // Music!
            Ballz.The().Services.GetService<SoundControl>().StartMusic(SoundControl.MenuMusic);
            return new Menu.MainMenu();
        }

        void loadTeamnames()
        {
            string path = Content.RootDirectory + "/Textures/Teams";
            DirectoryInfo d = new DirectoryInfo(path);
            FileInfo[] Files = d.GetFiles(); //Getting Text files
            //string str = "";
            foreach (FileInfo file in Files)
            {
                //str = str + ", " + file.Name;
                var teamName = Path.GetFileNameWithoutExtension(file.Name);
                if (!Teamnames.Contains(teamName))
                    Teamnames.Add(teamName);
            }
        }

        public bool LockMouse { get; set; } = false;
        public Vector2 MouseAimDirection { get; protected set; } = Vector2.Zero;

        /// <summary>
        ///     Allows the game to perform any initialization it needs to before starting to run.
        ///     This is where it can query for any required services and load any non-graphic
        ///     related content.  Calling base.Initialize will enumerate through any components
        ///     and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            loadTeamnames();
            base.Initialize();
        }

        /// <summary>
        ///     LoadContent will be called once per game and is the place to load
        ///     all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            Content.RootDirectory = "Content";

            Logo = Content.Load<Texture2D>("Textures/Logo");
            MainMenu = DefaultMenu();
            Logic.SetMainMenu(MainMenu);
        }

        /// <summary>
        ///     Allows the game to run logic such as updating the world,
        ///     checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if(LockMouse)
            {
                var pos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
                var center = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
                var maxMouseDistance = 70;

                var d = pos - center;
                var len = d.Length();

                var clampedLen = Math.Min(len, maxMouseDistance);
                if(clampedLen > 3f)
                {
                    d.Normalize();
                    MouseAimDirection = d * new Vector2(1, -1);
                    d *= clampedLen;
                }
                else
                {
                    MouseAimDirection = Vector2.Zero;
                }
                pos = center + d;
                
                
                Mouse.SetPosition((int)Math.Round(pos.X), (int)Math.Round(pos.Y));
            }

            Logic.Update(gameTime);
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

        public void Quit()
        {
            OnExiting(this, new EventArgs());
        }

        public event EventHandler Exiting;

        protected override void OnExiting(object sender, EventArgs args)
        {
            Exiting?.Invoke(this, new EventArgs());
            base.OnExiting(sender, args);
            Process.GetCurrentProcess().Kill();
        }
    }
}