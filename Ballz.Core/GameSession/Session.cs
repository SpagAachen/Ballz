using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Ballz.GameSession.World;

namespace Ballz.GameSession
{
    class Session : GameComponent
    {
        private List<Entity> Entities;
        private Physics.PhysicsControl physics;
        private Renderer.GameRenderer renderer;

        Session(Ballz _game) : base(_game)
        {
            physics = new Physics.PhysicsControl(_game);
            renderer = new Renderer.GameRenderer(_game);
            _game.Components.Add( physics);
            _game.Components.Add(renderer);

            _game.Input += physics.HandleMessage;
            _game.Input += renderer.HandleMessage;

           
        }
        public override void Initialize()
        {
            Entities.Add(new Player());
            base.Initialize();
        }
    }
}
