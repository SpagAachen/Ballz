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
        public Water(int width, int height)
        {
            Width = width;
            Height = height;
            pressure = new BufferedArray<float>(ArrayWidth, ArrayHeight);
            force = new BufferedArray<Vector2>(ArrayWidth, ArrayHeight);
        }

        public int Width { get; }
        public int Height { get; }

        private int ArrayWidth => Width / cellSize;
        private int ArrayHeight => Height / cellSize;

        private BufferedArray<float> pressure;
        private BufferedArray<Vector2> force;

        private const int cellSize = 5;

        public float this[int x, int y]
        {
            get { return pressure[x/ cellSize, y/cellSize]; }
            set { pressure[x/ cellSize, y/ cellSize] = value; }
        }

        public Vector2 Velocity(int x, int y)
        {
            return force[x, y];
        }

        public void Initialize(World worldState)
        {
            for (var x = 0; x < ArrayWidth; ++x)
                for (var y = 0; y < ArrayHeight; ++y)
                {
                    var relativeY = y*1.0f/ ArrayHeight;
                    if (worldState.StaticGeometry.PublicShape.TerrainBitmap[x*cellSize, y * cellSize])
                        pressure[x, y] = 0;
                    else
                        pressure[x, y] = relativeY;

                    force[x, y] = new Vector2(0,0);
                }

            pressure.Unbuffer();
            force.Unbuffer();
        }

        private void Advect(World worldState, float elapsedSeconds)
        {
            for (var x = 0; x < ArrayWidth; ++x)
                for (var y = 0; y < ArrayHeight; ++y)
                {
                    if (worldState.StaticGeometry.PublicShape.TerrainBitmap[x * cellSize, y * cellSize])
                    {
                        force[x, y] = force[x, y];
                        continue;
                    }

                    var prePos = new Vector2(x, y) - force[x, y] * elapsedSeconds * 0.001f;
                    var left = (int)Math.Floor(prePos.X).Clamp(0, ArrayWidth - 1);
                    var bottom = (int)Math.Floor(prePos.Y).Clamp(0, ArrayHeight - 1);
                    var alpha = prePos.X - left;
                    var beta = prePos.Y - bottom;
                    var force1 = force[left, bottom];
                    var force2 = force[left, (bottom + 1).Clamp(0, ArrayHeight - 1)];
                    var force3 = force[(left + 1).Clamp(0, ArrayWidth - 1), bottom];
                    var force4 = force[(left + 1).Clamp(0, ArrayWidth - 1), (bottom + 1).Clamp(0, ArrayHeight - 1)];
                    var preForce = force1 * (1 - alpha) * (1 - beta)
                        + force2 * (1 - alpha) * beta
                        + force3 * alpha * (1 - beta)
                        + force4 * alpha * beta;

                    force[x, y] = preForce;
                }

            force.Unbuffer();
        }

        private void AddForce(World worldState, float elapsedSeconds)
        {
            for (var x = 0; x < ArrayWidth; ++x)
                for (var y = 0; y < ArrayHeight; ++y)
                {
                    if (worldState.StaticGeometry.PublicShape.TerrainBitmap[x * cellSize, y * cellSize])
                    {
                        force[x, y] = force[x, y];
                        continue;
                    }

                    force[x, y] = force[x, y] - new Vector2(0, 1 * elapsedSeconds);
                }

            force.Unbuffer();
        }

        private void Diffuse(World worldState, float elapsedSeconds)
        {
            for (var i = 0; i < 40; ++i)
            {
                for (var x = 0; x < ArrayWidth; ++x)
                    for (var y = 0; y < ArrayHeight; ++y)
                    {
                        if (x == 0 || y == 0 || x == ArrayWidth - 1 || y == ArrayHeight - 1)
                        {
                            continue;
                        }
                        if (worldState.StaticGeometry.PublicShape.TerrainBitmap[x * cellSize, y * cellSize])
                        {
                            force[x, y] = force[x, y];
                            continue;
                        }
                        var alpha = 1/(1000*elapsedSeconds);
                        force[x, y] = (force[x - 1, y] + force[x + 1, y] + force[x, y - 1] + force[x, y + 1] + force[x,y]*alpha) / (4+alpha);

                    }
                for (var x = 0; x < ArrayWidth; ++x)
                {
                    pressure[x, 0] = pressure[x, 1];
                    pressure[x, ArrayHeight - 1] = pressure[x, ArrayHeight - 2];
                }
                for (var y = 0; y < ArrayHeight; ++y)
                {
                    pressure[0, y] = pressure[1, y];
                    pressure[ArrayWidth - 1, y] = pressure[ArrayWidth - 2, y];
                }
                pressure.Unbuffer();
            }
        }

        private void ComputePressure(World worldState, float elapsedSeconds)
        {
            for (var i = 0; i < 40; ++i)
            {
                for (var x = 0; x < ArrayWidth; ++x)
                    for (var y = 0; y < ArrayHeight; ++y)
                    {
                        if (x == 0 || y == 0 || x == ArrayWidth - 1 || y == ArrayHeight - 1)
                        {
                            continue;
                        }
                        if (worldState.StaticGeometry.PublicShape.TerrainBitmap[x*cellSize, y*cellSize])
                        {
                            pressure[x, y] = pressure[x, y];
                            continue;
                        }
                        var div = (force[x + 1, y].X - force[x - 1, y].X)/2 + (force[x, y + 1].Y - force[x, y - 1].Y)/2;
                        pressure[x, y] = (pressure[x - 1, y] + pressure[x + 1, y] + pressure[x, y - 1] +
                                         pressure[x, y + 1] - div)/4;

                    }
                for (var x = 0; x < ArrayWidth; ++x)
                {
                    pressure[x, 0] = pressure[x, 1];
                    pressure[x, ArrayHeight - 1] = pressure[x, ArrayHeight - 2];
                }
                for (var y = 0; y < ArrayHeight; ++y)
                {
                    pressure[0, y] = pressure[1, y];
                    pressure[ArrayWidth - 1, y] = pressure[ArrayWidth - 2, y];
                }
                pressure.Unbuffer();
            }
        }

        private void SubtractPressureGradient(World worldState, float elapsedSeconds)
        {
            for (var x = 0; x < ArrayWidth; ++x)
                for (var y = 0; y < ArrayHeight; ++y)
                {
                    if (x == 0 || y == 0 || x == ArrayWidth - 1 || y == ArrayHeight - 1)
                    {
                        continue;
                    }
                    var grad = new Vector2((pressure[x + 1, y] - pressure[x - 1, y])/2, (pressure[x, y + 1] - pressure[x, y - 1])/2);
                    force[x, y] -= grad;
                }

            for (var x = 0; x < ArrayWidth; ++x)
            {
                force[x, 0] = -force[x, 1];
                force[x, ArrayHeight - 1] = -force[x, ArrayHeight - 2];
            }
            for (var y = 0; y < ArrayHeight; ++y)
            {
                force[0, y] = -force[1, y];
                force[ArrayWidth - 1, y] = -force[ArrayWidth - 2, y];
            }
            force.Unbuffer();
        }

        public void Step(World worldState, float elapsedSeconds)
        {
            Advect(worldState, elapsedSeconds);
            Diffuse(worldState,elapsedSeconds);
            AddForce(worldState, elapsedSeconds);
            ComputePressure(worldState, elapsedSeconds);
            SubtractPressureGradient(worldState, elapsedSeconds);
        }
    }
}