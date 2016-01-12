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

        public bool IsAiming { get; set; } = false;

        /// <summary>
        /// Indicates the "charging level" that is used to control the initial velocity of certain projectiles.
        /// </summary>
        public float ShootCharge { get; set; } = 0f;

        public Player Player { get; set; } = Player.NPC;
        
        public Vector2 AimDirection { get; set; } = Vector2.UnitX;

        public string HoldingWeapon;

    }
}