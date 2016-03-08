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

        public List<Entity> Entities
        {
            get;
        }

        public List<Rope> Ropes { get; private set; } = new List<Rope>();

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

        public World(List<Entity> newEntitites, Terrain newTerrain)
        {
            Entities = newEntitites;
            StaticGeometry = newTerrain;
            Water = new Water(50, 25);
        }
    }
}