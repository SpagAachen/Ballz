using Ballz.Messages;
using Microsoft.Xna.Framework;

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
            var snapshot = Game.World.GetSnapshot(time);

            var elapsedSeconds = (float)time.ElapsedGameTime.TotalSeconds;

            foreach(var e in snapshot.Entities)
            {
                e.Position = e.Position + e.Velocity * elapsedSeconds;
                if(e.Position.Y > 5)
                    e.Velocity += new Vector2(0, -10) * elapsedSeconds;
                else
                    e.Velocity += new Vector2(0, 10) * elapsedSeconds;
            }
        }

        public void HandleMessage(object sender, Message message)
        {
            //TODO: handle Messages
        }
    }
}