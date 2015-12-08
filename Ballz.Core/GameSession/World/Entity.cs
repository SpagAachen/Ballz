using System;
using Microsoft.Xna.Framework;

namespace Ballz.GameSession.World
{
    /// <summary>
    ///     Entity is the Base class for all types of Entities in our Game.
    /// </summary>
    public class Entity: IDisposable
    {
        private static int InstanceCounter = 1;

		public int ID
        {
            get;
            set;
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

        public float Radius { get; set; } = 0.5f;

        public bool IsStatic { get; set; } = false;

        public FarseerPhysics.Dynamics.Body PhysicsBody { get; set; }

        public Entity()
        {
            ID = InstanceCounter++;
        }

        public virtual object Clone()
        {
            return new Entity
            {
                ID = ID,
                Material = Material,
                Position = Position,
                Rotation = Rotation,
                Velocity = Velocity
            };
        }

        public bool Disposed { get; private set; } = false;

        public void Dispose()
        {
            Disposed = true;
        }
    }
}