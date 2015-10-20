#region Using Statements
using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;

#endregion

namespace Ballz
{
	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	public class BallzGame : Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		static BallzGame instance;

		public RenderSystem rendering = new RenderSystem();
		public PhysicsControl physics = new PhysicsControl();
		public LogicControl	logic = new LogicControl();
		public InputTranslator input = new InputTranslator();
		public NetworkControl network = new NetworkControl();
		public World world;

		private BallzGame ()
		{
			graphics = new GraphicsDeviceManager (this);
			Content.RootDirectory = "Content";	            
			graphics.IsFullScreen = true;	

			//add eventhandlers to events
			input.Translate += logic.handleInputMessage;
			input.Translate += physics.handleMessage;
			input.Translate += network.handleMessage;

			logic.Message += physics.handleMessage;
			logic.Message += network.handleMessage;
			logic.Message += rendering.handleMessage;
		}

		public static BallzGame The()
		{
			if (instance != null)
				return instance;
			else 
			{
				instance = new BallzGame ();
				return instance;
			}
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize ()
		{
			// TODO: Add your initialization logic here
			base.Initialize ();
				
		}

        Texture2D TextureSplashScreen;

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent ()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch (GraphicsDevice);
            Content.RootDirectory = "Content";
			//TODO: use this.Content to load your game content here
            TextureSplashScreen = Content.Load<Texture2D>("Balls");
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update (GameTime gameTime)
		{
			// For Mobile devices, this logic will close the Game when the Back button is pressed
			// Exit() is obsolete on iOS
			#if !__IOS__
			if (GamePad.GetState (PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
			    Keyboard.GetState ().IsKeyDown (Keys.Escape)) {
				Exit ();
			}
			#endif
			// TODO: Add your update logic here	
			input.update (gameTime);
			physics.update (gameTime);
			network.update (gameTime);
			base.Update (gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw (GameTime gameTime)
		{
			graphics.GraphicsDevice.Clear (Color.CornflowerBlue);
		
            spriteBatch.Begin();
            spriteBatch.Draw(TextureSplashScreen, Window.ClientBounds, Color.White);
            spriteBatch.End();
            
			rendering.draw (gameTime);

			base.Draw (gameTime);
		}
	}
}

