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
    public class Potato: ChargedProjectileWeapon
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

        protected override Shot CreateShot()
        {
            float damage = 100.0f * (float)random.NextDouble(); //100.0f / (1f + 99.0f * (float)random.NextDouble());
            return new Shot
            {
                ProjectileTexture = "PotatoBullet",
                BulletHoleRadius = (float)Math.Sqrt(damage) / 2.0f,
                ExplosionRadius = (float)Math.Sqrt(damage) / 2.0f,
                HealthDecreaseFromExplosionImpact = damage,
                HealthDecreaseFromProjectileHit = damage,
                ShotType = Shot.ShotType_T.Normal,
                ExplosionDelay = 0.0f,
                Recoil = 0.0f,
                Position = Ball.Position + Ball.AimDirection * (Ball.Radius + 0.101f),
                Velocity = Ball.AimDirection * Ball.ShootCharge * 25f,
            };
        }
        
    }
}

