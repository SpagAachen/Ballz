using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ballz.GameSession.World
{
    public class Water
    {
        public Water(int width, int height)
        {
            Width = width;
            Height = height;
            density = new float[Width,Height];
        }

        public int Width { get; }
        public int Height { get; }

        private readonly float[,] density;

        public float this[int x, int y]
        {
            get { return density[x, y]; }

            set { density[x, y] = value; }
        }
        
        public void Step(World worldState, float elapsedSeconds)
        {
            for (var x = 0; x < Width; ++x)
                for (var y = 0; y < Height; ++y)
                {
                    if (worldState.StaticGeometry.publicShape.terrainBitmap[x, y])
                        density[x, y] = 0;
                    else
                        density[x, y] = 1;
                }
        }
    }
}
