using Ballz.GameSession.Logic;
using Microsoft.Xna.Framework;
using System;

namespace Ballz.GameSession.World
{
    /// <summary>
    /// Represents a Ball character. 
    /// </summary>
    [Serializable]
    public class Ball : Entity
    {
        public Ball()
        {
            Material = PhysicsMaterial.Ball;
            Radius = 0.8f;
        }

        /// <summary>
        /// The health value of the ball. Typical value ranges are 0-100.
        /// </summary>
        public double Health { get; set; } = 100;

        public bool IsAlive { get { return Health > 0; } }

        public bool IsAiming { get; set; } = false;

        public bool IsCharging { get; set; } = false;

        /// <summary>
        /// Indicates the "charging level" that is used to control the initial velocity of certain projectiles.
        /// </summary>
        public float ShootCharge { get; set; } = 0f;

        public Player Player { get; set; }

        public Vector2 AimDirection { get; set; } = Vector2.UnitX;

        public string HoldingWeapon;

        public Rope AttachedRope = null;

        public override void OnEntityCollision(Entity other)
        {
            var shot = other as Shot;
            if (shot != null)
            {
                float impact = shot.Velocity.Length() * shot.ExplosionRadius;
                if (impact < 5)
                    return;

                //TODO(ks) more elaborate damage model
                Health -= shot.HealthImpactAtDirectHit * Math.Min(1, impact / 20);
                if (Health < 0)
                    Health = 0;

                PhysicsBody.ApplyLinearImpulse(10 * shot.Velocity);
            }
            else
            {
                // Other balls, etc -> no-op
            }
        }

        public override void OnTerrainCollision(Terrain terrain, Vector2 position)
        {
            base.OnTerrainCollision(terrain, position);
        }
    }
}