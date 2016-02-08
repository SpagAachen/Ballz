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

namespace Ballz.GameSession.Logic
{
    public abstract class BallControl
    {
        public Session Match { get; set; }

        public Ballz Game { get; set; }

        public Ball Ball { get; set; }

        public WeaponControl Weapon { get; set; }

        protected Dictionary<InputMessage.MessageType, bool> KeyPressed = new Dictionary<InputMessage.MessageType, bool>();

        public BallControl(Ballz game, Session match, Ball ball)
        {
            Game = game;
            Match = match;
            Ball = ball;

            Weapon = new Weapons.RopeTool(ball, game);

            KeyPressed[InputMessage.MessageType.ControlsAction] = false;
            KeyPressed[InputMessage.MessageType.ControlsUp] = false;
            KeyPressed[InputMessage.MessageType.ControlsDown] = false;
            KeyPressed[InputMessage.MessageType.ControlsLeft] = false;
            KeyPressed[InputMessage.MessageType.ControlsRight] = false;
        }
        
        /// <summary>
        /// Updates the ball state and preforms ball-related actions.
        /// </summary>
        /// <returns>Returns true if the ball has performed an action that finishes a player turn.</returns>
        public virtual bool Update(float elapsedSeconds, World.World worldState)
        {
            bool ballMadeAction = false;
            if (!Ball.IsAlive)
            {
                Ball.IsAiming = false;
                Ball.ShootCharge = 0f;
                Ball.IsCharging = false;
            }
            else
            {
                ballMadeAction = Weapon?.Update(elapsedSeconds, KeyPressed) ?? false;
                if (Ball.IsCharging)
                {
                    Ball.ShootCharge += elapsedSeconds * 0.7f;
                    if (Ball.ShootCharge > 1f)
                        Ball.ShootCharge = 1f;
                }
                else
                    Ball.ShootCharge = 0f;

                JumpCoolDown -= elapsedSeconds;
            }

            return ballMadeAction;
        }
        
        const float PauseBetweenJumps = 0.2f;
        protected float JumpCoolDown = 0f;

        public void TryJump()
        {
            if (JumpCoolDown <= 0f)
            {
                // Send raycasts to the bottom. If it hits anything, perform the actual jump.

                for(float angle = -60f; angle < 60f; angle += 5f)
                {
                    var rayDirection = new Vector2(0, -(Ball.Radius + 0.05f));
                    rayDirection = rayDirection.Rotate(angle * (float)Math.PI / 180f);
                    var rayHit = Match.Physics.Raycast(Ball.Position, Ball.Position + rayDirection);
                    if (rayHit.HasHit)
                    {
                        Ball.Velocity = new Vector2(Ball.Velocity.X, 5f);
                        JumpCoolDown = PauseBetweenJumps;
                        break;
                    }
                }
            }
        }
        
        public virtual void HandleMessage(object sender, Message message)
        {
            InputMessage input = message as InputMessage;
            if (input?.Pressed != null)
            {
                Weapon?.HandleInput(input);
                KeyPressed[input.Kind] = input.Pressed.Value;
            }
        }   
    }
}
