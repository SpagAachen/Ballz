using System;
using System.Linq;
using System.Collections.Generic;

namespace Ballz.GameSession.World
{
    /// <summary>
    ///     World snapshot represents a discrete snapshot of the game world.
    ///     Thus each snapshot holds the state of Terrain, entities etc for their corresponding GameTime.
    /// </summary>
    public class World
    {
        public TimeSpan GameTime
        {
            get; protected set;
        } = new TimeSpan(0, 0, 0);

        public List<Entity> Entities
        {
            get;
            private set;
        }

        public Terrain StaticGeometry
        {
            get;
            set;
        }

        public List<Shot> Shots
        {
            get;
            private set;
        }

        public Entity EntityById(int id)
        {
            return (from e in Entities
                    where e.ID == id
                    select e).FirstOrDefault();
        }

        public World(List<Entity> newEntitites, Terrain newTerrain, List<Shot> shots = null)
        {
            Shots = shots;
            if (shots == null)
                shots = new List<Shot>();
            Entities = newEntitites;
            StaticGeometry = newTerrain;
            Shots = shots;
        }
    }
}