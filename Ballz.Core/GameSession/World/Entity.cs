using System;
using Microsoft.Xna.Framework;
using ObjectSync;

using Ballz.Network;
using System.IO;
using Lidgren.Network;

namespace Ballz.GameSession.World
{
    /// <summary>
    ///     Entity is the Base class for all types of Entities in our Game.
    /// </summary>
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

        public virtual void Serialize(NetOutgoingMessage writer)
        {
            writer.Write(ID);
            writer.Write(Position.X);
            writer.Write(Position.Y);
            writer.Write(Velocity.X);
            writer.Write(Velocity.Y);
            writer.Write(Rotation);
            writer.Write(ViewRotation);
            writer.Write(Radius);
            writer.Write(IsStatic);
        }

        public virtual void Deserialize(NetIncomingMessage data)
        {
            ID = data.ReadInt32();
            Position = new Vector2(data.ReadSingle(), data.ReadSingle());
            Velocity = new Vector2(data.ReadSingle(), data.ReadSingle());
            Rotation = (float)data.ReadSingle();
            ViewRotation = data.ReadSingle();
            Radius = data.ReadSingle();
            IsStatic = data.ReadBoolean();
        }

        public static SynchronizingInfo GetSyncInfo()
        {
            return new SynchronizingInfo
            {
                ObjectConstructor = () => new Entity(),
                IdToObject = (id) => Ballz.The().Match?.World.EntityById((int)id),
                ObjectToId = (e) => (e as Entity).ID,
            };
        }
    }
}