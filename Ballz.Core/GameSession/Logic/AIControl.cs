using System;
using System.Linq;
using Ballz.GameSession.Logic.Weapons;
using Ballz.GameSession.World;
using Ballz.Utils;
using Microsoft.Xna.Framework;

namespace Ballz.GameSession.Logic
{
    public class AIControl: BallControl
    {
        public AIControl(Ballz game, Session match, Ball ball):
            base(game, match, ball)
        {
            ctrl = new Bazooka(ball,game);
        }

        Ball CurrentTarget;

        private WeaponControl ctrl;

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
                            .Where((e) =>
                                e is Ball
                                && ((Ball)e).Player != Ball.Player
                                && ((Ball)e).Health > 0)
                            .FirstOrDefault();
                    }

                    if (CurrentTarget != null)
                    {
                        Ball.IsAiming = true;
                        Ball.IsCharging = true;

                        var charg = 0.7f;
                        var g = 9.81f; // gravity
                        var v = charg * 25f; // velocity
                        var x = CurrentTarget.Position.X - Ball.Position.X; // target x
                        var y = CurrentTarget.Position.Y - Ball.Position.Y; // target y
                        var s = (v * v * v * v) - g * (g * (x * x) + 2 * y * (v * v)); //substitution
                        if (s < 0)
                        {
                            Ball.IsCharging = false;
                            Ball.IsAiming = false;
                        }
                        var o = Math.Atan2(((v * v) + Math.Sqrt(s)), (g * x)); // launch angle

                        Ball.AimDirection = Vector2.UnitX.Rotate((float) o);

                        if (Ball.ShootCharge > charg)
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
                                Velocity = Ball.AimDirection * Ball.ShootCharge * 25f,
                            };
                            Game.Match.World.AddEntity(newShot);

                            Ball.PhysicsBody.ApplyForce(-10000 * Ball.ShootCharge * newShot.Recoil * Ball.AimDirection);

                            Ball.ShootCharge = 0f;
                            ShootCooldown = PauseBetweenShots;
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
