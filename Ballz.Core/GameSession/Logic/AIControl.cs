using System;
using System.Linq;
using Ballz.GameSession.Logic.Weapons;
using Ballz.GameSession.World;
using Ballz.Utils;
using Microsoft.Xna.Framework;

namespace Ballz.GameSession.Logic
{
    public class AIControl : BallControl
    {
        public AIControl(Ballz game, Session match, Ball ball) :
            base(game, match, ball)
        {
            pistol = new Pistol(ball, game);
            bazoo = new Bazooka(ball, game);
        }

        Ball CurrentTarget;

        private WeaponControl bazoo;
        private WeaponControl pistol;

        float ShootCooldown;
        const float PauseBetweenShots = 2.0f;

        public override bool Update(float elapsedSeconds, World.World worldState)
        {
            base.Update(elapsedSeconds, worldState);

            if (Ball.IsAlive)
            {
                ShootCooldown -= elapsedSeconds;

                if (ShootCooldown <= 0f)
                {
                    if (CurrentTarget == null || CurrentTarget.Disposed || CurrentTarget.Health <= 0)
                    {
                        CurrentTarget = (Ball)worldState.Entities
                            .FirstOrDefault(e => e is Ball
                                && ((Ball)e).Player != Ball.Player
                                && ((Ball)e).Health > 0);
                    }

                    if (CurrentTarget != null)
                    {
                        Ball.IsAiming = false;
                        Ball.IsCharging = false;
                        float curCharge = 0.01f;
                        float o = 0;
                        while (curCharge < 0.5f)
                        {
                            var g = 9.81f; // gravity
                            var v = curCharge * 25f; // velocity
                            var x = CurrentTarget.Position.X - Ball.Position.X; // target x
                            var y = CurrentTarget.Position.Y - Ball.Position.Y; // target y
                            var s = (v * v * v * v) - g * (g * (x * x) + 2 * y * (v * v)); //substitution

                            if (s < 0)
                            {
                                curCharge += 0.01f;
                                continue;
                            }
                            o = (float)Math.Atan2(((v * v) + Math.Sqrt(s)), (g * x)); // launch angle
                            Ball.IsAiming = true;
                            Ball.IsCharging = true;
                            break;
                        }

                        if (!Ball.IsAiming)
                        {
                            Ball.HoldingWeapon = "Handgun";
                            Ball.AimDirection = Vector2.Normalize(CurrentTarget.Position - Ball.Position);
                            var muzzle = GenericGraphicsEffect.CreateMuzzle(
                                Game.Match.GameTime,
                                Ball.Position + 2f * Ball.AimDirection,
                                Ball.AimDirection.RotationFromDirection()
                                );
                            Game.Match.World.GraphicsEvents.Add(muzzle);

                            var rayHit = Game.Match.Physics.Raycast(Ball.Position, Ball.Position + Ball.AimDirection * 1000f);
                            if (rayHit.HasHit)
                            {
                                const float ExplosionRadius = 0.3f;
                                const float Damage = 25f;
                                Game.Match.World.StaticGeometry.SubtractCircle(rayHit.Position.X, rayHit.Position.Y, ExplosionRadius);
                                Ballz.The().Match.World.GraphicsEvents.Add(GenericGraphicsEffect.CreateExplosion(Ballz.The().Match.GameTime, rayHit.Position, 0, 0.2f));
                                if (rayHit.Entity != null)
                                {
                                    if (rayHit.Entity is Ball)
                                    {
                                        Ball theBall = rayHit.Entity as Ball;
                                        if (theBall.Health > 0)
                                            theBall.Health -= Damage;
                                    }
                                }
                            }
                            ShootCooldown = PauseBetweenShots;
                            return true;
                        }
                        Ball.HoldingWeapon = "Bazooka";

                        Ball.AimDirection = Vector2.UnitX.Rotate(o);

                        if (Ball.ShootCharge > curCharge)
                        {
                            Shot newShot = new Shot
                            {
                                ExplosionRadius = 3.0f,
                                HealthDecreaseFromExplosionImpact = 25,
                                HealthDecreaseFromProjectileHit = 10,
                                ShotType = Shot.ShotType_T.Normal,
                                ExplosionDelay = 0.0f,
                                Recoil = 1.0f,
                                Position = Ball.Position + Ball.AimDirection * (Ball.Radius + 0.101f),
                                Velocity = Ball.AimDirection * curCharge * 25f,
                            };
                            Game.Match.World.AddEntity(newShot);

                            Ball.PhysicsBody.ApplyForce(-10000 * Ball.ShootCharge * newShot.Recoil * Ball.AimDirection);

                            Ball.ShootCharge = 0f;
                            ShootCooldown = PauseBetweenShots;
                            return true;
                        }
                    }
                    else
                    {
                        Ball.IsAiming = false;
                    }
                }
                else
                {
                    Ball.IsCharging = false;
                }
            }

            return false;
        }
    }
}
