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
    public class Potato: WeaponControl
    {
        public Potato(Ball ball, Ballz game) : base(ball, game) { }
           

        public override string Icon { get; } = "Potato";

        public override string Name { get; } = "Potato of Randomness";

        Random random = new Random();

        /*
        private double pdf(double x)
        {
            return 1.0 / (19.36408 * Math.Pow(1.05, x));
        }
        private float SampleDamage()
        {
            double u = random.NextDouble();

            double sample = pdf(100.0 * u);
        }
        */

        public override bool Update(float elapsedSeconds, Dictionary<InputMessage.MessageType, bool> KeysPressed)
        {
            if (Game.Match.IsRemoteControlled)
                return false;

            Ball.IsCharging = KeysPressed[InputMessage.MessageType.ControlsAction];
            Ball.IsAiming = true;
            if (!Ball.IsCharging && Ball.ShootCharge > 0)
            {
                
                float damage = 100.0f / (1f + 99.0f * (float)random.NextDouble());
                Game.Services.GetService<SoundControl>().PlaySound(SoundControl.ShotSound);
                Shot newShot = new Shot
                    {
                        ProjectileTexture = "PotatoBullet",
                        BulletHoleRadius = damage / 10.0f,
                        ExplosionRadius = damage / 10.0f,
                        HealthDecreaseFromExplosionImpact = damage,
                        HealthDecreaseFromProjectileHit = damage,
                        ShotType = Shot.ShotType_T.Normal,
                        ExplosionDelay = 0.0f,
                        Recoil = 0.0f,
                        Position = Ball.Position + Ball.AimDirection * (Ball.Radius + 0.101f),
                        Velocity = Ball.AimDirection * Ball.ShootCharge * 25f,
                    };
                Game.Match.World.AddEntity(newShot);

                Ball.PhysicsBody.ApplyForce(-10000 * Ball.ShootCharge * newShot.Recoil * Ball.AimDirection);

                Ball.ShootCharge = 0f;
                return true;
            }
            else
                return false;
        }
    }
}

