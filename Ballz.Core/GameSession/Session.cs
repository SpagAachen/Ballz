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
    public class Session: IDisposable
    {
        public List<Entity> Entities = new List<Entity>();
        public Terrain Terrain;
        public Physics.PhysicsControl Physics;
        public Logic.GameLogic SessionLogic;
        public Renderer.GameRenderer GameRenderer;
        public Renderer.DebugRenderer DebugRenderer;
        public LogicControl Logic;
        public Input.InputTranslator Input;
        public Ballz Game;

        public List<Player> Players { get; set; } = new List<Player>();
        public Player Winner { get; set; } = null;
        public SessionState State { get; set; } = SessionState.Starting;

        public Session(Ballz _game)
        {
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

            SessionLogic = new Logic.GameLogic(_game);
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
