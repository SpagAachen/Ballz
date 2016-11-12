using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ballz.GameSession.World;
using Ballz.Messages;

namespace Ballz.GameSession.Logic
{
    public abstract class WeaponControl
    {
        public WeaponControl(Ball ball, Ballz game)
        {
            Ball = ball;
            Game = game;
        }

        protected Ballz Game { get; set; }

        public Ball Ball { get; set; }

        public abstract string Name { get; }

        public abstract string Icon { get; }

        public virtual void OnTurnEnd() { }
        public virtual void OnTurnStart() { }
        public virtual void FireShot() { }
        /// <summary>
        /// Updates the weapon state and performs weapon actions.
        /// </summary>
        /// <returns>Returns true if the ball has made an action that finishes a player turn.</returns>
        public virtual void Update(float elapsedSeconds, Dictionary<InputMessage.MessageType, bool> keysPressed, out bool turnEndindActionHappened, out bool canSwitchWeapon)
        {
            turnEndindActionHappened = false;
            canSwitchWeapon = true;
        }

        public virtual void HandleInput(InputMessage input) { }
    }
}
