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
        private Renderer.GameRenderer gameRenderer;
        private Renderer.DebugRenderer debugRenderer;
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

            gameRenderer = new Renderer.GameRenderer(_game);
            gameRenderer.Enabled = false;
            gameRenderer.Visible = false;
            _game.Components.Add(gameRenderer);

            debugRenderer = new Renderer.DebugRenderer(_game);
            debugRenderer.Enabled = false;
            debugRenderer.Visible = false;
            _game.Components.Add(debugRenderer);

            sessionLogic = new Logic.GameLogic(_game);
            sessionLogic.Enabled = false;
            _game.Components.Add(sessionLogic);

            logic = _game.Services.GetService<LogicControl>();
            logic.Message += physics.HandleMessage;
            logic.Message += gameRenderer.HandleMessage;
            logic.Message += sessionLogic.HandleMessage;
            logic.Message += debugRenderer.HandleMessage;

            input = _game.Services.GetService<Input.InputTranslator>();
           

            _game.Components.ComponentRemoved += cleanup;
            //Initialize();
            theGame = _game;
        }

        public void start(bool random)
        {
            generateWorld(random);
            input.Input += physics.HandleMessage;
            input.Input += gameRenderer.HandleMessage;
            input.Input += sessionLogic.HandleMessage;
            input.Input += debugRenderer.HandleMessage;
        }

        private void generateWorld(bool random)
        {
            if (random)
            {
                theTerrain=Terrain.mountainTerrain(GraphicsDevice);
            }
            else
            {
                theTerrain = new Terrain(Game.Content.Load<Texture2D>("Worlds/TestWorld2"));
            }

            var player1 = new Player
                {
                    Name = "Player1"
                };
            Players.Add(player1);

            var player1Ball = new Ball
                {
                    Position = new Vector2(4, 10),
                    Velocity = new Vector2(2, 0),
                    IsAiming = true,
                    Player = player1,
                    HoldingWeapon = "Bazooka",
                };
            Entities.Add(player1Ball);

            sessionLogic.BallControllers[player1] = new UserControl(theGame, this, player1Ball);

            var player2 = new Player
                {
                    Name = "Player2"
                };
            Players.Add(player2);

            var player2Ball = new Ball
                {
                    Position = new Vector2(27, 7),
                    Velocity = new Vector2(2, 0),
                    IsAiming = true,
                    Player = player2,
                    HoldingWeapon = "HandGun",
            };
            Entities.Add(player2Ball);

            sessionLogic.BallControllers[player2] = new UserControl(theGame, this, player2Ball);

            var playerAi = new Player
            {
                Name = "NPC"
            };
            Players.Add(playerAi);

            var aiBall = new Ball
            {
                Position = new Vector2(8, 10),
                Velocity = new Vector2(0, 0),
                Player = playerAi
            };
            Entities.Add(aiBall);
            sessionLogic.BallControllers[playerAi] = new AIControl(theGame, this, aiBall);

            //System.Console.WriteLine("");

            World.World snpsht = new World.World(Entities, theTerrain);
            theGame.World = snpsht;
        }

        public void cleanup(object sender, GameComponentCollectionEventArgs args)
        {
            if (args.GameComponent == this)  //we got removed so we get rid of all the other components
            {
                logic.Message -= physics.HandleMessage;
                logic.Message -= gameRenderer.HandleMessage;
                logic.Message -= debugRenderer.HandleMessage;

                Game.Components.Remove(physics);
                Game.Components.Remove(gameRenderer);
                Game.Components.Remove(sessionLogic);

                input.Input -= physics.HandleMessage;
                input.Input -= gameRenderer.HandleMessage;
            }
        }
            
        protected override void LoadContent()
        {         
            //maybe use paused
            State = SessionState.Starting;

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
