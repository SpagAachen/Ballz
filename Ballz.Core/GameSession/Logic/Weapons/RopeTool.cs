using Ballz.GameSession.World;
using Ballz.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ballz.GameSession.Logic.Weapons
{
    public class RopeTool: WeaponControl
    {
        public RopeTool(Ball ball, Ballz game) : base(ball, game) { }

        public override string Icon { get; } = "RopeTool";

        public override string Name { get; } = "Rope";

        public override void HandleInput(InputMessage input)
        {
            base.HandleInput(input);
            
            if(input.Pressed)
            {
                switch(input.Kind)
                {
                    case InputMessage.MessageType.ControlsAction:
                        // Toggle Rope
                        if (Ball.AttachedRope != null)
                        {
                            Game.Match.Physics.RemoveRope(Ball.AttachedRope);
                            Ball.AttachedRope = null;
                            Game.Match.World.Ropes.Remove(Ball.AttachedRope);
                        }
                        else
                        {
                            var rayHit = Game.Match.Physics.Raycast(Ball.Position, Ball.Position + Ball.AimDirection * Rope.MaxLength);
                            if (rayHit.HasHit)
                            {
                                var rope = new Rope
                                {
                                    AttachedEntity = Ball,
                                    AttachedPosition = rayHit.Position
                                };

                                Ball.AttachedRope = rope;
                                Game.Match.Physics.AddRope(rope);
                                Game.Match.World.Ropes.Add(rope);
                            }
                        }

                        break;
                    case InputMessage.MessageType.ControlsUp:
                        // Make rope shorter
                        if (Ball.AttachedRope != null)
                            Game.Match.Physics.ShortenRope(Ball.AttachedRope);
                        break;
                    case InputMessage.MessageType.ControlsDown:
                        // Make rope longer
                        if (Ball.AttachedRope != null)
                            Game.Match.Physics.LoosenRope(Ball.AttachedRope);
                        break;
                }
            }
        }
    }
}
