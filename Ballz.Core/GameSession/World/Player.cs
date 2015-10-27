using System;

namespace Ballz.GameSession.World
{
    /// <summary>
    ///     Player is the concrete player (aka Ball).
    /// </summary>
    public class Player : Entity
    {
        public Player() : base(EntityType.Player)
        {
            Material = PhysicsMaterial.Ball;
        }
    }
}