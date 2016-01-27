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
    public enum SessionState
    {
        Starting,
        Running,
        Finished,
        Paused,
    }

    public class GameLogic: GameComponent
    {
        new private Ballz Game;

        public Dictionary<Player, BallControl> BallControllers = new Dictionary<Player, BallControl>();

        public GameLogic(Ballz game):
            base(game)
        {
            Game = game;
        }
        
        public override void Update(GameTime time)
        {
            var elapsedSeconds = (float)time.ElapsedGameTime.TotalSeconds;

            var worldState = Game.World;

            if (Game.Match.State == SessionState.Running)
            {
                // Update all balls
                var currentControllers = BallControllers.Values.ToArray();
                foreach (var controller in currentControllers)
                {
                    if (controller.Ball.Disposed)
                    {
                        BallControllers.Remove(controller.Ball.Player);
                    }
                    else
                        controller.Update(elapsedSeconds, worldState);
                }
                
                // Check for dead players
                int alivePlayers = 0;
                Player winner = null;
                foreach (var controller in BallControllers.Values)
                {
                    if (controller.IsAlive)
                    {
                        alivePlayers++;
                        winner = controller.Ball.Player;
                    }
                }

                if (alivePlayers < 2)
                {
                    Game.Match.State = SessionState.Finished;
                    Game.Match.Winner = winner;
                }
            }

        }

        public void HandleMessage(object sender, Message message)
        {
            if (message.Kind == Message.MessageType.InputMessage)
            {
                // Pass input messages to the respective ball controllers
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
                    if (Enabled && Game.Match.State != SessionState.Finished)
                        Game.Match.State = SessionState.Running;
                    else
                    {
                        Game.Match.State = SessionState.Paused;
                    }
                }
            }
        }
    }
}
