using Microsoft.Xna.Framework;
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
    public class Shot : Entity
    {
        public Shot()
        {
            Radius = 0.1f;
            IsStatic = false;
        }

        /// <summary>
        /// Explosion radius of the shot impact.
        /// </summary>
        /// <remarks>
        /// If this value is greater than zero, a sphere of the given size is substracted from the terrain.
        /// </remarks>
        public float ExplosionRadius;

        /// <summary>
        /// The health impact if the shot directly hits a different Ball.
        /// </summary>
        /// <remarks>
        /// If a ball is not hit directly, the damage is linearly interpolated based on the ExplosionRadius and the distance to the ball.
        /// </remarks>
        public float HealthImpactAtDirectHit;
        
        /// <summary>
        /// If this value is true, the shot velocity is ignored and the projectile will hit its target instantly.
        /// </summary>
        public bool IsInstantShot { get; set; }
        
        public bool DisposeOnCollision { get; set; } = true;

        public override void OnEntityCollision(Entity other)
        {
            // Die?
            if(DisposeOnCollision)
                Dispose();
        }

        public override void OnTerrainCollision(Terrain terrain, Vector2 position)
        {
            float impact = Velocity.Length() * ExplosionRadius;
            if (impact > 5)
            {
                // Destroy terrain and die
                terrain.SubtractCircle(position.X, position.Y, 0.04f * impact);
            }

            if(DisposeOnCollision)
                Dispose();
        }
    }
}
