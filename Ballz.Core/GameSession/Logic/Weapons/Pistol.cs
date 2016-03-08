using Ballz.GameSession.World;
using Ballz.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ballz.GameSession.Logic.Weapons
{
    class Pistol : WeaponControl
    {
        const float ExplosionRadius = 0.3f;
        const float Damage = 25f;

        public Pistol(Ball ball, Ballz game) : base(ball, game) { }

        public override string Icon
        {
            get
            {
                return "HandGun";
            }
        }

        public override string Name
        {
            get
            {
                return "Pistol";
            }
        }

        public override void HandleInput(InputMessage input)
        {
            if((input.Pressed ?? false) && input.Kind == InputMessage.MessageType.ControlsAction)
            {
                var rayHit = Game.Match.Physics.Raycast(Ball.Position, Ball.Position + Ball.AimDirection * 1000f);
                if(rayHit.HasHit)
                {
                    Game.World.StaticGeometry.SubtractCircle(rayHit.Position.X, rayHit.Position.Y, ExplosionRadius);
                    if(rayHit.Entity != null)
                    {
                        if(rayHit.Entity is Ball)
                        {
                            (rayHit.Entity as Ball).Health -= Damage;
                        }
                    }
                }
            }

            base.HandleInput(input);
        }
    }
    }
