using System;
using Microsoft.Xna.Framework;
using ObjectSync;

namespace Ballz.GameSession.World
{
    /// <summary>
    ///     Entity is the Base class for all types of Entities in our Game.
    /// </summary>
    [Serializable]
    public class Entity : IDisposable
    {
        public int ID
        {
            get;
            set;
        } = -1;

        [Synced]
        public Vector2 Position
        {
            get;
            set;
        }

        [Synced]
        public Vector2 Velocity
        {
            get;
            set;
        }

        [Synced]
        public float Rotation
        {
            get;
            set;
        }

        [Synced]
        public float ViewRotation
        {
            get;
            set;
        }

        [Newtonsoft.Json.JsonIgnore]
        PhysicsMaterial _material;

        [Newtonsoft.Json.JsonIgnore]
        public PhysicsMaterial Material
        {
            get
            {
                return _material;
            }
            set
            {
                _material = value;
            }
        }

        [Synced]
        public float Radius { get; set; } = 0.5f;

        [Synced]
        public bool IsStatic { get; set; } = false;

        [Newtonsoft.Json.JsonIgnore]
        FarseerPhysics.Dynamics.Body _physicsBody;

        [Newtonsoft.Json.JsonIgnore]
        public FarseerPhysics.Dynamics.Body PhysicsBody
        {
            get
            {
                return _physicsBody;
            }
            set
            {
                _physicsBody = value;
            }
        }

        public Entity()
        {
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

        public virtual void OnTerrainCollision(Terrain terrain, Vector2 position)
        {
        }

        public virtual void OnEntityCollision(Entity other)
        {
        }
    }
}