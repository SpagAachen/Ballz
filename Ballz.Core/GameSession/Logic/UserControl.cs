using Ballz.GameSession.World;
using Ballz.Messages;
using Ballz.Sound;
using Ballz.Utils;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MathFloat.MathF;

namespace Ballz.GameSession.Logic
{
    public class UserControl: BallControl
    {
        public UserControl(Ballz game, Session match, Ball ball):
            base(game, match, ball)
        {
            AvailableWeapons.Add(new Weapons.Potato(ball, game));
            AvailableWeapons.Add(new Weapons.RopeTool(ball, game));
            AvailableWeapons.Add(new Weapons.Bazooka(ball, game));
            AvailableWeapons.Add(new Weapons.Pistol(ball, game));
            Weapon = AvailableWeapons[SelectedWeaponIndex];
            Ball.HoldingWeapon = AvailableWeapons[SelectedWeaponIndex].Icon;
        }

        InputMessage.MessageType? controlInput = null;

        const float WalkingSpeedSlow = 0.1f;
        const float SlowWalkTime = 0.1f;
        const float WalkingSpeedNormal = 2f;

        float WalkingTime = 0;

        const float ViewRotationSpeed = 3f;

        public override bool Update(float elapsedSeconds, World.World worldState)
        {
            bool ballMadeAction = base.Update(elapsedSeconds, worldState);
            
            if(Ball.IsAlive)
            {
                if (KeyPressed[InputMessage.MessageType.ControlsLeft])
                {
                    var speed = WalkingTime < SlowWalkTime ? WalkingSpeedSlow : WalkingSpeedNormal;
                    Ball.Velocity = new Vector2(Min(-speed, Ball.Velocity.X), Ball.Velocity.Y);
                    Ball.AimDirection = new Vector2(-Math.Abs(Ball.AimDirection.X), Ball.AimDirection.Y);
                    WalkingTime += elapsedSeconds;
                }
                else if (KeyPressed[InputMessage.MessageType.ControlsRight])
                {
                    var speed = WalkingTime < SlowWalkTime ? WalkingSpeedSlow : WalkingSpeedNormal;
                    Ball.Velocity = new Vector2(Max(speed, Ball.Velocity.X), Ball.Velocity.Y);
                    Ball.AimDirection = new Vector2(Math.Abs(Ball.AimDirection.X), Ball.AimDirection.Y);
                    WalkingTime += elapsedSeconds;
                }
                else
                {
                    WalkingTime = 0;
                }

                if (Ball.Velocity.X > 0.01 && Ball.ViewRotation < 0.99)
                    Ball.ViewRotation += ViewRotationSpeed * elapsedSeconds;
                else if (Ball.Velocity.X < -0.01 && Ball.ViewRotation > -0.99)
                    Ball.ViewRotation -= ViewRotationSpeed * elapsedSeconds;
                else if (Abs(Ball.Velocity.X) < 0.01 && Abs(Ball.ViewRotation) > 0.01)
                    Ball.ViewRotation -= Math.Sign(Ball.ViewRotation) * ViewRotationSpeed * elapsedSeconds;

                Ball.ViewRotation = Max(-1, Min(Ball.ViewRotation, 1));

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

                // Handle single-shot input events
                switch (controlInput)
                {
                    case InputMessage.MessageType.ControlsJump:
                        TryJump();
                        break;
                    case InputMessage.MessageType.ControlsNextWeapon:
                        if (CanSwitchWeapons)
                        {
                            SelectedWeaponIndex = (SelectedWeaponIndex + 1) % AvailableWeapons.Count;
                            Weapon = AvailableWeapons[SelectedWeaponIndex];
                            Ball.HoldingWeapon = AvailableWeapons[SelectedWeaponIndex].Icon;
                            Ball.IsCharging = false;
                            Ball.ShootCharge = 0f;
                        }
                        break;
                    case InputMessage.MessageType.ControlsPreviousWeapon:
                        if (CanSwitchWeapons)
                        {
                            SelectedWeaponIndex = SelectedWeaponIndex - 1;
                            if (SelectedWeaponIndex < 0)
                                SelectedWeaponIndex += AvailableWeapons.Count;
                            Weapon = AvailableWeapons[SelectedWeaponIndex];
                            Ball.HoldingWeapon = AvailableWeapons[SelectedWeaponIndex].Icon;
                            Ball.IsCharging = false;
                            Ball.ShootCharge = 0f;
                        }
                        break;
                    default:
                        break;
                }
            }

            controlInput = null;

            return ballMadeAction;
        }

        private void ProcessInput(InputMessage message)
        {
        }

        public override void HandleMessage(object sender, Message message)
        {
            base.HandleMessage(sender, message);
            
            InputMessage input = message as InputMessage;
            if (input?.Pressed ?? false)
            {
                controlInput = input.Kind;
            }
        }
    }
}
