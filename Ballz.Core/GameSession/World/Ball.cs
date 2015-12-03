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
        public Ball() : base(EntityType.Player)
        {
            Material = PhysicsMaterial.Ball;
        }

        /// <summary>
        /// The health value of the ball. Typical value ranges are 0-100.
        /// </summary>
        public double Health { get; set; } = 100;

        public bool IsAiming { get; set; } = false;

        public Player Player { get; set; } = Player.NPC;
        
        public Vector2 AimDirection { get; set; } = Vector2.UnitX;
    }
}