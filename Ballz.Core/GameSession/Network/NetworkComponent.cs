using Ballz.Messages;
using Microsoft.Xna.Framework;

namespace Ballz.GameSession.Network
{
    /// <summary>
    ///     Network control Takes care of all network related stuff.
    /// </summary>
    public class NetworkControl : GameComponent
    {
        public NetworkControl(Game _game) : base(_game)
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