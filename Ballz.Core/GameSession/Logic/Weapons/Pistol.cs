using Ballz.GameSession.World;
using Ballz.Messages;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ballz.Utils;
using Ballz.Sound;

namespace Ballz.GameSession.Logic.Weapons
{
    class Pistol : WeaponControl
    {
        const float ExplosionRadius = 0.3f;
        const float Damage = 25f;
        int shotsFired = 0;

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
            if (Game.Match.IsRemoteControlled)
                return;

            if(input.Pressed && input.Kind == InputMessage.MessageType.ControlsAction)
            {
                ++shotsFired;
                Game.Services.GetService<SoundControl>().PlaySound(SoundControl.PistolSound);

                var muzzle = GenericGraphicsEffect.CreateMuzzle(
                    Game.Match.GameTime,
                    Ball.Position + 2f * Ball.AimDirection,
                    Ball.AimDirection.RotationFromDirection()
                    );
                Game.Match.World.GraphicsEvents.Add(muzzle);

                var rayHit = Game.Match.Physics.Raycast(Ball.Position, Ball.Position + Ball.AimDirection * 1000f);
                if(rayHit.HasHit)
                {
                    Game.Match.World.StaticGeometry.SubtractCircle(rayHit.Position.X, rayHit.Position.Y, ExplosionRadius);
                    Ballz.The().Match.World.GraphicsEvents.Add(GenericGraphicsEffect.CreateExplosion(Ballz.The().Match.GameTime, rayHit.Position, 0, 0.2f));
                    if (rayHit.Entity != null)
                    {
                        if(rayHit.Entity is Ball)
                        {
                            Ball theBall = rayHit.Entity as Ball;
                            if (theBall.Health > 0)
                                theBall.Health -= Damage;                            
                        }
                    }
                }
            }

            base.HandleInput(input);
        }

        public override bool Update(float elapsedSeconds, Dictionary<InputMessage.MessageType, bool> KeysPressed)
        {
            //reset the shotsFired counter after 2 shots and end the current turn.
            if (shotsFired > 0 && (shotsFired % 2) == 0)
            {
                shotsFired = 0;
                return true;
            }
            return base.Update(elapsedSeconds, KeysPressed);
        }
    }
    }
