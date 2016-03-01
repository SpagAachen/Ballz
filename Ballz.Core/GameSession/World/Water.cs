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
            colour = new BufferedArray<float>(ArrayWidth,ArrayHeight);
            force = new BufferedArray<Vector2>(ArrayWidth, ArrayHeight);
        }

        public int Width { get; }
        public int Height { get; }

        private int ArrayWidth => Width / CellSize;
        private int ArrayHeight => Height / CellSize;

        private BufferedArray<float> pressure;
        private BufferedArray<float> colour;
        private BufferedArray<Vector2> force;
        private BufferedArray<bool> particles; 

        public const int CellSize = 5;

        public float Pressure(int x, int y)
        {
            return pressure.Read(x / CellSize, y / CellSize);
        }

        public float Colour(int x, int y)
        {
            return colour.Read(x / CellSize, y / CellSize);
        }

        public Vector2 Velocity(int x, int y)
        {
            return force.Read(x / CellSize, y / CellSize);
        }

        public bool HasFluid(int x, int y)
        {
            return particles.Read(x, y);
        }

        public void Initialize(World worldState)
        {
            using (var forceI = force.Open())
            {
                using (var pressureI = pressure.Open())
                {
                    using (var colourI = colour.Open())
                    {
                        using (var particleI = particles.Open())
                        {
                            var rng = new Random();
                            for (var x = 0; x < ArrayWidth; ++x)
                                for (var y = 0; y < ArrayHeight; ++y)
                                {
                                    if (GetBoundary(worldState.StaticGeometry.PublicShape.TerrainBitmap, x, y) != 0)
                                    {
                                        forceI[x, y] = new Vector2(0, 0);
                                        pressureI[x, y] = 0;
                                        continue;
                                    }
                                    var relativeX = x*1.0f/ArrayWidth;
                                    var relativeY = y*1.0f/ArrayHeight;
                                    if (worldState.StaticGeometry.PublicShape.TerrainBitmap[x*CellSize, y*CellSize])
                                        pressureI[x, y] = 0;
                                    else
                                    {
                                        pressureI[x, y] = (float) rng.NextDouble();
                                        colourI[x, y] = (float) rng.NextDouble();
                                    }

                                    forceI[x, y] = new Vector2(0, 0);
                                }
                            for (var x = 0; x < ArrayWidth*CellSize; ++x)
                                for (var y = 0; y < ArrayHeight*CellSize; ++y)
                                {
                                    var val = (x - ArrayWidth*CellSize/2)*(x - ArrayWidth*CellSize/2) +
                                              (y - ArrayHeight*CellSize/2)*(y - ArrayHeight*CellSize/2) < 36;
                                    val &= GetBoundary(worldState.StaticGeometry.PublicShape.TerrainBitmap, x/5, y/5) ==
                                           0;
                                    particleI[x, y] = val;
                                }
                        }
                    }
                }
            }
        }

        private int GetBoundary(bool[,] terrain, int x, int y)
        {
            var bounds = 0;
            if (x == 0 || terrain[(x - 1) * CellSize + CellSize/2, y * CellSize + CellSize / 2])
                bounds |= 0x1;
            if (x == ArrayWidth - 1 || terrain[(x + 1) * CellSize + CellSize / 2, y * CellSize + CellSize / 2])
                bounds |= 0x2;
            if (y == 0 || terrain[x * CellSize + CellSize / 2, (y - 1) * CellSize + CellSize / 2])
                bounds |= 0x4;
            if (y == ArrayHeight - 1 || terrain[x * CellSize + CellSize / 2, (y + 1) * CellSize + CellSize / 2])
                bounds |= 0x8;
            if (terrain[x*CellSize + CellSize / 2, y*CellSize + CellSize / 2])
                bounds = 0xFF;
            return bounds;
        }
        
        private void Advect(World worldState, float elapsedSeconds)
        {
            using (var forceI = force.Open())
            {
                using (var colourI = colour.Open())
                {
                    for (var x = 0; x < ArrayWidth; ++x)
                        for (var y = 0; y < ArrayHeight; ++y)
                        {
                            if (GetBoundary(worldState.StaticGeometry.PublicShape.TerrainBitmap, x, y) != 0)
                            {
                                colourI[x, y] = colourI[x, y];
                                forceI[x, y] = forceI[x, y];
                                continue;
                            }

                            var curPos = new Vector2(x, y);
                            const int its = 5;
                            var curForce = forceI[x, y]*1f;
                            var curColour = colourI[x, y];
                            for (var i = 0; i < its; ++i)
                            {
                                curPos -= curForce*elapsedSeconds/its;
                                var left = (int) Math.Floor(curPos.X).Clamp(0, ArrayWidth - 1);
                                var bottom = (int) Math.Floor(curPos.Y).Clamp(0, ArrayHeight - 1);
                                var alpha = curPos.X - left;
                                var beta = curPos.Y - bottom;
                                var force1 = forceI[left, bottom];
                                var force2 = forceI[left, (bottom + 1).Clamp(0, ArrayHeight - 1)];
                                var force3 = forceI[(left + 1).Clamp(0, ArrayWidth - 1), bottom];
                                var force4 =
                                    forceI[(left + 1).Clamp(0, ArrayWidth - 1), (bottom + 1).Clamp(0, ArrayHeight - 1)];
                                var colour1 = colourI[left, bottom];
                                var colour2 = colourI[left, (bottom + 1).Clamp(0, ArrayHeight - 1)];
                                var colour3 = colourI[(left + 1).Clamp(0, ArrayWidth - 1), bottom];
                                var colour4 =
                                    colourI[(left + 1).Clamp(0, ArrayWidth - 1), (bottom + 1).Clamp(0, ArrayHeight - 1)];
                                curForce = force1*(1 - alpha)*(1 - beta)
                                           + force2*(1 - alpha)*beta
                                           + force3*alpha*(1 - beta)
                                           + force4*alpha*beta;
                                curColour = colour1*(1 - alpha)*(1 - beta)
                                            + colour2*(1 - alpha)*beta
                                            + colour3*alpha*(1 - beta)
                                            + colour4*alpha*beta;

                            }
                            forceI[x, y] = curForce;
                            colourI[x, y] = curColour;
                        }
                }
            }
        }

        private void AddForce(World worldState, float elapsedSeconds)
        {
            using (var forceI = force.Open())
            {
                for (var x = 0; x < ArrayWidth; ++x)
                    for (var y = 0; y < ArrayHeight; ++y)
                    {
                        if (GetBoundary(worldState.StaticGeometry.PublicShape.TerrainBitmap, x, y) != 0 || (x != ArrayWidth/2+5 || y != ArrayHeight/2))
                        {
                            forceI[x, y] = forceI[x, y];
                            continue;
                        }
                        forceI[x, y] = forceI[x, y] - new Vector2(0,1000f);
                    }
            }
        }

        private void Diffuse(World worldState, float elapsedSeconds)
        {
            using (var forceI = force.Open())
            {
                    for (var i = 0; i < 40; ++i)
                    {
                        for (var x = 0; x < ArrayWidth; ++x)
                            for (var y = 0; y < ArrayHeight; ++y)
                            {
                                if (GetBoundary(worldState.StaticGeometry.PublicShape.TerrainBitmap, x, y) != 0)
                                {
                                    forceI[x, y] = forceI[x, y];
                                    continue;
                                }
                                var alpha = 1/(1000*elapsedSeconds);
                                forceI[x, y] = (forceI[x - 1, y] + forceI[x + 1, y] + forceI[x, y - 1] +
                                                forceI[x, y + 1] +
                                                forceI[x, y]*alpha)/(4 + alpha);

                            }
                    }
            }
        }

        private void ComputePressure(World worldState, float elapsedSeconds)
        {
                for (var i = 0; i < 40; ++i)
            {
                using (var pressureI = pressure.Open())
                {
                    for (var x = 0; x < ArrayWidth; ++x)
                        for (var y = 0; y < ArrayHeight; ++y)
                        {
                            if (GetBoundary(worldState.StaticGeometry.PublicShape.TerrainBitmap, x, y) != 0)
                            {
                                continue;
                            }
                            var div = (force.Read(x + 1, y).X - force.Read(x - 1, y).X)/2 +
                                      (force.Read(x, y + 1).Y - force.Read(x, y - 1).Y)/2;
                            pressureI[x, y] = ((pressureI[x - 1, y] + pressureI[x + 1, y] + pressureI[x, y - 1] +
                                                pressureI[x, y + 1] - div)/4).Clamp(0, float.MaxValue);

                        }
                }
                using (var pressureI = pressure.Open())
                {
                    for (var x = 0; x < ArrayWidth; ++x)
                        for (var y = 0; y < ArrayHeight; ++y)
                        {
                            switch (GetBoundary(worldState.StaticGeometry.PublicShape.TerrainBitmap, x, y))
                            {
                                case 0:
                                    pressureI[x, y] = pressureI[x, y];
                                    break;
                                case 1:
                                    pressureI[x, y] = pressureI[x + 1, y];
                                    break;
                                case 2:
                                    pressureI[x, y] = pressureI[x - 1, y];
                                    break;
                                case 4:
                                    pressureI[x, y] = pressureI[x, y + 1];
                                    break;
                                case 8:
                                    pressureI[x, y] = pressureI[x, y - 1];
                                    break;
                                default:
                                    pressureI[x, y] = 0;
                                    break;
                            }
                        }
                    for (var x = 0; x < ArrayWidth; ++x)
                        for (var y = 0; y < ArrayHeight; ++y)
                        {
                            switch (GetBoundary(worldState.StaticGeometry.PublicShape.TerrainBitmap, x, y))
                            {
                                case 5:
                                    pressureI[x, y] = (pressureI[x + 1, y] + pressureI[x, y + 1])/2;
                                    break;
                                case 6:
                                    pressureI[x, y] = (pressureI[x - 1, y] + pressureI[x, y + 1])/2;
                                    break;
                                case 9:
                                    pressureI[x, y] = (pressureI[x + 1, y] + pressureI[x, y - 1])/2;
                                    break;
                                case 10:
                                    pressureI[x, y] = (pressureI[x - 1, y] + pressureI[x, y - 1])/2;
                                    break;
                                default:
                                    break;
                            }
                        }
                }
            }
            
        }

        private void SubtractPressureGradient(World worldState, float elapsedSeconds)
        {
            using (var forceI = force.Open())
            {
                for (var x = 0; x < ArrayWidth; ++x)
                    for (var y = 0; y < ArrayHeight; ++y)
                    {
                        if (GetBoundary(worldState.StaticGeometry.PublicShape.TerrainBitmap, x, y) != 0)
                        {
                            continue;
                        }
                        var grad = new Vector2((pressure.Read(x + 1, y) - pressure.Read(x - 1, y))/2,
                            (pressure.Read(x, y + 1) - pressure.Read(x, y - 1))/2);
                        forceI[x, y] -= grad;
                    }
            }

            using (var forceI = force.Open())
            {
                for (var x = 0; x < ArrayWidth; ++x)
                    for (var y = 0; y < ArrayHeight; ++y)
                    {
                        switch (GetBoundary(worldState.StaticGeometry.PublicShape.TerrainBitmap, x, y))
                        {
                            case 0:
                                forceI[x, y] = forceI[x, y];
                                break;
                            case 1:
                                forceI[x, y] = -forceI[x + 1, y];
                                break;
                            case 2:
                                forceI[x, y] = -forceI[x - 1, y];
                                break;
                            case 4:
                                forceI[x, y] = -forceI[x, y + 1];
                                break;
                            case 8:
                                forceI[x, y] = -forceI[x, y - 1];
                                break;
                            default:
                                forceI[x, y] = new Vector2(0, 0);
                                break;
                        }
                    }
                for (var x = 0; x < ArrayWidth; ++x)
                    for (var y = 0; y < ArrayHeight; ++y)
                    {
                        switch (GetBoundary(worldState.StaticGeometry.PublicShape.TerrainBitmap, x, y))
                        {
                            case 5:
                                forceI[x, y] = new Vector2(-forceI[x + 1, y].X, -forceI[x, y + 1].Y);
                                break;
                            case 6:
                                forceI[x, y] = new Vector2(-forceI[x - 1, y].X, -forceI[x, y + 1].Y);
                                break;
                            case 9:
                                forceI[x, y] = new Vector2(-forceI[x + 1, y].X, -forceI[x, y - 1].Y);
                                break;
                            case 10:
                                forceI[x, y] = new Vector2(-forceI[x - 1, y].X, -forceI[x, y - 1].Y);
                                break;
                            default:
                                break;
                        }
                    }
            }
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