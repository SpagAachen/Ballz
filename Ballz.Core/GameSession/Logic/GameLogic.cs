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

        public Session Session;
        
        public GameLogic(Ballz game, Session session) :
            base(game)
        {
            Game = game;
            Session = session;
        }
        
        public void NextTurn()
        {
            if (Session.UsePlayerTurns)
            {
                Session.ActivePlayer = Session.Players[(Session.Players.IndexOf(Session.ActivePlayer) + 1) % Session.Players.Count];
                Session.TurnTimeLeft = Session.SecondsPerTurn;
            }
        }

        public override void Update(GameTime time)
        {
            var elapsedSeconds = (float)time.ElapsedGameTime.TotalSeconds;
            if (Session.UsePlayerTurns)
            {
                if (Session.ActivePlayer == null)
                {
                    Session.ActivePlayer = Session.Players[0];
                }

                Session.TurnTimeLeft -= elapsedSeconds;
                if (Session.TurnTimeLeft < 0f)
                {
                    NextTurn();
                }
            }

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
                    else if(controller.Ball.Player == Session.ActivePlayer || !Session.UsePlayerTurns)
                    {
                        var playerMadeAction = controller.Update(elapsedSeconds, worldState);
                        if (playerMadeAction)
                            NextTurn();
                    }
                }
                
                // Check for dead players
                int alivePlayers = 0;
                Player winner = null;
                foreach (var controller in BallControllers.Values)
                {
                    if (controller.Ball.IsAlive)
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

                // When using turn mode in hot-seat, direct all input messages to the active player
                if (Session.UsePlayerTurns && Session.ActivePlayer != null)
                {
                    BallControllers[Session.ActivePlayer].HandleMessage(sender, msg);
                }
                // Otherwise, redirect input messages to the player given by msg.Player
                else if(msg.Player != null && BallControllers.ContainsKey(msg.Player))
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
