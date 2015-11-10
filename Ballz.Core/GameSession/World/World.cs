using System;
using System.Collections.Generic;
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
        readonly List<WorldSnapshot> snapshots = new List<WorldSnapshot>();
        TimeSpan headTime = new TimeSpan(0,0,0);
        private const double IntervalMs = 16;
        private const int SnapshotCount = 6;

        public World()
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        ///     Interpolates between discrete timestep Snapshots, and returns the interpolated Snapshot.
        /// </summary>
        /// <param name="time">The time that should be used for interpolation (absolute time since start of game).</param>
        public WorldSnapshot GetSnapshot(GameTime time)
        {
            if (snapshots.Count > 1)
            {
                var msSinceTime = headTime.Subtract(time.ElapsedGameTime).TotalMilliseconds;
                var timeInShots = snapshots.Count - 1 - msSinceTime/IntervalMs;
                var preIndex = (int) Math.Floor(timeInShots);
                var postIndex = preIndex + 1;
                var alpha = (float) (timeInShots - preIndex);
                var preSnapshot = snapshots[preIndex];
                var postSnapshot = snapshots[postIndex];
                var interpolated = new List<Entity>();
                foreach (var pre in preSnapshot.Entities)
                {
                    var post = postSnapshot.Entities.Find(e => e.ID == pre.ID);
                    if (post == null)
                        interpolated.Add(pre);
                    else
                        interpolated.Add(new Entity(pre.Kind)
                        {
                            ID = pre.ID,
                            Material = pre.Material,
                            Position = pre.Position*(1-alpha) + post.Position*alpha,
                            Rotation = pre.Rotation*(1-alpha) + post.Rotation*alpha,
                            Velocity = pre.Velocity*(1-alpha) + post.Velocity*alpha,
                        });
                }
                return new WorldSnapshot(interpolated, preSnapshot.StaticGeometry);
            }
            else
                return snapshots[0];
        }

        public WorldSnapshot GetDiscreteSnapshot(GameTime time)
        {
            var msSinceTime = headTime.Subtract(time.ElapsedGameTime).TotalMilliseconds;
            var timeInShots = snapshots.Count - 1 - msSinceTime / IntervalMs;
            var preIndex = (int)Math.Floor(timeInShots);
            return snapshots[preIndex];
        }

        public void AddDiscreteSnapshot(WorldSnapshot snpsht)
        {
            snapshots.Add(snpsht);
            if (snapshots.Count > SnapshotCount)
                snapshots.RemoveAt(0);
            //the simulation time is for now 16 ms;
            headTime = headTime.Add(new TimeSpan(0,0,0,0,16));
        }
    }
}