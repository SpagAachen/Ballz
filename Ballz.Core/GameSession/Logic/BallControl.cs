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
        }

        public void Shoot()
        {
            Game.Services.GetService<SoundControl>().playSound(SoundControl.shotSound);
            Game.World.Entities.Add(new Shot
            {
                ExplosionRadius = 1.0f,
                HealthImpactAtDirectHit = 25,
                IsInstantShot = false,
                Position = Ball.Position + Ball.AimDirection * (Ball.Radius + 0.2f),
                Velocity = Ball.AimDirection * Ball.ShootCharge * 30f,
            });

            Ball.ShootCharge = 0f;
        }

        public virtual void HandleMessage(object sender, Message message)
        {
        }

        public bool IsAlive {  get { return Ball.Health > 0;  } }

    }
}
