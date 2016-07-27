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

        public override void Update(float elapsedSeconds, Dictionary<InputMessage.MessageType, bool> KeysPressed, out bool turnEndindActionHappened, out bool canSwitchWeapon)
        {
            base.Update(elapsedSeconds, KeysPressed, out turnEndindActionHappened, out canSwitchWeapon);

            if (Game.Match.IsRemoteControlled)
                return;

            Ball.IsCharging = KeysPressed[InputMessage.MessageType.ControlsAction];
            canSwitchWeapon = Ball.ShootCharge == 0;

            Ball.IsAiming = true;
            if (!Ball.IsCharging && Ball.ShootCharge > 0)
            {
                Game.Services.GetService<SoundControl>().PlaySound(SoundControl.BazookaSound);
                Shot newShot = new Shot
                {
                    ExplosionRadius = 3.0f,
                    HealthDecreaseFromExplosionImpact = 25,
                    HealthDecreaseFromProjectileHit = 100,
                    ShotType = Shot.ShotType_T.Normal,
                    ExplosionDelay = 0.0f,
                    Recoil = 1.0f,
                    Position = Ball.Position + Ball.AimDirection * (Ball.Radius + 0.101f),
                    Velocity = Ball.AimDirection * Ball.ShootCharge * 25f,
                };
                Game.Match.World.AddEntity(newShot);
                turnEndindActionHappened = true;
                canSwitchWeapon = false;

                Ball.PhysicsBody.ApplyForce(-10000 * Ball.ShootCharge * newShot.Recoil * Ball.AimDirection);

                Ball.ShootCharge = 0f;
            }
        }
    }
}
