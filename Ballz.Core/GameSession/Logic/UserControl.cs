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
using static MathFloat.MathF;

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

        List<String> AvailableWeapons = new List<string> { "HandGun", "Bazooka", "RopeTool" };
        int SelectedWeapon = 0;

        public override void Update(float elapsedSeconds, World.World worldState)
        {
            base.Update(elapsedSeconds, worldState);
            
            if(IsAlive)
            {
                if (KeyPressed[InputMessage.MessageType.ControlsLeft])
                {
                    Ball.Velocity = new Vector2(Min(-2f, Ball.Velocity.X), Ball.Velocity.Y);
                    Ball.AimDirection = new Vector2(-Math.Abs(Ball.AimDirection.X), Ball.AimDirection.Y);
                }
                if (KeyPressed[InputMessage.MessageType.ControlsRight])
                {
                    Ball.Velocity = new Vector2(Max(2f, Ball.Velocity.X), Ball.Velocity.Y);
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

                if (Ball.HoldingWeapon == "Bazooka" || Ball.HoldingWeapon == "HandGun")
                {
                    IsCharging = KeyPressed[InputMessage.MessageType.ControlsAction];
                    Ball.IsAiming = true;
                    if (!IsCharging && Ball.ShootCharge > 0)
                        Shoot();
                }
                else if (Ball.HoldingWeapon == "RopeTool")
                {
                    Ball.IsAiming = Ball.AttachedRope == null;
                }

                // Handle single-shot input events
                switch (controlInput)
                {
                    case InputMessage.MessageType.ControlsJump:
                        if (Ball.AttachedRope != null)
                            ToggleRope();
                        TryJump();
                        break;
                    case InputMessage.MessageType.ControlsUp:
                        if (Ball.HoldingWeapon == "RopeTool" && Ball.AttachedRope != null)
                        {
                            Match.Physics.ShortenRope(Ball.AttachedRope);
                        }
                        break;
                    case InputMessage.MessageType.ControlsDown:
                        if (Ball.HoldingWeapon == "RopeTool" && Ball.AttachedRope != null)
                        {
                            Match.Physics.LoosenRope(Ball.AttachedRope);
                        }
                        break;
                    case InputMessage.MessageType.ControlsNextWeapon:
                        SelectedWeapon = (SelectedWeapon+1) % AvailableWeapons.Count;
                        Ball.HoldingWeapon = AvailableWeapons[SelectedWeapon];
                        IsCharging = false;
                        Ball.ShootCharge = 0f;
                        break;
                    case InputMessage.MessageType.ControlsPreviousWeapon:
                        SelectedWeapon = SelectedWeapon-1;
                        if (SelectedWeapon < 0)
                            SelectedWeapon += AvailableWeapons.Count;
                        Ball.HoldingWeapon = AvailableWeapons[SelectedWeapon];
                        IsCharging = false;
                        Ball.ShootCharge = 0f;
                        break;
                    case InputMessage.MessageType.ControlsAction:
                        if (Ball.HoldingWeapon == "RopeTool")
                            ToggleRope();
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
