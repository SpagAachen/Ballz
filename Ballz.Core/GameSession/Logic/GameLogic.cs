using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ballz.GameSession.World;
using Ballz.Messages;
using Microsoft.Xna.Framework;

namespace Ballz.GameSession.Logic
{
    public class GameLogic: GameComponent
    {
        new Ballz Game;

        public Dictionary<Player, BallControl> ActiveControllers { get; set; } = new Dictionary<Player, BallControl>();
        public Dictionary<Ball, BallControl> BallControllers { get; set; } = new Dictionary<Ball, BallControl>();

        public Session Session { get; set; }
        
        public GameLogic(Ballz game, Session session) :
            base(game)
        {
            Game = game;
            Session = session;
        }
        
        public void StartNextTurn()
        {
            if (Session.UsePlayerTurns)
            {
                var player = Session.Players[(Session.Players.IndexOf(Session.ActivePlayer) + 1) % Session.Players.Count];
                Session.ActivePlayer = player;
                
                player.ActiveBall = ChooseNextBall(player);
                ActiveControllers[player] = BallControllers[player.ActiveBall];
                ActiveControllers[player]?.OnTurnStart();
                Session.TurnTime = 0;
                Session.TurnState = TurnState.Running;
            }
        }

        public void EndTurn()
        {
            if (Session.UsePlayerTurns)
            {
                ActiveControllers[Session.ActivePlayer]?.OnTurnEnd();
                Session.TurnTime = 0;
                Session.TurnState = TurnState.WaitingForEnd;
            }
        }

        public Ball ChooseNextBall(Player player)
        {
            return player.OwnedBalls[(player.OwnedBalls.IndexOf(player.ActiveBall) + 1) % player.OwnedBalls.Count];
        }

        public override void Update(GameTime time)
        {
            var elapsedSeconds = (float)time.ElapsedGameTime.TotalSeconds;

            Session.GameTime += elapsedSeconds;

            // Remove finished graphics events
            var graphicsEffects = Session.World.GraphicsEvents.Where(e => e.End < Session.GameTime).ToArray();
            foreach (var e in graphicsEffects)
                Session.World.GraphicsEvents.Remove(e);

            if (Session.UsePlayerTurns)
            {
                if (Session.ActivePlayer == null)
                {
                    Session.ActivePlayer = Session.Players[0];
                }

                Session.TurnTime += elapsedSeconds;
                if (Session.TurnTime > Session.SecondsPerTurn && Session.TurnState == TurnState.Running)
                {
                    EndTurn();
                }

                // End the turn if the turn is over and MinSecondsBetweenTurns have passed.
                // Wait for all entities to stand still, but at most MaxSecondsBetweenTurns.
                if (Session.TurnState == TurnState.WaitingForEnd
                    && (Session.TurnTime > Session.MaxSecondsBetweenTurns
                        || (Session.TurnTime > Session.MinSecondsBetweenTurns && !Session.World.IsSomethingMoving ))
                    )
                {
                    StartNextTurn();
                }
            }

            var worldState = Session.World;

            if (Game.Match.State == SessionState.Running)
            {
                // Update all active balls
                var currentControllers = ActiveControllers.Values.ToArray();
                foreach (var controller in currentControllers)
                {
                    if(controller != null && (controller.Ball.Player == Session.ActivePlayer || !Session.UsePlayerTurns))
                    {
                        var playerMadeAction = controller.Update(elapsedSeconds, worldState);
                        if (playerMadeAction)
                            EndTurn();
                    }
                }
                
                // Check for dead balls
                List<Player> alivePlayers = new List<Player>();
                Player winner = null;
                var ballControllers = BallControllers.Values.ToArray();
                foreach (var controller in ballControllers)
                {
                    if (controller.Ball.Disposed || !controller.Ball.IsAlive)
                    {
                        controller.Ball.Player.OwnedBalls.Remove(controller.Ball);

                        if (controller.Ball.Player == Session.ActivePlayer)
                        {
                            StartNextTurn();
                        }

                        ActiveControllers.Remove(controller.Ball.Player);
                        BallControllers.Remove(controller.Ball);
                    }
                    else if (controller.Ball.IsAlive)
                    {
                        if (!alivePlayers.Contains(controller.Ball.Player))
                            alivePlayers.Add(controller.Ball.Player);
                        winner = controller.Ball.Player;
                    }
                }

                if (alivePlayers.Count < 2)
                {
                    Game.Match.State = SessionState.Finished;
                    if(alivePlayers.Count == 1)
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
                if (Session.UsePlayerTurns && Session.ActivePlayer != null && ActiveControllers.ContainsKey(Session.ActivePlayer))
                {
                    ActiveControllers[Session.ActivePlayer]?.HandleMessage(sender, msg);
                }
                // Otherwise, redirect input messages to the player given by msg.Player
                else if(msg.Player != null && ActiveControllers.ContainsKey(msg.Player) && ActiveControllers.ContainsKey(msg.Player))
                {
                    ActiveControllers[msg.Player]?.HandleMessage(sender, msg);
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
