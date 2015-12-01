using System;
using System.Collections.Generic;

namespace Ballz.GameSession.World
{
    /// <summary>
    ///     World snapshot represents a discrete snapshot of the game world.
    ///     Thus each snapshot holds the state of Terrain, entities etc for their corresponding GameTime.
    /// </summary>
    public class WorldSnapshot : ICloneable
    {
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

        public WorldSnapshot(List<Entity> newEntitites, Terrain newTerrain, List<Shot> shots = null)
        {
            Shots = shots;
            if (shots == null)
                shots = new List<Shot>();
            Entities = newEntitites;
            StaticGeometry = newTerrain;
            Shots = shots;
        }

        public object Clone()
        {
            var newEntities = new List<Entity>();
            foreach (var oldEntity in Entities)
            {
                newEntities.Add((Entity)oldEntity.Clone());
            }

            var shots = new List<Shot>();
            foreach (var oldShot in Shots)
            {
                shots.Add((Shot)oldShot.Clone());
            }

            //TODO: Clone Terrain
            return new WorldSnapshot(newEntities, StaticGeometry, shots);
        }
        
    }
}