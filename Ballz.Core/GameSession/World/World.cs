using System;
using System.Collections.Generic;
using Ballz.GameSession.Physics;
using System.Linq;

namespace Ballz.GameSession.World
{
    /// <summary>
    ///     World snapshot represents a discrete snapshot of the game world.
    ///     Thus each snapshot holds the state of Terrain, entities etc for their corresponding GameTime.
    /// </summary>
    public class World
    {
        public TimeSpan GameTime { get; protected set; } = new TimeSpan(0, 0, 0);

        public event EventHandler<Entity> EntityAdded;
        public event EventHandler<Entity> EntityRemoved;

        List<Entity> EntityList = new List<Entity>();

        public IEnumerable<Entity> Entities
        {
            get { return EntityList; }
        }

        public List<Rope> Ropes { get; private set; } = new List<Rope>();

        public List<GraphicsEvent> GraphicsEvents = new List<GraphicsEvent>();

        public Terrain StaticGeometry
        {
            get;
            set;
        }

        public Water Water { get; }
        
        public Entity EntityById(int id)
        {
            return (from e in Entities
                    where e.ID == id
                    select e).FirstOrDefault();
        }

        private int EntityIdCounter = 0;
        public void AddEntity(Entity e)
        {
            e.ID = EntityIdCounter++;
            EntityAdded?.Invoke(this, e);
            EntityList.Add(e);
        }

        public void RemoveEntity(Entity e)
        {
            EntityRemoved?.Invoke(this, e);
            EntityList.Remove(e);
        }

        public World(Terrain newTerrain)
        {
            StaticGeometry = newTerrain;
            Water = new Water(50,25);
        }
    }
}