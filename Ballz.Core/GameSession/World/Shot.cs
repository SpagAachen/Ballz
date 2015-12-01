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
    public class Shot : ICloneable
    {
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
        /// The position where the shot projectile was fired.
        /// </summary>
        public Vector2 ShotStart { get; set; }

        /// <summary>
        /// The travel direction and speed of the shot projectile.
        /// </summary>
        /// <remarks>
        /// If IsInstantShot is true, only the direction matters, the length of this vector is ignored.
        /// </remarks>
        public Vector2 ShotVelocity { get; set; }

        /// <summary>
        /// If this value is true, the shot velocity is ignored and the projectile will hit its target instantly.
        /// </summary>
        public bool IsInstantShot { get; set; }
        
        public object Clone()
        {
            return new Shot
            {
                ExplosionRadius = ExplosionRadius,
                HealthImpactAtDirectHit = HealthImpactAtDirectHit,
                IsInstantShot = IsInstantShot,
                ShotStart = ShotStart,
                ShotVelocity = ShotVelocity,
            };
        }
    }
}
