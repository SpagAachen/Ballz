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

        public int ShooterId { get; set; }

        public Vector2 TargetPosition { get; set; }
        public int TargetId { get; set; } = -1;

    }
}
