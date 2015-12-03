using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Ballz.GameSession.World;
using Ballz.Logic;
using Ballz.GameSession.Logic;

namespace Ballz.GameSession
{
    public class Session : DrawableGameComponent
    {
        private List<Entity> Entities = new List<Entity>();
        private Terrain theTerrain;
        private Physics.PhysicsControl physics;
        private Logic.GameLogic sessionLogic;
        private Renderer.GameRenderer renderer;
        private LogicControl logic;
        private Input.InputTranslator input;
        private Ballz theGame;

        public List<Player> Players { get; set; } = new List<Player>();
        public Player Winner { get; set; } = null;
        public SessionState State { get; set; } = SessionState.Starting;

        public Session(Ballz _game) : base(_game)
        {
            physics = new Physics.PhysicsControl(_game);
            physics.Enabled = false;
            _game.Components.Add(physics);

            renderer = new Renderer.GameRenderer(_game);
            renderer.Enabled = false;
            renderer.Visible = false;
            _game.Components.Add(renderer);

            sessionLogic = new Logic.GameLogic(_game);
            sessionLogic.Enabled = false;
            _game.Components.Add(sessionLogic);

            logic = _game.Services.GetService<LogicControl>();
            logic.Message += physics.HandleMessage;
            logic.Message += renderer.HandleMessage;
            logic.Message += sessionLogic.HandleMessage;

            input = _game.Services.GetService<Input.InputTranslator>();
            input.Input += physics.HandleMessage;
            input.Input += renderer.HandleMessage;
            input.Input += sessionLogic.HandleMessage;

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

            var player1 = new Player
            {
                Name = "Player1"
            };
            Players.Add(player1);

            var player1Ball = new Ball
            {
                Position = new Vector2(5, 10),
                Velocity = new Vector2(2, 0),
                IsAiming = true,
                Player = player1
            };
            Entities.Add(player1Ball);

            sessionLogic.AddPlayer(player1, player1Ball);

            var player2 = new Player
            {
                Name = "Player2"
            };
            Players.Add(player2);

            var player2Ball = new Ball
            {
                Position = new Vector2(5, 12),
                Velocity = new Vector2(2, 0),
                IsAiming = true,
                Player = player2
            };
            Entities.Add(player2Ball);

            sessionLogic.AddPlayer(player2, player2Ball);

            var npc = new Ball
            {
                Position = new Vector2(8, 10),
                Velocity = new Vector2(0, 0)
            };
            Entities.Add(npc);

            //System.Console.WriteLine("");

            World.World snpsht = new World.World(Entities, theTerrain);

            theGame.World = snpsht;

            State = SessionState.Running;

        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public Player PlayerByNumber(int number)
        {
            if (Players.Count < number)
                return null;
            return Players[number - 1];
        }
    }
}
