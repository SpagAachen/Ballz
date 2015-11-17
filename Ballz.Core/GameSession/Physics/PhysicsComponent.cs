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
                    e.Velocity += new Vector2(0, -10) * intervalSeconds;

                    if (e.Position.Y < 0.5)
                    {
                        e.Velocity *= new Vector2(1, -0.95f);
                        e.Position = new Vector2(e.Position.X, 0.5f);
                    }
                    if (e.Position.X < 0.5)
                    {
                        e.Velocity *= new Vector2(-0.95f, 1);
                        e.Position = new Vector2(0.5f, e.Position.Y);
                    }
                    if (e.Position.X > 9.5)
                    {
                        e.Velocity *= new Vector2(-0.95f, 1);
                        e.Position = new Vector2(9.5f, e.Position.Y);
                    }
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