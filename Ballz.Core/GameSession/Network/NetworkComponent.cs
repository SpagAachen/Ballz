using Ballz.Messages;
using Microsoft.Xna.Framework;

namespace Ballz.GameSession.Network
{
    /// <summary>
    /// Network component takes care of all network related stuff for a game session.
    /// </summary>
    public class NetworkControl : GameComponent
    {
        public NetworkControl(Game game) : base(game)
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