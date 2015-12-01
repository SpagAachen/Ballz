using System;
using Microsoft.Xna.Framework;

namespace Ballz.GameSession.World
{
    /// <summary>
    ///     Entity is the Base class for all types of Entities in our Game.
    /// </summary>
    public class Entity: ICloneable
    {
        private static int InstanceCounter = 0;

        public int ID
        {
            get;
            set;
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

        public Vector2 Direction
        {
            get
            {
                return new Vector2((float)Math.Sin(Rotation), (float)Math.Cos(Rotation));
            }
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

        protected Entity() { }

        public virtual object Clone()
        {
            return new Entity
            {
                ID = ID,
                Kind = Kind,
                Material = Material,
                Position = Position,
                Rotation = Rotation,
                Velocity = Velocity
            };
        }
    }
}