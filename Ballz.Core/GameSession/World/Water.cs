using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ballz.Utils;
using Microsoft.Xna.Framework;
using Ballz.GameSession.Physics;

namespace Ballz.GameSession.World
{
    public class Water
    {
        private readonly int _width;
        private readonly int _height;

        PhysicsControl _physics;

        public Water(int width, int height)
        {
            _width = width;
            _height = height;
            Particles = new Vector2[ParticleCount];
        }

        public void Initialize(World world, PhysicsControl physics)
        {
            _physics = physics;
            var rng = new Random();
            for (var i = 0; i < ParticleCount; ++i)
            {
                var pos = new Vector2(rng.Next(0, _width), rng.Next(0, _height));
                if (_physics.IsEmpty(pos))
                    Particles[i] = pos;
            }
        }

        public const int ParticleCount = 1000;

        public Vector2[] Particles { get; }

        public void Step(World worldState, float elapsedSeconds)
        {
        }
    }
}