using Ballz.GameSession.World;
using Ballz.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ballz.GameSession.Logic.Weapons
{
    public class Bazooka: WeaponControl
    {
        public Bazooka(Ball ball, Ballz game) : base(ball, game) { }

        public override string Icon { get; } = "Bazooka";

        public override string Name { get; } = "Bazooka";

        public override bool Update(float elapsedSeconds, Dictionary<InputMessage.MessageType, bool> KeysPressed)
        {
            Ball.IsCharging = KeysPressed[InputMessage.MessageType.ControlsAction];
            Ball.IsAiming = true;
            if (!Ball.IsCharging && Ball.ShootCharge > 0)
            {
                FireProjectile();
                return true;
            }
            else
                return false;
        }
    }
}
