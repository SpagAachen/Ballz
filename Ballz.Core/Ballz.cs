using System;
using Ballz.GameSession.Physics;
using Ballz.GameSession.Renderer;
using Ballz.GameSession.World;
using Ballz.Input;
using Ballz.Logic;
using Ballz.Messages;
using Ballz.Renderer;
using Microsoft.Xna.Framework;

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

        private Ballz()
        {
            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Graphics.IsFullScreen = true;

            // create the Game Components
            var gameRendering = new GameRenderer(this)
            {
                Enabled = false,
                Visible = false
            };
            //initially we are in teh menuState and GameRendering needs to be disabled
            var menuRendering = new MenuRenderer(this);
            var physics = new PhysicsControl(this);
            var input = new InputTranslator(this);
            var network = new Network.Network(this);

            Components.Add(input);
            Components.Add(physics);
            Components.Add(network);
            Components.Add(menuRendering);
            Components.Add(gameRendering);


            Logic = new LogicControl();

            Services.AddService<LogicControl>(Logic);
            Services.AddService<InputTranslator>(input);
            //add eventhandlers to events
            input.Input += Logic.HandleInputMessage;
            input.Input += physics.HandleMessage;
            input.Input += network.HandleMessage;

            Logic.Message += physics.HandleMessage;
            Logic.Message += network.HandleMessage;
            Logic.Message += gameRendering.HandleMessage;
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