using Ballz.GameSession.Logic;
using Ballz.GameSession.World;
using Ballz.Logic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ballz.GameSession
{
    public class Session: IDisposable
    {
        public List<Entity> Entities { get; set; } = new List<Entity>();

        public Terrain Terrain { get; set; }

        public Physics.PhysicsControl Physics { get; set; }

        public Logic.GameLogic SessionLogic { get; set; }

        public Renderer.GameRenderer GameRenderer { get; set; }

        public Renderer.DebugRenderer DebugRenderer { get; set; }

        public LogicControl Logic { get; set; }

        public Input.InputTranslator Input { get; set; }

        public Ballz Game { get; set; }

        public List<Player> Players { get; set; } = new List<Player>();

        public Player Winner { get; set; } = null;

        public SessionState State { get; set; } = SessionState.Starting;

        public bool UsePlayerTurns { get; set; } = false;

        public Player ActivePlayer { get; set; }

        public const float SecondsPerTurn = 60f;

        public float TurnTimeLeft { get; set; } = SecondsPerTurn;

        public GameSession.Logic.GameSettings GameSettings { get; set; }

        public Session(Ballz _game, GameSession.Logic.GameSettings settings)
        {
            GameSettings = settings;

            Physics = new Physics.PhysicsControl(_game);
            Physics.Enabled = false;
            _game.Components.Add(Physics);

            GameRenderer = new Renderer.GameRenderer(_game);
            GameRenderer.Enabled = false;
            GameRenderer.Visible = false;
            _game.Components.Add(GameRenderer);

            DebugRenderer = new Renderer.DebugRenderer(_game);
            DebugRenderer.Enabled = false;
            DebugRenderer.Visible = false;
            _game.Components.Add(DebugRenderer);

            SessionLogic = new Logic.GameLogic(_game, this);
            SessionLogic.Enabled = false;
            _game.Components.Add(SessionLogic);

            Logic = _game.Services.GetService<LogicControl>();
            Logic.Message += Physics.HandleMessage;
            Logic.Message += GameRenderer.HandleMessage;
            Logic.Message += SessionLogic.HandleMessage;
            Logic.Message += DebugRenderer.HandleMessage;

            Input = _game.Services.GetService<Input.InputTranslator>();
            
            Game = _game;
        }

        public void Start()
        {
            Input.Input += Physics.HandleMessage;
            Input.Input += GameRenderer.HandleMessage;
            Input.Input += SessionLogic.HandleMessage;
            Input.Input += DebugRenderer.HandleMessage;
            State = SessionState.Running;
        }

        public Player PlayerByNumber(int number)
        {
            if (Players.Count < number)
                return null;
            return Players[number - 1];
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Logic.Message -= Physics.HandleMessage;
                    Logic.Message -= GameRenderer.HandleMessage;
                    Logic.Message -= DebugRenderer.HandleMessage;

                    Game.Components.Remove(Physics);
                    Game.Components.Remove(GameRenderer);
                    Game.Components.Remove(SessionLogic);

                    Input.Input -= Physics.HandleMessage;
                    Input.Input -= GameRenderer.HandleMessage;
                }
                
                disposedValue = true;
            }
        }

        ~Session()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
