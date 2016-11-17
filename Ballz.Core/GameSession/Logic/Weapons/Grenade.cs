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
    public class Grenade: ChargedProjectileWeapon
    {
        public Grenade(Ball ball, Ballz game) : base(ball, game) { }
           

        public override string Icon { get; } = "GrenadeIcon";

        public override string Name { get; } = "GrenadeBullet";

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
            float damage = 50.0f;
            return new Shot
            {
                ProjectileTexture = "GrenadeBullet",
                BulletHoleRadius = 0.0f, // No terrain modification before explosion
                ExplosionRadius = 4.0f,
                HealthDecreaseFromExplosionImpact = damage,
                HealthDecreaseFromProjectileHit = damage / 5,
                ShotType = Shot.ShotType_T.Normal,
                ExplosionDelay = 2.0f,
                Recoil = 0.0f,
                Position = Ball.Position + Ball.AimDirection * (Ball.Radius + 0.3f),
                Velocity = Ball.AimDirection * Ball.ShootCharge * 25f,
                Radius = 0.3f,
                Restitution = 0.3f,
				Team = Ball.Player.TeamName,
				//SpawnEntity = 1
            };
        }
    }
}

