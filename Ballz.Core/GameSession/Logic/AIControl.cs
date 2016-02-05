using Ballz.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ballz.GameSession.World;
using Microsoft.Xna.Framework;
using Ballz.Sound;
using Ballz.Utils;

namespace Ballz.GameSession.Logic
{
    public class AIControl: BallControl
    {
        public AIControl(Ballz game, Session match, Ball ball):
            base(game, match, ball)
        {

        }

        Ball CurrentTarget;

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
                        Ball.IsAiming = true;

                        Ball.AimDirection = Vector2.Normalize(CurrentTarget.Position - Ball.Position);

                        if (Ball.ShootCharge > 0.9f)
                        {
                            //Shoot();
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
