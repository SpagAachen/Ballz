using Ballz.Messages;
using Microsoft.Xna.Framework;

namespace Ballz.GameSession.Physics
{
    /// <summary>
    ///     Physics control is called by BallzGame update to simulate discrete GamePhysics.
    /// </summary>
    public class PhysicsControl : GameComponent
    {
        public PhysicsControl(Game _game) : base(_game)
        {
        }

        public void update(GameTime _time)
        {
        }

        public void handleMessage(object _sender, Message _message)
        {
            //TODO: handle Messages
        }
    }
}