using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Ballz.GameSession.World;


namespace Ballz.GameSession
{
    public class Session : DrawableGameComponent
    {
      private List<Entity> Entities = new List<Entity>();
        private Terrain theTerrain;
        private Physics.PhysicsControl physics;
        private Renderer.GameRenderer renderer;
        private Logic.LogicControl logic;
        private Input.InputTranslator input;
      private Ballz theGame;

        public Session(Ballz _game) : base(_game)
        {
            physics = new Physics.PhysicsControl(_game);
            _game.Components.Add(physics);

            renderer = new Renderer.GameRenderer(_game);
            renderer.Enabled = false;
            renderer.Visible = false;
            _game.Components.Add(renderer);

            logic = _game.Services.GetService<Logic.LogicControl>();
            logic.Message += physics.HandleMessage;
            logic.Message += renderer.HandleMessage;

            input = _game.Services.GetService<Input.InputTranslator>();
            input.Input += physics.HandleMessage;
            input.Input += renderer.HandleMessage;

            _game.Components.ComponentRemoved += cleanup;
            //Initialize();
         theGame = _game;
        }

        public void cleanup(object sender, GameComponentCollectionEventArgs args)
        {
            if(args.GameComponent == this)  //we got removed so we get rid of all the other components
            {
                logic.Message -= physics.HandleMessage;
                logic.Message -= renderer.HandleMessage;

                Game.Components.Remove(physics);
                Game.Components.Remove(renderer);

                input.Input -= physics.HandleMessage;
                input.Input -= renderer.HandleMessage;
            }
        }

      protected override void LoadContent()
      {
         ///generate a dummy game world
         /// TODO: find a nice solution to initialize the world especially regarding networking. maybe use an event for this
         theTerrain = new Terrain();
         theTerrain.outline = new List<Vector2>();
         //try to load the testworld

         Entities.Add(new Player());
         int x=0;
         int y=0;
         int width = 0;

            //System.Console.WriteLine("");
            Texture2D testWorld = Game.Content.Load<Texture2D>("Worlds/TestWorld");
            Boolean[,] values = new Boolean[testWorld.Width,testWorld.Height];
            width = testWorld.Width;
            //int x = 0;
            //int y = 0;
            Color[] theColors = new Color[testWorld.Width * testWorld.Height];
            testWorld.GetData<Color>(theColors);

            foreach(Color aColor in theColors)
            {
               
               if(aColor.ToVector3().LengthSquared() > 0)
               {
                  values[x,y] = true;
                  //System.Console.Out.Write("#");
               }
               else
               {
                  //System.Console.Out.Write("0");
               }
               if(++x >= width)
               {
                  //System.Console.WriteLine("");
                  x = 0;
                  ++y;
               }

            }
            Physics2DDotNet.Shapes.ArrayBitmap ab = new Physics2DDotNet.Shapes.ArrayBitmap(values);
            AdvanceMath.Vector2D[] geometry = Physics2DDotNet.Shapes.VertexHelper.CreateFromBitmap(ab);
            foreach(AdvanceMath.Vector2D vec in geometry)
            {
               theTerrain.outline.Add(new Vector2(vec.X,vec.Y));
            }
            WorldSnapshot snpsht = new WorldSnapshot(Entities,theTerrain);

            theGame.World = new World.World();
            theGame.World.AddDiscreteSnapshot(snpsht);
         
            //use Physics2d to get terrain stuff from the bitmap
         //TODO: use kerstens code as soon as possible

      }
         
      public override void Initialize()
      {
         base.Initialize();
      }
    }
}
