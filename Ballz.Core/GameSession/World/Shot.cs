using Microsoft.Xna.Framework;
using ObjectSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ballz.GameSession.World
{
    /// <summary>
    /// Represents a shot in the 
    /// </summary>
    [Serializable]
    public class Shot : Entity
    {
        public Shot()
        {
            // The extents of the projectile
            Radius = 0.1f;
            IsStatic = false;

            if (ExplosionDelay < 0)
            {
                // Explosion countdown starts on release
                explosionCountdown = Math.Abs(ExplosionDelay);
            }
        }

        // Radius[m] of the hole that is created when the projectile collides with terrain
        [Synced]
        public float BulletHoleRadius;
        // Time [s] between hit and explosion (if negative then countdown starts after release)
        [Synced]
        public float ExplosionDelay;
        // Radius [m] of the explosion impact (if non-positive then no explosion)
        [Synced]
        public float ExplosionRadius;

        // Player health decrease [HP] when hit by projectile
        [Synced]
        public float HealthDecreaseFromProjectileHit;
        // Player health decrease [HP] when in explosion radius on detonation
        [Synced]
        public float HealthDecreaseFromExplosionImpact;

        // How much is the player forced into the opposite aim direction?
        [Synced]
        public float Recoil = 0.0f;

        // Detonation when countdown reaches zero (if negative then inactive)
        private float explosionCountdown = -1.0f;

        public enum ShotType_T
        {
            Normal,         // simulated by physics ("flies" through the air)
            InstantHit,     // instantly reaches the first collision in aim direction
            Droppable       // is dropped at the current position of the ball (e.g. mine)
        }

        // Determines how the shot is simulated
        [Synced]
        public ShotType_T ShotType { get; set; }


        public void update (float elapsedSeconds)
        {
            if (explosionCountdown > 0)
            {
                explosionCountdown -= elapsedSeconds;

                if (explosionCountdown <= 0.0f)
                {
                    Explode();
                }
            }
        }

        public void Explode()
        {
            if (ExplosionRadius <= 0.0f)
                return;
            
            System.Console.WriteLine("Explosion " + " with radius " + ExplosionRadius + " and " + HealthDecreaseFromExplosionImpact + " damage.");
            Ballz.The().Match.World.StaticGeometry.SubtractCircle(Position.X, Position.Y, ExplosionRadius);

            // TODO: damage to all players within explosion radius
            // TODO: force on players within explosion radius (dir = playerpos - Position.X/Y)

            // Remove projectile
            Dispose();
        }

        public bool DisposeOnCollision { get{ return ExplosionDelay == 0.0f;} }

        private void onAnyCollision()
        {
            // Should the explosion countdown start on collision and has not yet started?
            if (ExplosionDelay > 0.0f && explosionCountdown < 0.0f)
            {
                explosionCountdown = ExplosionDelay;
            }
                

            // Die?
            if(DisposeOnCollision)
                Explode();
        }

        public override void OnEntityCollision(Entity other)
        {
            if (Ballz.The().Match.IsRemoteControlled)
                return;
            //TODO: Player damage

            onAnyCollision();
        }

        public override void OnTerrainCollision(Terrain terrain, Vector2 position)
        {
            if (Ballz.The().Match.IsRemoteControlled)
                return;

            float impact = 0.04f * Velocity.Length() * BulletHoleRadius;
            if (impact > 0.2)
            {
                // Destroy terrain and die
                terrain.SubtractCircle(position.X, position.Y, impact);
            }

            onAnyCollision();
        }
    }
}
