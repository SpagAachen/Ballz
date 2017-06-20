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

        public List<WeaponControl> AvailableWeapons { get; set; } = new List<WeaponControl>();
        public int SelectedWeaponIndex { get; set; } = 0;

        public bool CanSwitchWeapons { get; set; } = true;
		public bool ballMadeAction = false;
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
            //bool ballMadeAction = false;
            if (!Ball.IsAlive)
            {
                Ball.IsAiming = false;
                Ball.ShootCharge = 0f;
                Ball.IsCharging = false;
            }
            else
            {
                var projectileInAir = (Weapon as Weapons.ChargedProjectileWeapon)?.ProjectileInAir;
                if (projectileInAir != null && !projectileInAir.Disposed)
                {
                    Game.Match.FocussedEntity = projectileInAir;
                }
                else if (Ball == Game.Match.ActivePlayer?.ActiveBall)
                {
                    Game.Match.FocussedEntity = Ball;
                }

				bool weaponSwitchAllowed = true;
				if (!(Game.Match.SessionLogic.Session.TurnState == TurnState.WaitingForEnd))
					Weapon?.Update (elapsedSeconds, KeyPressed, out ballMadeAction, out weaponSwitchAllowed);
				//if (Ballz.The ().Match.SessionLogic.Session.UsePlayerTurns && ballMadeAction)
				else {
					ballMadeAction = false;
					weaponSwitchAllowed = false;
				}
				CanSwitchWeapons &= weaponSwitchAllowed;
					
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

        /// <summary>
        /// Called when the turn of this ball is over
        /// </summary>
        public virtual void OnTurnEnd()
        {
            AvailableWeapons.ForEach(w => w.OnTurnEnd());
            if (Ball.AttachedRope != null)
            {
                Match.Physics.RemoveRope(Ball.AttachedRope);
                Ball.AttachedRope = null;
                Match.World.Ropes.Remove(Ball.AttachedRope);
            }
        }

        /// <summary>
        /// Called when the turn of this ball is started
        /// </summary>
        public virtual void OnTurnStart()
        {
            CanSwitchWeapons = true;
            AvailableWeapons.ForEach(w => w.OnTurnStart());
			ballMadeAction = false;
        }

        const float PauseBetweenJumps = 0.1f;
        protected float JumpCoolDown = 0f;

        public void TryJump()
        {
            if (JumpCoolDown <= 0f)
            {
                // Send raycasts to the bottom. If it hits anything, perform the actual jump.

                float closestDist = float.MaxValue;
                Vector2 bestPos = Vector2.Zero;

                for(float angle = -180f; angle <= 180f; angle += 5f)
                {
                    var rayDirection = new Vector2(0, -(Ball.Radius + 0.05f));
					rayDirection = rayDirection.Rotate(angle * (float)Math.PI / 180f);
                    var rayHit = Match.Physics.Raycast(Ball.Position, Ball.Position + rayDirection);
					if (rayHit.HasHit)
                    {
                        Vector2 pos = rayHit.Position;
                        float dist = Vector2.Distance(pos, Ball.Position);

                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            bestPos = pos;
                        }
                    }
                }

                if (closestDist < float.MaxValue)
                {
                    //Ball.Velocity = new Vector2(Ball.Velocity.X, 0) - 5f * Vector2.Normalize(bestPos - Ball.Position);
                    Vector2 VecUp = new Vector2(0, 1);
                    if (Ballz.The().Match.World.StaticGeometry.HasGravityPoint)
                    {
                        Vector2.Normalize(Ball.Position - Ballz.The().Match.World.StaticGeometry.GravityPoint);
                    }

                    Ball.Velocity = 5f * VecUp;
					JumpCoolDown = PauseBetweenJumps;

                }
            }
        }

        public virtual void HandleMessage(object sender, Message message)
        {
            if (Game.Match.State == GameSession.SessionState.Running)
            {
                InputMessage input = message as InputMessage;
                if (input?.Pressed != null)
                {
                    // Weapons are only controlled by the server
                    if (!Match.IsRemoteControlled)
                    {
                        Weapon?.HandleInput(input);
                    }
                    KeyPressed[input.Kind] = input.Pressed;
                }
            }
        }   
    }
}
