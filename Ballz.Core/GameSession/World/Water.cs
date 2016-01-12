using System;
using System.Collections.Generic;
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
            density = new BufferedArray<float>(ArrayWidth, ArrayHeight);
            force = new BufferedArray<Vector2>(ArrayWidth, ArrayHeight);
        }

        public int Width { get; }
        public int Height { get; }

        private int ArrayWidth => Width / cellSize;
        private int ArrayHeight => Height / cellSize;

        private BufferedArray<float> density;
        private BufferedArray<Vector2> force;

        private const int cellSize = 5;

        public float this[int x, int y]
        {
            get { return density[x/ cellSize, y/cellSize]; }
            set { density[x/ cellSize, y/ cellSize] = value; }
        }

        public void Initialize(World worldState)
        {
            for (var x = 0; x < ArrayWidth; ++x)
                for (var y = 0; y < ArrayHeight; ++y)
                {
                    var relativeY = y*1.0f/ ArrayHeight;
                    if (worldState.StaticGeometry.PublicShape.TerrainBitmap[x*cellSize, y * cellSize])
                        density[x, y] = 0;
                    else
                        density[x, y] = relativeY;

                    force[x, y] = new Vector2(0,0);
                }

            density.Unbuffer();
            force.Unbuffer();
        }



        public void Step(World worldState, float elapsedSeconds)
        {
            //Step 1: Add external force.

            for (var x = 0; x < ArrayWidth; ++x)
                for (var y = 0; y < ArrayHeight; ++y)
                {
                    if (worldState.StaticGeometry.PublicShape.TerrainBitmap[x * cellSize, y * cellSize])
                        continue;

                    force[x, y] = force[x, y] - new Vector2(0,1*elapsedSeconds);
                }

            force.Unbuffer();

            //Step 2: Advect

            for (var x = 0; x < ArrayWidth; ++x)
                for (var y = 0; y < ArrayHeight; ++y)
                {
                    if (worldState.StaticGeometry.PublicShape.TerrainBitmap[x * cellSize, y * cellSize])
                        continue;

                    var prePos = new Vector2(x,y) - force[x,y]*elapsedSeconds;
                    var left = (int) Math.Floor(prePos.X).Clamp(0, ArrayWidth - 1);
                    var bottom = (int) Math.Floor(prePos.Y).Clamp(0, ArrayHeight - 1);
                    var alpha = prePos.X - left;
                    var beta = prePos.Y - bottom;
                    var force1 = force[left, bottom];
                    var force2 = force[left, (bottom+1).Clamp(0, ArrayHeight - 1)];
                    var force3 = force[(left+1).Clamp(0, ArrayWidth - 1), bottom];
                    var force4 = force[(left + 1).Clamp(0, ArrayWidth - 1), (bottom + 1).Clamp(0, ArrayHeight - 1)];
                    var preForce = force1*(1 - alpha)*(1 - beta) 
                        + force2 * (1 - alpha) * beta
                        + force3 * alpha * (1 - beta)
                        + force4 * alpha * beta;

                    var density1 = density[left, bottom];
                    var density2 = density[left, (bottom + 1).Clamp(0, ArrayHeight - 1)];
                    var density3 = density[(left + 1).Clamp(0, ArrayWidth - 1), bottom];
                    var density4 = density[(left + 1).Clamp(0, ArrayWidth - 1), (bottom + 1).Clamp(0, ArrayHeight - 1)];
                    var preDensity = density1 * (1 - alpha) * (1 - beta)
                        + density2 * (1 - alpha) * beta
                        + density3 * alpha * (1 - beta)
                        + density4 * alpha * beta;

                    force[x, y] = preForce;
                    density[x, y] = preDensity;
                }

            force.Unbuffer();
            density.Unbuffer();

            //Step 3: 
            for (var x = 0; x < ArrayWidth; ++x)
                for (var y = 0; y < ArrayHeight; ++y)
                {
                    if (worldState.StaticGeometry.PublicShape.TerrainBitmap[x * cellSize, y * cellSize])
                        continue;

                    var hasAbove = y < ArrayHeight - 1 && !worldState.StaticGeometry.PublicShape.TerrainBitmap[x * cellSize, y * cellSize + cellSize];
                    var hasBelow = y > 0 && !worldState.StaticGeometry.PublicShape.TerrainBitmap[x * cellSize, y * cellSize - cellSize];
                    var hasRight = x < ArrayWidth - 1 && !worldState.StaticGeometry.PublicShape.TerrainBitmap[x * cellSize + cellSize, y * cellSize];
                    var hasLeft = x > 0 && !worldState.StaticGeometry.PublicShape.TerrainBitmap[x * cellSize - cellSize, y * cellSize]; ;
                    var neighbors = (hasAbove ? 1 : 0) + (hasBelow ? 1 : 0) + (hasRight ? 1 : 0) +
                                    (hasLeft ? 1 : 0);
                    var above = hasAbove ? density[x, y + 1] : 0;
                    var below = hasBelow ? density[x, y - 1] : 0;
                    var left = hasLeft ? density[x - 1, y] : 0;
                    var right = hasRight ? density[x + 1, y] : 0;

                    var aboveForce = hasAbove ? force[x, y + 1] : new Vector2(0, 0);
                    var belowForce = hasBelow ? force[x, y - 1] : new Vector2(0, 0);
                    var leftForce = hasLeft ? force[x - 1, y] : new Vector2(0, 0);
                    var rightForce = hasRight ? force[x + 1, y] : new Vector2(0, 0);

                    var diffused = (above + below + left + right + density[x, y]*neighbors)/(neighbors*2);
                    var diffusedForce = (aboveForce + belowForce + leftForce + rightForce + force[x, y] * neighbors) / (neighbors * 2);
                    density[x, y] = diffused;
                    force[x, y] = diffusedForce;
                }

            force.Unbuffer();
            density.Unbuffer();
        }
    }
}