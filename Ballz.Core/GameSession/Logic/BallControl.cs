using Ballz.GameSession.World;
using Ballz.Messages;
using Ballz.Utils;
using Ballz.Sound;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ballz.GameSession.Logic
{
    public class BallControl
    {
        public Session Match;
        public Ballz Game;

        public Ball Ball;

        InputMessage.MessageType? controlInput = null;
        Dictionary<InputMessage.MessageType, bool> KeyPressed = new Dictionary<InputMessage.MessageType, bool>();

        public BallControl(Ballz game, Session match, Ball ball)
        {
            Game = game;
            Match = match;
            Ball = ball;

            KeyPressed[InputMessage.MessageType.ControlsAction] = false;
            KeyPressed[InputMessage.MessageType.ControlsUp] = false;
            KeyPressed[InputMessage.MessageType.ControlsDown] = false;
            KeyPressed[InputMessage.MessageType.ControlsLeft] = false;
            KeyPressed[InputMessage.MessageType.ControlsRight] = false;
        }
        
        public void Update(float elapsedSeconds, World.World worldState)
        {
            if (!IsAlive)
            {
                Ball.IsAiming = false;
            }
            else
            {

                if (KeyPressed[InputMessage.MessageType.ControlsLeft])
                {
                    Ball.Velocity = new Vector2(-2f, Ball.Velocity.Y);
                    Ball.AimDirection = new Vector2(-Math.Abs(Ball.AimDirection.X), Ball.AimDirection.Y);
                }
                if (KeyPressed[InputMessage.MessageType.ControlsRight])
                {
                    Ball.Velocity = new Vector2(2f, Ball.Velocity.Y);
                    Ball.AimDirection = new Vector2(Math.Abs(Ball.AimDirection.X), Ball.AimDirection.Y);
                }

                // Up/Down keys rotate the aim vector
                if (KeyPressed[InputMessage.MessageType.ControlsUp])
                {
                    var v = Ball.AimDirection;
                    // Rotate at 60°/s. Use sign of v.x to determine the direction, so that the up key always moves the crosshair upwards.
                    var radians = (v.X > 0 ? 1 : -1) * elapsedSeconds * 2 * (float)Math.PI * 60f / 360f;
                    Ball.AimDirection = v.Rotate(radians);
                }
                if (KeyPressed[InputMessage.MessageType.ControlsDown])
                {
                    var v = Ball.AimDirection;
                    // Rotate at 60°/s. Use sign of v.x to determine the direction, so that the up key always moves the crosshair upwards.
                    var radians = (v.X > 0 ? -1 : 1) * elapsedSeconds * 2 * (float)Math.PI * 60f / 360f;
                    Ball.AimDirection = v.Rotate(radians);
                }

                if(KeyPressed[InputMessage.MessageType.ControlsAction])
                {
                    Ball.ShootCharge += elapsedSeconds * 0.33f;
                    if (Ball.ShootCharge > 1f)
                        Ball.ShootCharge = 1f;
                }
                else if(Ball.ShootCharge > 0 )
                {
                    Game.Services.GetService<SoundControl>().playSound(SoundControl.shotSound);
                    worldState.Entities.Add(new Shot
                    {
                        ExplosionRadius = 1.0f,
                        HealthImpactAtDirectHit = 25,
                        IsInstantShot = false,
                        Position = Ball.Position + Ball.AimDirection * (Ball.Radius + 0.2f),
                        Velocity = Ball.AimDirection * Ball.ShootCharge * 30f,
                        ShooterId = Ball.ID
                    });

                    Ball.ShootCharge = 0f;
                }
                // Handle single-shot input events
                switch (controlInput)
                {
                    case InputMessage.MessageType.ControlsJump:
                        Ball.Velocity = new Vector2(Ball.Velocity.X, 5f);
                        break;
                    default:
                        break;
                }
            }
            controlInput = null;

        }

        private void processInput(InputMessage message)
        {
            if (message.Pressed.HasValue)
            {
                KeyPressed[message.Kind] = message.Pressed.Value;

                if (message.Pressed.Value)
                    controlInput = message.Kind;
            }
        }

        public void HandleMessage(object sender, Message message)
        {
            if (message.Kind == Message.MessageType.InputMessage)
            {
                InputMessage msg = (InputMessage)message;

                processInput(msg);
            }

        }

        public bool IsAlive {  get { return Ball.Health > 0;  } }
    }
}
