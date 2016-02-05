using Ballz.GameSession.World;
using Ballz.Messages;
using Ballz.Sound;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ballz.GameSession.Logic
{
    public abstract class WeaponControl
    {
        public WeaponControl(Ball ball, Ballz game)
        {
            Ball = ball;
            Game = game;
        }

        protected Ballz Game;

        public Ball Ball;

        public abstract string Name { get; }

        public abstract string Icon { get; }

        public void FireProjectile()
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

        /// <summary>
        /// Updates the weapon state and performs weapon actions.
        /// </summary>
        /// <returns>Returns true if the ball has made an action that finishes a player turn.</returns>
        public virtual bool Update(float elapsedSeconds, Dictionary<InputMessage.MessageType, bool> KeysPressed) { return false; }

        public virtual void HandleInput(InputMessage input) { }
    }
}
