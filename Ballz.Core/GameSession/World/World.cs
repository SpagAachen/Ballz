using System;
using Ballz.GameSession.world;
using Microsoft.Xna.Framework;

namespace Ballz.GameSession.World
{
    /// <summary>
    ///     The World manages an array of discrete Snapshots of the World.
    ///     It provides access for all other modules to these snapshots in the required way.
    ///     (e.g. the rendering system needs interpolated snapshots, while physics computes snapshots for discrete timesteps)
    /// </summary>
    public class World
    {
        public World()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Interpolates between discrete timestep Snapshots, and returns the interpolated Snapshot.
        /// </summary>
        /// <param name="_time">The time that should be used for interpolation (absolute time since start of game).</param>
        public WorldSnapshot getSnapshot(GameTime _time)
        {
            throw new NotImplementedException();
        }

        public WorldSnapshot getDiscreteSnapshot(GameTime _time)
        {
            throw new NotImplementedException();
        }

        public void addDiscreteSnapshot(WorldSnapshot _snpsht)
        {
            throw new NotImplementedException();
        }
    }
}