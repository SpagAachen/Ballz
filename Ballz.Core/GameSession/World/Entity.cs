using System;
using Microsoft.Xna.Framework;

namespace Ballz.GameSession.World
{
    /// <summary>
    ///     Entity is the Base class for all types of Entities in our Game.
    /// </summary>
    public class Entity
    {
        private static int InstanceCounter = 0;

        public int ID
        {
            get;
            private set;
        }
        public enum EntityType
        {
            Player
        }

        public EntityType Kind
        {
            get;
            private set;
        }

        public Vector2 Position
        {
            get;
            set;
        }

        public Vector2 Velocity
        {
            get;
            set;
        }

        public float Rotation
        {
            get;
            set;
        }

        public PhysicsMaterial Material
        {
            get;
            set;
        }


        public Entity(EntityType kind)
        {
            Kind = kind;
            ID = InstanceCounter++;
        }
    }
}