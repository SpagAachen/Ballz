using System;
using Ballz.GameSession.World;
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
        /// <param name="time">The time that should be used for interpolation (absolute time since start of game).</param>
        public WorldSnapshot GetSnapshot(GameTime time)
        {
            throw new NotImplementedException();
        }

        public WorldSnapshot GetDiscreteSnapshot(GameTime time)
        {
            throw new NotImplementedException();
        }

        public void AddDiscreteSnapshot(WorldSnapshot snpsht)
        {
            throw new NotImplementedException();
        }
    }
}