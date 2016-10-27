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
        int ShotsFired = 0;

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

        public override void OnTurnEnd()
        {
            base.OnTurnEnd();
            ShotsFired = 0;
        }

        public override void HandleInput(InputMessage input)
        {
            if (Game.Match.IsRemoteControlled)
                return;

            if(input.Pressed && input.Kind == InputMessage.MessageType.ControlsAction)
            {
                ++ShotsFired;
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

        public override void Update(float elapsedSeconds, Dictionary<InputMessage.MessageType, bool> KeysPressed, out bool turnEndindActionHappened, out bool canSwitchWeapon)
        {
            base.Update(elapsedSeconds, KeysPressed, out turnEndindActionHappened, out canSwitchWeapon);

            // Weapon switching is only allowed if the pistol has not been used yet
            canSwitchWeapon = (ShotsFired == 0) || !Game.Match.UsePlayerTurns;

            // Turn ends after two shots
            turnEndindActionHappened = ShotsFired >= 2;
        }
    }
    }
