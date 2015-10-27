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
        private Logic.LogicControl logic;
        private Input.InputTranslator input;

        Session(Ballz _game) : base(_game)
        {
            physics = new Physics.PhysicsControl(_game);
            renderer = new Renderer.GameRenderer(_game);
            _game.Components.Add( physics);
            _game.Components.Add(renderer);

            logic = _game.Services.GetService<Logic.LogicControl>();
            logic.Message += physics.HandleMessage;
            logic.Message += renderer.HandleMessage;

            input = _game.Services.GetService<Input.InputTranslator>();
            input.Input += physics.HandleMessage;
            input.Input += renderer.HandleMessage;

            _game.Components.ComponentRemoved += cleanup;
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

        public override void Initialize()
        {
            Entities.Add(new Player());
            base.Initialize();
        }
    }
}
