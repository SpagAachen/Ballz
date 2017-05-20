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
    public class UserControl : BallControl
    {
        public UserControl(Ballz game, Session match, Ball ball) :
            base(game, match, ball)
        {
            AvailableWeapons.Add(new Weapons.Potato(ball, game));
            AvailableWeapons.Add(new Weapons.Grenade(ball, game));
            AvailableWeapons.Add(new Weapons.RopeTool(ball, game));
            AvailableWeapons.Add(new Weapons.Bazooka(ball, game));
            AvailableWeapons.Add(new Weapons.Pistol(ball, game));
            AvailableWeapons.Add(new Weapons.Waterbomb(ball, game));
            AvailableWeapons.Add(new Weapons.Drill(ball, game));
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

            if (Ball.IsAlive)
            {
                Vector2 upDir = new Vector2(0, 1);

                if (worldState.StaticGeometry.HasGravityPoint)
                {
                    upDir = Vector2.Normalize(Ball.Position - worldState.StaticGeometry.GravityPoint);
                }

                Vector2 rightDir = new Vector2(upDir.Y, -upDir.X); ;

                // Calculate movement in local coordinate system that has the x axis orthogonal to the gravity
                Matrix toGlobalOrientation = new Matrix(
                        new Vector4(rightDir.X, rightDir.Y, 0, 0),
                        new Vector4(upDir.X, upDir.Y, 0, 0),
                        new Vector4(0, 0, 1, 0),
                        new Vector4(0, 0, 0, 1)
                );

                Matrix toLocalOrientation = Matrix.Invert(toGlobalOrientation);

                if (KeyPressed[InputMessage.MessageType.ControlsLeft] || KeyPressed[InputMessage.MessageType.ControlsRight])
                {
                    var speed = WalkingTime < SlowWalkTime ? WalkingSpeedSlow : WalkingSpeedNormal;
                    
                    float localMovementTarget = speed * (KeyPressed[InputMessage.MessageType.ControlsRight] ? 1 : -1);
                    Vector2 localVelocity = Vector2.Transform(Ball.Velocity, toLocalOrientation);

                    // In local coordinates, movement only influences the velocity on the X axis.
                    // Only apply the movement velocity if it would either increase the current velocity or change the direction.
                    if(Abs(localMovementTarget) > Abs(localVelocity.X) || Math.Sign(localMovementTarget) != Math.Sign(localVelocity.X))
                    {
                        localVelocity.X = localMovementTarget;
                    }

                    Ball.Velocity = Vector2.Transform(localVelocity, toGlobalOrientation);

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


                if (Ball.Player == Match.PlayerByNumber(1) || Match.UsePlayerTurns)
                {
                    Ball.AimDirection = Ballz.The().MouseAimDirection;
                }
                else
                {
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

                    if (KeyPressed[InputMessage.MessageType.ControlsLeft])
                        Ball.AimDirection = new Vector2(-Math.Abs(Ball.AimDirection.X), Ball.AimDirection.Y);
                    else if (KeyPressed[InputMessage.MessageType.ControlsRight])
                        Ball.AimDirection = new Vector2(Math.Abs(Ball.AimDirection.X), Ball.AimDirection.Y);
                }

                // Handle single-shot input events
                ProcessInput(controlInput);
            }

            controlInput = null;

            return ballMadeAction;
        }

        private void ProcessInput(InputMessage.MessageType? messageType)
        {
            if (messageType == null)
                return;

            switch (messageType)
            {
                case InputMessage.MessageType.ControlsCameraModeToggle:
                    {
                        var cam = Game.Camera;
                        var time = Game.Match.GameTime;
                        cam.SetZoom(1f, true, time);
                    }
                    break;
                case InputMessage.MessageType.ControlsCameraZoomIn:
                    {
                        var cam = Game.Camera;
                        var time = Game.Match.GameTime;
                        cam.SetZoom(cam.Zoom * 1.2f, true, time);
                    }
                    break;
                case InputMessage.MessageType.ControlsCameraZoomOut:
                    {
                        var cam = Game.Camera;
                        var time = Game.Match.GameTime;
                        cam.SetZoom(cam.Zoom * (1 / 1.2f), true, time);
                    }
                    break;
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
