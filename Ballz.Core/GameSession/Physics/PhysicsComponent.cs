using Ballz.Messages;
using Microsoft.Xna.Framework;

namespace Ballz.GameSession.Physics
{
    /// <summary>
    ///     Physics control is called by BallzGame update to simulate discrete GamePhysics.
    /// </summary>
    public class PhysicsControl : GameComponent
    {
        public PhysicsControl(Game game) : base(game)
        {
        }

        public override void Update(GameTime time)
        {
        }

        public void HandleMessage(object sender, Message message)
        {
            //TODO: handle Messages
        }
    }
}