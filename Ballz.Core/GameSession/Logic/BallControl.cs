using Ballz.GameSession.World;
using Ballz.Messages;
using Ballz.Utils;
using Ballz.Sound;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ballz.GameSession.Logic
{
    public abstract class BallControl
    {
        public Session Match;
        public Ballz Game;

        public Ball Ball;

        public bool IsCharging;

        public BallControl(Ballz game, Session match, Ball ball)
        {
            Game = game;
            Match = match;
            Ball = ball;
        }
        
        public virtual void Update(float elapsedSeconds, World.World worldState)
        {
            if (!IsAlive)
            {
                Ball.IsAiming = false;
            }

            if(IsCharging)
            {
                Ball.ShootCharge += elapsedSeconds * 0.7f;
                if (Ball.ShootCharge > 1f)
                    Ball.ShootCharge = 1f;
            }
            else
                Ball.ShootCharge = 0f;

            JumpCoolDown -= elapsedSeconds;
        }

        public void Shoot()
        {
            Game.Services.GetService<SoundControl>().playSound(SoundControl.shotSound);
            Game.World.Entities.Add(new Shot
            {
                ExplosionRadius = 1.0f,
                HealthImpactAtDirectHit = 25,
                IsInstantShot = false,
                Position = Ball.Position + Ball.AimDirection * (Ball.Radius + 0.101f),
                Velocity = Ball.AimDirection * Ball.ShootCharge * 30f,
            });

            Ball.ShootCharge = 0f;
        }

        public void ToggleRope()
        {
            if (Ball.AttachedRope != null)
            {
                Match.Physics.RemoveRope(Ball.AttachedRope);
                Ball.AttachedRope = null;
                Game.World.Ropes.Remove(Ball.AttachedRope);
            }
            else
            {
                var rayHit = Match.Physics.Raycast(Ball.Position, Ball.Position + Ball.AimDirection * Rope.MaxLength);
                if (rayHit.HasHit)
                {
                    var rope = new Rope
                    {
                        AttachedEntity = Ball,
                        AttachedPosition = rayHit.Position
                    };

                    Ball.AttachedRope = rope;
                    Match.Physics.AddRope(rope);
                    Game.World.Ropes.Add(rope);
                }
            }
        }

        const float PauseBetweenJumps = 0.2f;
        protected float JumpCoolDown = 0f;

        public void TryJump()
        {
            if (JumpCoolDown <= 0f)
            {
                // Send raycasts to the bottom. If it hits anything, perform the actual jump.

                for(float angle = -60f; angle < 60f; angle += 5f)
                {
                    var rayDirection = new Vector2(0, -(Ball.Radius + 0.05f));
                    rayDirection = rayDirection.Rotate(angle * (float)Math.PI / 180f);
                    var rayHit = Match.Physics.Raycast(Ball.Position, Ball.Position + rayDirection);
                    if (rayHit.HasHit)
                    {
                        Ball.Velocity = new Vector2(Ball.Velocity.X, 5f);
                        JumpCoolDown = PauseBetweenJumps;
                        break;
                    }
                }
            }
        }
        
        public virtual void HandleMessage(object sender, Message message)
        {
        }

        public bool IsAlive {  get { return Ball.Health > 0;  } }

    }
}
