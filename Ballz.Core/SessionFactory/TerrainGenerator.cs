using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ballz.GameSession.World;
using Ballz.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Ballz.SessionFactory
{
    internal class TerrainGenerator
    {
        static readonly Random Rng = new Random();
        static readonly PerlinNoise noise = new PerlinNoise();

        private static float weightedNoise(float f, int height, int x, int y)
        {
            var p = 1 - y*1.0f/height;
            var noise = TerrainGenerator.noise.Noise(f*x/100f, f*y/100f);

            if (p <= 0.5)
            {
                var fp = p*2;
                return fp*noise + (1-fp);
            }
            var fp2 = (1-p) * 2;
            return fp2 * noise + (fp2 - 1);
        }

        private static Terrain.TerrainType[,] GenerateArray(int width, int height)
        {
            var types = new Terrain.TerrainType[height, width];
            
            for (var x = 0; x < width; ++x)
                for (var y = height - 20; y < height - 10; ++y)
                    types[y, x] = Terrain.TerrainType.Water;


            for (var x = 0; x < width; ++x)
                for (var y = 0; y < height - 10; ++y)
                    if (weightedNoise(2f, height,x,y) > 0)
                        types[y, x] = Terrain.TerrainType.Earth;
                    else if (weightedNoise(2f, height, x, y) > -0.1)
                        types[y, x] = Terrain.TerrainType.Sand;
                    else if (weightedNoise(2f, height, x, y) > -0.2)
                        types[y, x] = Terrain.TerrainType.Water;

            for (var x = 0; x < width; ++x)
                for (var y = height - 10; y < height; ++y)
                    types[y,x] = Terrain.TerrainType.Stone;

            for (var i = 0; i < 4; ++i)
                while (true)
                {
                    var x = Rng.Next(width);
                    var y = Rng.Next(height);
                    if (types[y, x] != Terrain.TerrainType.Air)
                        continue;
                    types[y, x] = Terrain.TerrainType.VegetationSeed;
                    break;
                }
            
            return types;
        }
        public static Texture2D GenerateTerrain(int width, int height)
        {
            var types = GenerateArray(width, height);

            var typeData = types.Cast<Terrain.TerrainType>();

            var colorData = typeData.Select(t => {
                switch (t)
                {
                    case Terrain.TerrainType.Air:
                        return Color.Black;
                    case Terrain.TerrainType.Earth:
                        return new Color(127, 64, 0);
                    case Terrain.TerrainType.Stone:
                        return new Color(127, 127, 127);
                    case Terrain.TerrainType.Sand:
                        return Color.Yellow;
                    case Terrain.TerrainType.Water:
                        return Color.Blue;
                    case Terrain.TerrainType.VegetationSeed:
                        return Color.Lime;
                    case Terrain.TerrainType.SpawnPoint:
                        return Color.Red;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(t), t, null);
                }
            });

            var t2D = new Texture2D(Ballz.The().GraphicsDevice, width, height);
            t2D.SetData(colorData.ToArray());

            return t2D;
        }
    }
}
