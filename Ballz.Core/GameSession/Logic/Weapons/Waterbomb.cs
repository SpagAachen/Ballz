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
	public class Waterbomb: ChargedProjectileWeapon
	{
		public Waterbomb(Ball ball, Ballz game) : base(ball, game) { }


		public override string Icon { get; } = "Waterbomb";

		public override string Name { get; } = "Water Bomb";

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
			return new Shot
			{
				ProjectileTexture = "WaterbombBullet",
				BulletHoleRadius = 0.0f,
				ExplosionRadius = 1e-12f,
				HealthDecreaseFromExplosionImpact = 0.0f,
				HealthDecreaseFromProjectileHit = 0.0f,
				ShotType = Shot.ShotType_T.Generating,
				ExplosionDelay = 0.0f,
				Recoil = 0.0f,
				Position = Ball.Position + Ball.AimDirection * (Ball.Radius + 0.101f),
				Velocity = Ball.AimDirection * Ball.ShootCharge * 25f,
				Team = Ball.Player.TeamName
			};
		}

	}
}

