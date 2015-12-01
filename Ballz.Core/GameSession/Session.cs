using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Ballz.GameSession.World;
using Ballz.Logic;

namespace Ballz.GameSession
{
    public class Session : DrawableGameComponent
    {
        private List<Entity> Entities = new List<Entity>();
        private Terrain theTerrain;
        private Physics.PhysicsControl physics;
        private Renderer.GameRenderer renderer;
        private LogicControl logic;
        private Input.InputTranslator input;
        private Ballz theGame;

        /// <summary>
        /// The entity id of the character that is currently controlled by the local player
        /// </summary>
        /// TODO: Add support for multiple local players
        public int PlayerBallId { get; set; }

        public Session(Ballz _game) : base(_game)
        {
            physics = new Physics.PhysicsControl(_game);
            physics.Enabled = false;
            _game.Components.Add(physics);

            renderer = new Renderer.GameRenderer(_game);
            renderer.Enabled = false;
            renderer.Visible = false;
            _game.Components.Add(renderer);

            logic = _game.Services.GetService<LogicControl>();
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
            if (args.GameComponent == this)  //we got removed so we get rid of all the other components
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
            theTerrain = new Terrain(Game.Content.Load<Texture2D>("Worlds/TestWorld"));

            var player = new Ball
            {
                Position = new Vector2(5, 10),
                Velocity = new Vector2(2, 0)
            };
            PlayerBallId = player.ID;
            Entities.Add(player);

            var npc = new Ball
            {
                Position = new Vector2(5, 5),
                Velocity = new Vector2(0, 0)
            };
            Entities.Add(npc);

            //System.Console.WriteLine("");

            WorldSnapshot snpsht = new WorldSnapshot(Entities, theTerrain);

            theGame.World = new World.World();
            theGame.World.AddDiscreteSnapshot(snpsht);

        }

        public override void Initialize()
        {
            base.Initialize();
        }
    }
}
