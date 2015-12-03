using Ballz.GameSession.World;
using Ballz.Messages;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ballz.GameSession.Logic
{
    public class GameLogic: GameComponent
    {
        private Ballz Game;

        Dictionary<Player, BallControl> BallControllers = new Dictionary<Player, BallControl>();

        public GameLogic(Ballz game):
            base(game)
        {
            Game = game;
        }

        public void AddPlayer(Player player, Ball ball)
        {
            BallControllers[player] = new BallControl(Game, Game.Match, ball);
        }

        public override void Update(GameTime time)
        {
            var elapsedSeconds = (float)time.ElapsedGameTime.TotalSeconds;

            var worldState = Game.World;

            foreach (var controller in BallControllers.Values)
                controller.Update(elapsedSeconds, worldState);
        }

        public void HandleMessage(object sender, Message message)
        {
            if (message.Kind == Message.MessageType.InputMessage)
            {
                InputMessage msg = (InputMessage)message;
                if(msg.Player != null && BallControllers.ContainsKey(msg.Player))
                {
                    BallControllers[msg.Player].HandleMessage(sender, msg);
                }
            }

            if (message.Kind == Message.MessageType.LogicMessage)
            {
                LogicMessage msg = (LogicMessage)message;

                if (msg.Kind == LogicMessage.MessageType.GameMessage)
                {
                    Enabled = !Enabled;
                }
            }
        }
    }
}
