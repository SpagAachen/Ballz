using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ballz.Utils;
using Microsoft.Xna.Framework;

namespace Ballz.GameSession.World
{
    public class Water
    {
        private readonly int _width;
        private readonly int _height;

        public Water(int width, int height)
        {
            _width = width;
            _height = height;
            Particles = new Vector2[ParticleCount];
            var rng = new Random();
            for (var i = 0; i < ParticleCount; ++i)
            {
                Particles[i].X = rng.Next(0, _width);
                Particles[i].Y = rng.Next(0, _height);
            }
        }

        public const int ParticleCount = 1000;

        public Vector2[] Particles { get; }

        public void Step(World worldState, float elapsedSeconds)
        {
        }
    }
}