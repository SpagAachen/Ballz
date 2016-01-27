using Ballz.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ballz.GameSession.World;
using Microsoft.Xna.Framework;
using Ballz.Sound;
using Ballz.Utils;

namespace Ballz.GameSession.Logic
{
    public class UserControl: BallControl
    {
        public UserControl(Ballz game, Session match, Ball ball):
            base(game, match, ball)
        {
            KeyPressed[InputMessage.MessageType.ControlsAction] = false;
            KeyPressed[InputMessage.MessageType.ControlsUp] = false;
            KeyPressed[InputMessage.MessageType.ControlsDown] = false;
            KeyPressed[InputMessage.MessageType.ControlsLeft] = false;
            KeyPressed[InputMessage.MessageType.ControlsRight] = false;
        }

        InputMessage.MessageType? controlInput = null;
        Dictionary<InputMessage.MessageType, bool> KeyPressed = new Dictionary<InputMessage.MessageType, bool>();

        public override void Update(float elapsedSeconds, World.World worldState)
        {
            base.Update(elapsedSeconds, worldState);
            
            if(IsAlive)
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

                IsCharging = KeyPressed[InputMessage.MessageType.ControlsAction];

                if (!IsCharging && Ball.ShootCharge > 0)
                    Shoot();

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

        public override void HandleMessage(object sender, Message message)
        {
            base.HandleMessage(sender, message);

            if (message.Kind == Message.MessageType.InputMessage)
            {
                InputMessage msg = (InputMessage)message;
                processInput(msg);
            }
        }

    }
}
