using System;
using System.Collections.Generic;

namespace Ballz.GameSession.World
{
    /// <summary>
    ///     World snapshot represents a discrete snapshot of the game world.
    ///     Thus each snapshot holds the state of Terrain, entities etc for their corresponding GameTime.
    /// </summary>
    public class WorldSnapshot
    {
      public List<Entity> Entities {
         get;
         private set;
      }
      public Terrain StaticGeometry
      {
         get;
         set;
      }
      public WorldSnapshot(List<Entity> newEntitites, Terrain newTerrain)
        {
            Entities = newEntitites;
            StaticGeometry = newTerrain;
        }
    }
}