using System;
using Ballz.GameSession.Network;
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
        private static Ballz instance;
        public GraphicsDeviceManager graphics;

        public LogicControl logic;
        public World world;

        private Ballz()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.IsFullScreen = true;

            // create the Game Components
            var gameRendering = new GameRenderer(this);
            //initially we are in teh menuState and GameRendering needs to be disabled
            gameRendering.Enabled = false;
            gameRendering.Visible = false;
            var menuRendering = new MenuRenderer(this);
            var physics = new PhysicsControl(this);
            var input = new InputTranslator(this);
            var network = new NetworkControl(this);

            Components.Add(input);
            Components.Add(physics);
            Components.Add(network);
            Components.Add(menuRendering);
            Components.Add(gameRendering);


            logic = new LogicControl();

            //add eventhandlers to events
            Input += logic.handleInputMessage;
            Input += physics.handleMessage;
            Input += network.handleMessage;

            logic.Message += physics.handleMessage;
            logic.Message += network.handleMessage;
            logic.Message += gameRendering.handleMessage;
            logic.Message += menuRendering.handleMessage;
        }

        public static Ballz The()
        {
            if (instance != null)
                return instance;
            instance = new Ballz();
            return instance;
        }

        public event EventHandler<InputMessage> Input;

        public void onInput(InputMessage.MessageType _inputMessage)
        {
            Input?.Invoke(this, new InputMessage(_inputMessage)); //todo: use object pooling and specify message better
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
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            base.Draw(gameTime);
        }
    }
}