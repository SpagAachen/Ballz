using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ballz.GameSession.Physics;
using Ballz.Utils;
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
        Random rng = new Random();

        public void Initialize(World world, PhysicsControl physics)
        {
            _physics = physics;
            var particles = new List<Vector2>();
            var velocities = new List<Vector2>();
            Particles = new Vector2[ParticleCount];
            Velocities = new Vector2[ParticleCount];
            _velocityBuffer = new Vector2[ParticleCount];
            const float initGridSize = 0.2f;
            for (var x = 0.0f; x < _width; x += initGridSize)
                for (var y = 0.0f; y < _height; y += initGridSize)
                {
                    if (world.StaticGeometry.IsWaterSpawn(x,y))
                    {
                        particles.Add(new Vector2(x, y));
                        velocities.Add(new Vector2(0,0));
                        _grid[(int)(particles.Last().X * GridMultiplier), (int)(particles.Last().Y * GridMultiplier)].Add(particles.Count-1);
                    }
                }

            Particles = particles.ToArray();
            Velocities = velocities.ToArray();
            _velocityBuffer = Velocities.ToArray();
            ParticleCount = particles.Count;
            w = new float[ParticleCount, ParticleCount];
        }

        public int ParticleCount;
        public const float R = 0.05f;
        public const float D = 2*R;

        public Vector2[] Particles { get; private set; }
        public Vector2[] Velocities { get; private set; }
        private readonly List<int>[,] _grid;

        private Vector2[] _velocityBuffer;

        private float[,] w;

        private void Collision(float elapsedSeconds)
        {
            const int cellRadius = (int) (D*GridMultiplier);
            const float w0 = 0.0005f;
            const float η = 0.0005f;
            var h = new float[ParticleCount];
            for (var i = 0; i < ParticleCount; ++i)
            {
                var ix = (int) (Particles[i].X*GridMultiplier);
                var iy = (int) (Particles[i].Y*GridMultiplier);

                var neighbors = new List<int>();
                for (var dx = Math.Max(-cellRadius, -ix);
                    dx < Math.Min(cellRadius, _width*GridMultiplier - ix - 1);
                    ++dx)
                    for (var dy = Math.Max(-cellRadius, -iy);
                        dy < Math.Min(cellRadius, _height*GridMultiplier - iy - 1);
                        ++dy)
                        neighbors.AddRange(_grid[ix + dx, iy + dy]);

                var wi = 0f;
                foreach (var j in neighbors)
                {
                    if (i <= j)
                        continue;
                    var dist = Vector2.Distance(Particles[i], Particles[j]);
                    if (dist > 0.1f)
                        continue;

                    var n = Particles[j] - Particles[i];
                    w[i, j] += (1 - n.Length())/D;
                    wi += w[i, j];
                }
                h[i] = Math.Max(0, (wi - w0)*η);
            }
            for (var i = 0; i < ParticleCount; ++i)
            {
                var ix = (int) (Particles[i].X*GridMultiplier);
                var iy = (int) (Particles[i].Y*GridMultiplier);

                var neighbors = new List<int>();
                for (var dx = Math.Max(-cellRadius, -ix);
                    dx < Math.Min(cellRadius, _width*GridMultiplier - ix - 1);
                    ++dx)
                    for (var dy = Math.Max(-cellRadius, -iy);
                        dy < Math.Min(cellRadius, _height*GridMultiplier - iy - 1);
                        ++dy)
                        neighbors.AddRange(_grid[ix + dx, iy + dy]);
                
                foreach (var j in neighbors)
                {
                    Velocities[i] += elapsedSeconds*0.5f*(h[i] + h[j])*w[i, j]*(Particles[i] - Particles[j]);
                }
            }
        }

        private void Viscosity(float elapsedSeconds)
        {
            const float µ = 0.5f;
            const float l = 0.5f;
            const int cellRadius = (int)(GridMultiplier * l);
            Parallel.For(0, ParticleCount, i =>
            {
                var ix = (int) (Particles[i].X*GridMultiplier);
                var iy = (int) (Particles[i].Y*GridMultiplier);

                var neighbors = new List<int>();
                for (var dx = Math.Max(-cellRadius, -ix);
                    dx < Math.Min(cellRadius, _width*GridMultiplier - ix - 1);
                    ++dx)
                    for (var dy = Math.Max(-cellRadius, -iy);
                        dy < Math.Min(cellRadius, _height*GridMultiplier - iy - 1);
                        ++dy)
                        neighbors.AddRange(_grid[ix + dx, iy + dy]);

                if (!neighbors.Any())
                    return;

                var avg = neighbors.Select(ii => Velocities[ii]).Aggregate((ll, r) => ll + r);
                avg /= neighbors.Count;
                _velocityBuffer[i] = Velocities[i] + elapsedSeconds*µ*(avg - Velocities[i]);
            });

            var tmp = _velocityBuffer;
            _velocityBuffer = Velocities;
            Velocities = tmp;
        }

        private void Avoidance(float elapsedSeconds)
        {
            const float k = 15f;
            const float l = 0.5f;
            const int cellRadius = (int) (GridMultiplier*l);
            for (var i = 0; i < ParticleCount; ++i)
            {
                var ix = (int)(Particles[i].X * GridMultiplier);
                var iy = (int)(Particles[i].Y * GridMultiplier);

                var neighbors = new List<int>();
                for (var dx = Math.Max(-cellRadius,-ix); dx < Math.Min(cellRadius,_width*GridMultiplier-ix-1); ++dx)
                    for (var dy = Math.Max(-cellRadius,-iy); dy < Math.Min(cellRadius,_height*GridMultiplier-iy-1); ++dy)
                        neighbors.AddRange(_grid[ix + dx, iy + dy]);

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
                const int dir = 4;
                for (var j = 0; j < dir; ++j)
                {
                    var wall = Particles[i] + Vector2.UnitX.Rotate((float) (2f*Math.PI*j/dir))*l;
                    if (!_physics.IsEmpty(wall))
                    {
                        var res = _physics.Raycast(Particles[i], wall);
                        if (!res.HasHit)
                            continue;
                        var n = res.Position - Particles[i];
                        var dist = Vector2.Distance(Particles[i], res.Position);
                        if (n.Length() == 0)
                            continue;
                        n.Normalize();
                        _velocityBuffer[i] += elapsedSeconds*k*(dist - l)*n;
                    }
                }
            }

            var tmp = _velocityBuffer;
            _velocityBuffer = Velocities;
            Velocities = tmp;
        }

        private void WorldCollision(float elapsedSeconds)
        {
            for (var i = 0; i < ParticleCount; ++i)
            {
                var newPos = Particles[i] + Velocities[i]*elapsedSeconds;
                if (!_physics.IsEmpty(newPos))
                {
                    var res = _physics.Raycast(Particles[i], newPos);
                    if (!res.HasHit)
                    {
                        Velocities[i] = new Vector2((float) rng.NextDouble()*2 - 1, (float) rng.NextDouble()*2 - 1);
                        var newPos2 = Particles[i] + Velocities[i] * elapsedSeconds;
                        var res2 = _physics.Raycast(Particles[i], newPos2);
                        if (!_physics.IsEmpty(newPos2) || res2.HasHit)
                        {
                            Velocities[i] = Vector2.Zero;
                        }
                        continue;
                    }

                    res.Entity?.PhysicsBody.ApplyForce(Velocities[i]*200f);
                    Velocities[i] = (Velocities[i] - (Vector2.Dot(Velocities[i], res.Normal)) * res.Normal * 2) * 0.5f;
                }
            }
        }

        private int seldomCtr = 0;
        public void Step(World worldState, float elapsedSeconds)
        {
            if (GameSession.Renderer.WaterRenderer.NOWATER)
                return;
            
            if (++seldomCtr < 2)
                return;
            //Collision(elapsedSeconds);
            Viscosity(elapsedSeconds);
            Avoidance(elapsedSeconds);


            for (var i = 0; i < ParticleCount; ++i)
            {
                Velocities[i].Y += -9.81f*elapsedSeconds;
            }

            WorldCollision(elapsedSeconds);


            for (var i = 0; i < ParticleCount; ++i)
            {
                Particles[i] = Particles[i] + Velocities[i]*elapsedSeconds;
                var x = Math.Max(0, Math.Min(Particles[i].X,_width-1));
                var y = Math.Max(0, Math.Min(Particles[i].Y,_height-1));

                if (x != Particles[i].X)
                    Velocities[i].X = -Velocities[i].X*0.5f;

                if (y != Particles[i].Y)
                    Velocities[i].Y = -Velocities[i].Y*0.5f;

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