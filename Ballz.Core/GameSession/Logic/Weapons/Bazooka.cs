using Ballz.GameSession.World;
using Ballz.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ballz.Sound;

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
                Game.Services.GetService<SoundControl>().PlaySound(SoundControl.ShotSound);
                Game.World.Entities.Add(new Shot
                    {
                        ExplosionRadius = 10.0f,
                        HealthImpactAtDirectHit = 25,
                        IsInstantShot = false,
                        Position = Ball.Position + Ball.AimDirection * (Ball.Radius + 0.101f),
                        Velocity = Ball.AimDirection * Ball.ShootCharge * 25f,
                    });

                Ball.ShootCharge = 0f;
                return true;
            }
            else
                return false;
        }
    }
}
