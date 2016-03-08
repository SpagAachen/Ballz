using System;
using System.Collections.Generic;
using System.Linq;
using Ballz.GameSession.Physics;
using Microsoft.Xna.Framework;

namespace Ballz.GameSession.World
{
    public class Water
    {
        private readonly int _width;
        private readonly int _height;

        private const int GridMultiplier = 10;
        
        private PhysicsControl _physics;

        public Water(int width, int height)
        {
            _width = width;
            _height = height;
            _grid = new List<int>[_width* GridMultiplier, _height * GridMultiplier];
            for (var x = 0; x < _width * GridMultiplier; ++x)
                for (var y = 0; y < _height* GridMultiplier; ++y)
                    _grid[x,y] = new List<int>();
        }
        
        public void Initialize(World world, PhysicsControl physics)
        {
            _physics = physics;
            Particles = new Vector2[ParticleCount];
            Velocities = new Vector2[ParticleCount];
            _velocityBuffer = new Vector2[ParticleCount];
            var rng = new Random();
            for (var i = 0; i < ParticleCount; ++i)
            {
                do
                {
                    Particles[i] = new Vector2((float)(rng.NextDouble() * _width), (float)(rng.NextDouble() * _height));
                } while (!_physics.IsEmpty(Particles[i]));
                Velocities[i] = new Vector2((float) rng.NextDouble()*2-1, (float) rng.NextDouble()*2-1);
            }

            for (var i = 0; i < ParticleCount; ++i)
                _grid[(int)(Particles[i].X * GridMultiplier), (int)(Particles[i].Y * GridMultiplier)].Add(i);
        }

        public const int ParticleCount = 10000;

        public Vector2[] Particles { get; private set; }
        public Vector2[] Velocities { get; private set; }
        private readonly List<int>[,] _grid;

        private Vector2[] _velocityBuffer;


        private void Viscosity(float elapsedSeconds)
        {
            const float µ = 0.5f;
            for (var i = 0; i < ParticleCount; ++i)
            {
                var ix = (int)(Particles[i].X*GridMultiplier);
                var iy = (int)(Particles[i].Y*GridMultiplier);

                var neighbors = new List<int>();
                neighbors.AddRange(_grid[ix, iy]);
                if (ix < _width - 1)
                    neighbors.AddRange(_grid[ix + 1, iy]);
                if (iy < _height - 1)
                    neighbors.AddRange(_grid[ix, iy + 1]);
                if (ix > 0)
                    neighbors.AddRange(_grid[ix - 1, iy]);
                if (iy > 0)
                    neighbors.AddRange(_grid[ix, iy - 1]);

                var avg = neighbors.Select(ii => Velocities[ii]).Aggregate((l, r) => l + r);
                avg /= neighbors.Count;
                _velocityBuffer[i] = Velocities[i] + elapsedSeconds * µ * (avg - Velocities[i]);
            }

            var tmp = _velocityBuffer;
            _velocityBuffer = Velocities;
            Velocities = tmp;
        }
        private void Avoidance(float elapsedSeconds)
        {
            const float k = 5f;
            const float l = 0.5f;
            for (var i = 0; i < ParticleCount; ++i)
            {
                var ix = (int)(Particles[i].X * GridMultiplier);
                var iy = (int)(Particles[i].Y * GridMultiplier);

                var neighbors = new List<int>();
                neighbors.AddRange(_grid[ix, iy]);
                if (ix < _width - 1)
                    neighbors.AddRange(_grid[ix + 1, iy]);
                if (iy < _height - 1)
                    neighbors.AddRange(_grid[ix, iy + 1]);
                if (ix > 0)
                    neighbors.AddRange(_grid[ix - 1, iy]);
                if (iy > 0)
                    neighbors.AddRange(_grid[ix, iy - 1]);

                _velocityBuffer[i] = Velocities[i];
                foreach (var j in neighbors)
                {
                    if (i == j)
                        continue;
                    var dist = Vector2.Distance(Particles[i], Particles[j]);
                    if (dist > l)
                        continue;
                    var n = Particles[j] - Particles[i];
                    if (n.Length() == 0)
                        continue;
                    n.Normalize();
                    _velocityBuffer[i] += elapsedSeconds * k * (dist-l) * n;
                }
            }

            var tmp = _velocityBuffer;
            _velocityBuffer = Velocities;
            Velocities = tmp;
        }

        public void Step(World worldState, float elapsedSeconds)
        {
            Viscosity(elapsedSeconds);
            Avoidance(elapsedSeconds);
            

            for (var i = 0; i < ParticleCount; ++i)
            {
                //Velocities[i].Y += -.81f*elapsedSeconds;
                Particles[i] = Particles[i] + Velocities[i]*elapsedSeconds;
                var x = Math.Max(0, Math.Min(Particles[i].X,_width-1));
                var y = Math.Max(0, Math.Min(Particles[i].Y,_height-1));

                if (x != Particles[i].X)
                    Velocities[i].X = -Velocities[i].X;

                if (y != Particles[i].Y)
                    Velocities[i].Y = -Velocities[i].Y;

                Particles[i].X = x;
                Particles[i].Y = y;
            }

            for (var x = 0; x < _width*GridMultiplier; ++x)
                for (var y = 0; y < _height * GridMultiplier; ++y)
                    _grid[x,y].Clear();

            for (var i = 0; i < ParticleCount; ++i)
                _grid[(int)(Particles[i].X * GridMultiplier), (int)(Particles[i].Y * GridMultiplier)].Add(i);
        }
    }
}