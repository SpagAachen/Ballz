using Ballz.GameSession.World;
using Ballz.Messages;
using Microsoft.Xna.Framework;
using System;

namespace Ballz.GameSession.Physics
{
    /// <summary>
    ///     Physics control is called by BallzGame update to simulate discrete GamePhysics.
    /// </summary>
    public class PhysicsControl : GameComponent
    {
        Ballz Game;
        public PhysicsControl(Ballz game) : base(game)
        {
            Game = game;
        }

        public override void Update(GameTime time)
        {
            float intervalSeconds = (float)World.World.IntervalMs / 1000.0f;

            var headSnapshot = Game.World.GetHeadSnapshot();
            var headTime = Game.World.HeadTime;

            for (var remainingSeconds = time.TotalGameTime.TotalSeconds - headTime.TotalSeconds;
                remainingSeconds > 0;
                remainingSeconds -= intervalSeconds)
            {
                headSnapshot = (WorldSnapshot)headSnapshot.Clone();

                foreach (var e in headSnapshot.Entities)
                {
                    e.Position = e.Position + e.Velocity * intervalSeconds;
                    if (e.Position.Y > 5)
                        e.Velocity += new Vector2(0, -10) * intervalSeconds;
                    else
                        e.Velocity += new Vector2(0, 10) * intervalSeconds;
                }

                Game.World.AddDiscreteSnapshot(headSnapshot);
            }
        }

        public void HandleMessage(object sender, Message message)
        {
            //TODO: handle Messages
        }
    }
}