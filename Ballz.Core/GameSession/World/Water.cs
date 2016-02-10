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
            return force[x/cellSize, y/cellSize];
        }
        
        public Vector2 LerpVelocity(float x, float y)
        {
            var left = (int)Math.Floor(x/cellSize).Clamp(0, ArrayWidth - 1);
            var bottom = (int)Math.Floor(y/cellSize).Clamp(0, ArrayHeight - 1);
            var alpha = x / cellSize - left;
            var beta = y / cellSize - bottom;
            var force1 = force[left, bottom];
            var force2 = force[left, (bottom + 1).Clamp(0, ArrayHeight - 1)];
            var force3 = force[(left + 1).Clamp(0, ArrayWidth - 1), bottom];
            var force4 = force[(left + 1).Clamp(0, ArrayWidth - 1), (bottom + 1).Clamp(0, ArrayHeight - 1)];
            return force1 * (1 - alpha) * (1 - beta)
                + force2 * (1 - alpha) * beta
                + force3 * alpha * (1 - beta)
                + force4 * alpha * beta;
        }

        public void Initialize(World worldState)
        {
            var rng = new Random(); 
            for (var x = 0; x < ArrayWidth; ++x)
                for (var y = 0; y < ArrayHeight; ++y)
                {
                    if (getBoundary(worldState.StaticGeometry.PublicShape.TerrainBitmap, x, y) != 0)
                    {
                        force[x, y] = new Vector2(0, 0);
                        pressure[x, y] = 0;
                        continue;
                    }
                    var relativeX = x * 1.0f / ArrayWidth;
                    var relativeY = y*1.0f/ ArrayHeight;
                    if (worldState.StaticGeometry.PublicShape.TerrainBitmap[x*cellSize, y * cellSize])
                        pressure[x, y] = 0;
                    else
                        pressure[x, y] = (float) rng.NextDouble();
                    
                    force[x, y] = new Vector2(0,0);
                }

            pressure.Unbuffer();
            force.Unbuffer();
        }

        private int getBoundary(bool[,] terrain, int x, int y)
        {
            var bounds = 0;
            if (x == 0 || terrain[(x - 1) * cellSize + cellSize/2, y * cellSize + cellSize / 2])
                bounds |= 0x1;
            if (x == ArrayWidth - 1 || terrain[(x + 1) * cellSize + cellSize / 2, y * cellSize + cellSize / 2])
                bounds |= 0x2;
            if (y == 0 || terrain[x * cellSize + cellSize / 2, (y - 1) * cellSize + cellSize / 2])
                bounds |= 0x4;
            if (y == ArrayHeight - 1 || terrain[x * cellSize + cellSize / 2, (y + 1) * cellSize + cellSize / 2])
                bounds |= 0x8;
            if (terrain[x*cellSize + cellSize / 2, y*cellSize + cellSize / 2])
                bounds = 0xFF;
            return bounds;
        }
        
        private void Advect(World worldState, float elapsedSeconds)
        {
            for (var x = 0; x < ArrayWidth; ++x)
                for (var y = 0; y < ArrayHeight; ++y)
                {
                    if (getBoundary(worldState.StaticGeometry.PublicShape.TerrainBitmap,x,y) != 0)
                    {
                        force[x, y] = force[x, y];
                        continue;
                    }

                    var curPos = new Vector2(x,y);
                    const int its = 5;
                    var curForce = force[x, y]*1f;
                    for (var i = 0; i < its; ++i)
                    {
                        curPos -= curForce * elapsedSeconds / its;
                        var left = (int)Math.Floor(curPos.X).Clamp(0, ArrayWidth - 1);
                        var bottom = (int)Math.Floor(curPos.Y).Clamp(0, ArrayHeight - 1);
                        var alpha = curPos.X - left;
                        var beta = curPos.Y - bottom;
                        var force1 = force[left, bottom];
                        var force2 = force[left, (bottom + 1).Clamp(0, ArrayHeight - 1)];
                        var force3 = force[(left + 1).Clamp(0, ArrayWidth - 1), bottom];
                        var force4 = force[(left + 1).Clamp(0, ArrayWidth - 1), (bottom + 1).Clamp(0, ArrayHeight - 1)];
                        curForce = force1 * (1 - alpha) * (1 - beta)
                            + force2 * (1 - alpha) * beta
                            + force3 * alpha * (1 - beta)
                            + force4 * alpha * beta;

                    }
                    force[x, y] = curForce;

                }

            force.Unbuffer();
        }

        private void AddForce(World worldState, float elapsedSeconds)
        {
            for (var x = 0; x < ArrayWidth; ++x)
                for (var y = 0; y < ArrayHeight; ++y)
                {
                    if (getBoundary(worldState.StaticGeometry.PublicShape.TerrainBitmap, x, y) != 0)
                    {
                        force[x, y] = force[x, y];
                        continue;
                    }

                    force[x, y] = force[x, y] - new Vector2(0, elapsedSeconds*0.01f);
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
                        if (getBoundary(worldState.StaticGeometry.PublicShape.TerrainBitmap, x, y) != 0)
                        {
                            force[x, y] = force[x, y];
                            continue;
                        }
                        var alpha = 1/(1000*elapsedSeconds);
                        force[x, y] = (force[x - 1, y] + force[x + 1, y] + force[x, y - 1] + force[x, y + 1] + force[x,y]*alpha) / (4+alpha);

                    }
                force.Unbuffer();
            }
        }

        private void ComputePressure(World worldState, float elapsedSeconds)
        {
            for (var i = 0; i < 40; ++i)
            {
                for (var x = 0; x < ArrayWidth; ++x)
                    for (var y = 0; y < ArrayHeight; ++y)
                    {
                        if (getBoundary(worldState.StaticGeometry.PublicShape.TerrainBitmap, x, y) != 0)
                        {
                            continue;
                        }
                        var div = (force[x + 1, y].X - force[x - 1, y].X)/2 + (force[x, y + 1].Y - force[x, y - 1].Y)/2;
                        pressure[x, y] = ((pressure[x - 1, y] + pressure[x + 1, y] + pressure[x, y - 1] +
                                         pressure[x, y + 1] - div)/4).Clamp(0,float.MaxValue);

                    }
                pressure.Unbuffer();
                for (var x = 0; x < ArrayWidth; ++x)
                    for (var y = 0; y < ArrayHeight; ++y)
                    {
                        switch (getBoundary(worldState.StaticGeometry.PublicShape.TerrainBitmap, x, y))
                        {
                            case 0:
                                pressure[x, y] = pressure[x, y];
                                break;
                            case 1:
                                pressure[x, y] = pressure[x + 1, y];
                                break;
                            case 2:
                                pressure[x, y] = pressure[x - 1, y];
                                break;
                            case 4:
                                pressure[x, y] = pressure[x, y + 1];
                                break;
                            case 8:
                                pressure[x, y] = pressure[x, y - 1];
                                break;
                            default:
                                pressure[x, y] = 0;
                                break;
                        }
                    }
                for (var x = 0; x < ArrayWidth; ++x)
                    for (var y = 0; y < ArrayHeight; ++y)
                    {
                        switch (getBoundary(worldState.StaticGeometry.PublicShape.TerrainBitmap, x, y))
                        {
                            case 5:
                                pressure[x, y] = (pressure[x + 1, y] + pressure[x, y + 1]) / 2;
                                break;
                            case 6:
                                pressure[x, y] = (pressure[x - 1, y] + pressure[x, y + 1]) / 2;
                                break;
                            case 9:
                                pressure[x, y] = (pressure[x + 1, y] + pressure[x, y - 1]) / 2;
                                break;
                            case 10:
                                pressure[x, y] = (pressure[x - 1, y] + pressure[x, y - 1]) / 2;
                                break;
                            default:
                                break;
                        }
                    }
                pressure.Unbuffer();
            }
        }

        private void SubtractPressureGradient(World worldState, float elapsedSeconds)
        {
            for (var x = 0; x < ArrayWidth; ++x)
                for (var y = 0; y < ArrayHeight; ++y)
                {
                    if (getBoundary(worldState.StaticGeometry.PublicShape.TerrainBitmap, x, y) != 0)
                    {
                        continue;
                    }
                    var grad = new Vector2((pressure[x + 1, y] - pressure[x - 1, y])/2, (pressure[x, y + 1] - pressure[x, y - 1])/2);
                    force[x, y] -= grad;
                }
            force.Unbuffer();

            for (var x = 0; x < ArrayWidth; ++x)
                for (var y = 0; y < ArrayHeight; ++y)
                {
                    switch (getBoundary(worldState.StaticGeometry.PublicShape.TerrainBitmap, x, y))
                    {
                        case 0:
                            force[x, y] = force[x, y];
                            break;
                        case 1:
                            force[x, y] = -force[x + 1, y];
                            break;
                        case 2:
                            force[x, y] = -force[x - 1, y];
                            break;
                        case 4:
                            force[x, y] = -force[x, y + 1];
                            break;
                        case 8:
                            force[x, y] = -force[x, y - 1];
                            break;
                        default:
                            force[x, y] = new Vector2(0,0);
                            break;
                    }
                }
            /*for (var x = 0; x < ArrayWidth; ++x)
                for (var y = 0; y < ArrayHeight; ++y)
                {
                    switch (getBoundary(worldState.StaticGeometry.PublicShape.TerrainBitmap, x, y))
                    {
                        case 5:
                            force[x, y] = new Vector2(-force[x + 1, y].X, -force[x, y + 1].Y);
                            break;
                        case 6:
                            force[x, y] = new Vector2(-force[x - 1, y].X, -force[x, y + 1].Y);
                            break;
                        case 9:
                            force[x, y] = new Vector2(-force[x + 1, y].X, -force[x, y - 1].Y);
                            break;
                        case 10:
                            force[x, y] = new Vector2(-force[x - 1, y].X, -force[x, y - 1].Y);
                            break;
                        default:
                            break;
                    }
                }*/
            force.Unbuffer();
        }

        public void Step(World worldState, float elapsedSeconds)
        {
            Advect(worldState, elapsedSeconds);
            Diffuse(worldState, elapsedSeconds);
            AddForce(worldState, elapsedSeconds);
            ComputePressure(worldState, elapsedSeconds);
            SubtractPressureGradient(worldState, elapsedSeconds);
        }
    }
}