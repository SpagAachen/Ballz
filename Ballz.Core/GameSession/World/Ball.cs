using Ballz.GameSession.Logic;
using Microsoft.Xna.Framework;
using ObjectSync;
using System;

namespace Ballz.GameSession.World
{
    /// <summary>
    /// Represents a Ball character. 
    /// </summary>
    [Serializable]
    public class Ball : Entity
    {
        public Ball()
        {
            Material = PhysicsMaterial.Ball;
            Radius = 0.8f;
        }

        public string Name { get; set; } = "Nobody";

        /// <summary>
        /// The health value of the ball. Typical value ranges are 0-100.
        /// </summary>
        [Synced]
        public double Health { get; set; } = 100;

        [Synced]
        public bool IsAlive { get { return Health > 0; } }

        [Synced]
        public bool IsAiming { get; set; } = false;

        [Synced]
        public bool IsCharging { get; set; } = false;

        /// <summary>
        /// Indicates the "charging level" that is used to control the initial velocity of certain projectiles.
        /// </summary>
        [Synced]
        public float ShootCharge { get; set; } = 0f;

        [Newtonsoft.Json.JsonIgnore]
        Player _player;

        [Newtonsoft.Json.JsonIgnore]
        public Player Player {
            get
            {
                return _player;
            }
            set
            {
                _player = value;
            }
        }

        [Synced]
        public Vector2 AimDirection { get; set; } = Vector2.UnitX;

        [Synced]
        public string HoldingWeapon;

        [Newtonsoft.Json.JsonIgnore]
        public Rope AttachedRope = null;

        public override void OnEntityCollision(Entity other)
        {
/*            var shot = other as Shot;
            if (shot != null)
            {
                float impact = shot.Velocity.Length() * shot.ExplosionRadius;
                if (impact < 5)
                    return;

                //TODO(ks) more elaborate damage model
                Health -= shot.HealthDecreaseFromProjectileHit * Math.Min(1, impact / 20);
                if (Health < 0)
                    Health = 0;

                PhysicsBody.ApplyLinearImpulse(10 * shot.Velocity);
            }
            else
            {
                // Other balls, etc -> no-op
            }
*/        }

        public void ChangeHealth(double healthDifference)
        {
            Health += healthDifference;

            if (healthDifference < 0)
            {
                Ballz.The().Match.World.GraphicsEvents.Add(new TextEffect
                    {
                        Text =$"{(int)healthDifference}",
                        TextSize = 0.5f,
                        TextColor = Color.Red,
                        Start = Ballz.The().Match.GameTime,
                        Duration = 3f,
                        PositionStart = Position + new Vector2(0, 1),
                        PositionEnd = Position + new Vector2(0, 3),
                        OpacityEnd = 0,
                    });
            }
            else if (healthDifference > 0)
            {
                Ballz.The().Match.World.GraphicsEvents.Add(new TextEffect{
                    Text = $"+{(int)healthDifference}",
                    TextSize = 0.5f,
                    TextColor = Color.Green,
                    Start = Ballz.The().Match.GameTime,
                    Duration = 3f,
                    PositionStart = Position + new Vector2(0, 1),
                    PositionEnd = Position + new Vector2(0, 3),
                    OpacityEnd = 0,
                });
            }
			if (Health <= 0.0f)
				Dispose ();
        }

        public override void OnTerrainCollision(Terrain terrain, Vector2 position)
        {
            base.OnTerrainCollision(terrain, position);
        }
    }
}