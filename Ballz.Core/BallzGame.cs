namespace Ballz
{
   #region Using Statements
   using System;

   using Microsoft.Xna.Framework;
   using Microsoft.Xna.Framework.Graphics;
   using Microsoft.Xna.Framework.Storage;
   using Microsoft.Xna.Framework.Input;

   #endregion


   /// <summary>
   /// This is the main type for your game.
   /// </summary>
   public class BallzGame : Game
   {
      public GraphicsDeviceManager graphics;
      //SpriteBatch spriteBatch;
      static BallzGame instance;

      public LogicControl	logic;
      public World world;

      private BallzGame ()
      {
			
         graphics = new GraphicsDeviceManager (this);
         Content.RootDirectory = "Content";	            
         graphics.IsFullScreen = true;	

         // create the Game Components
         GameRenderer gameRendering = new GameRenderer (this);
         MenuRenderer menuRendering = new MenuRenderer (this);
         PhysicsControl physics = new PhysicsControl (this);
         InputTranslator input = new InputTranslator (this);
         NetworkControl network = new NetworkControl (this);

         Components.Add (input);
         Components.Add (physics);
         Components.Add (network);
         Components.Add (menuRendering);
         Components.Add (gameRendering);


         logic = new LogicControl ();

         //add eventhandlers to events
         Input += logic.handleInputMessage;
         Input += physics.handleMessage;
         Input += network.handleMessage;
         Input += logic.handleInputMessage;

         logic.Message += physics.handleMessage;
         logic.Message += network.handleMessage;
         logic.Message += gameRendering.handleMessage;
      }

      public static BallzGame The ()
      {
         if (instance != null)
            return instance;
         else {
            instance = new BallzGame ();
            return instance;
         }
      }

      public event EventHandler<Message> Input;

      public void onInput (Message.MessageType _inputMessage)
      {
         if (Input != null)
            Input (this, new Message (_inputMessage)); //todo: use object pooling and specify message better
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

      /// <summary>
      /// LoadContent will be called once per game and is the place to load
      /// all of your content.
      /// </summary>
      protected override void LoadContent ()
      {
         Content.RootDirectory = "Content";
         //TODO: use this.Content to load your game content here
      }

      /// <summary>
      /// Allows the game to run logic such as updating the world,
      /// checking for collisions, gathering input, and playing audio.
      /// </summary>
      /// <param name="gameTime">Provides a snapshot of timing values.</param>
      protected override void Update (GameTime gameTime)
      {
         base.Update (gameTime);
      }

      /// <summary>
      /// This is called when the game should draw itself.
      /// </summary>
      /// <param name="gameTime">Provides a snapshot of timing values.</param>
      protected override void Draw (GameTime gameTime)
      {
         graphics.GraphicsDevice.Clear (Color.CornflowerBlue);

         base.Draw (gameTime);
      }
   }
}

