using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ballz.GameSession.World
{
    /// <summary>
    ///     Terrain represents the Terrain for our Gameworld.
    /// </summary>
    public class Terrain
    {
        public Texture2D ExtractSignedDistanceField(Texture2D terrainTex)
        {
            var terrainSize = terrainTex.Bounds;
            var pixels = new Color[terrainSize.Width*terrainSize.Height];
            terrainTex.GetData(pixels);


            var sdfPixels = new Color[terrainSize.Width*terrainSize.Height];

            // Left-to-right
            for (var y = 0; y < terrainSize.Height; ++y)
            {
                for (var x = 0; x < terrainSize.Width; ++x)
                {
                    var curPixel = pixels[y*terrainSize.Width + x];
                    var dirt = curPixel.R == curPixel.G && curPixel.G == curPixel.B && curPixel.R == 255;
                    int value;

                    if (x == 0)
                    {
                        // border case
                        if (dirt)
                            value = 127;
                        else
                        {
                            value = -128;
                        }
                    }
                    else
                    {
                        var valueOnTheLeft = sdfPixels[y*terrainSize.Width + x - 1].R - 128;
                        value = dirt 
                            ? Math.Max(0, Math.Min(valueOnTheLeft + 1, 127)) 
                            : Math.Min(-1, Math.Max(valueOnTheLeft - 1, -128));
                    }

                    value += 128;
                    sdfPixels[y*terrainSize.Width + x] = new Color(value, value, value);
                }
            }

            // Right-to-left
            for (var y = 0; y < terrainSize.Height; ++y)
            {
                for (var x = terrainSize.Width - 1; x >= 0; --x)
                {
                    var curPixel = pixels[y*terrainSize.Width + x];
                    var dirt = curPixel.R == curPixel.G && curPixel.G == curPixel.B && curPixel.R == 255;
                    int value;

                    if (x == terrainSize.Width - 1)
                    {
                        // border case
                        if (dirt)
                            value = 127;
                        else
                        {
                            value = -128;
                        }
                    }
                    else
                    {
                        var valueOnTheRight = sdfPixels[y*terrainSize.Width + x + 1].R - 128;
                        value = dirt 
                            ? Math.Max(0, Math.Min(valueOnTheRight + 1, 127)) 
                            : Math.Min(-1, Math.Max(valueOnTheRight - 1, -128));
                    }

                    value += 128;
                    int lastValue = sdfPixels[y*terrainSize.Width + x].R;
                    var newValue = dirt ? Math.Min(lastValue, value) : Math.Max(lastValue, value);
                    sdfPixels[y*terrainSize.Width + x] = new Color(newValue, newValue, newValue);
                }
            }


            // Top-to-bottom
            for (var y = 0; y < terrainSize.Height; ++y)
            {
                for (var x = 0; x < terrainSize.Width; ++x)
                {
                    var curPixel = pixels[y*terrainSize.Width + x];
                    var dirt = curPixel.R == curPixel.G && curPixel.G == curPixel.B && curPixel.R == 255;
                    int value;

                    if (y == 0)
                    {
                        // border case
                        if (dirt)
                            value = 127;
                        else
                        {
                            value = -128;
                        }
                    }
                    else
                    {
                        var valueOnTop = sdfPixels[(y - 1) * terrainSize.Width + x].R - 128;
                        value = dirt 
                            ? Math.Max(0, Math.Min(valueOnTop + 1, 127)) 
                            : Math.Min(-1, Math.Max(valueOnTop - 1, -128));
                    }

                    value += 128;
                    int lastValue = sdfPixels[y*terrainSize.Width + x].R;
                    var newValue = dirt ? Math.Min(lastValue, value) : Math.Max(lastValue, value);
                    sdfPixels[y*terrainSize.Width + x] = new Color(newValue, newValue, newValue);
                }
            }


            // Bottom-to-top
            for (var y = terrainSize.Height - 1; y >= 0; --y)
            {
                for (var x = 0; x < terrainSize.Width; ++x)
                {
                    var curPixel = pixels[y*terrainSize.Width + x];
                    var dirt = curPixel.R == curPixel.G && curPixel.G == curPixel.B && curPixel.R == 255;
                    int value;

                    if (y == terrainSize.Height - 1)
                    {
                        // border case
                        if (dirt)
                            value = 127;
                        else
                        {
                            value = -128;
                        }
                    }
                    else
                    {
                        var valueOnBottom = sdfPixels[(y + 1) * terrainSize.Width + x].R - 128;
                        value = dirt 
                            ? Math.Max(0, Math.Min(valueOnBottom + 1, 127)) 
                            : Math.Min(-1, Math.Max(valueOnBottom - 1, -128));
                    }

                    value += 128;
                    int lastValue = sdfPixels[y*terrainSize.Width + x].R;
                    var newValue = dirt ? Math.Min(lastValue, value) : Math.Max(lastValue, value);
                    sdfPixels[y*terrainSize.Width + x] = new Color(newValue, newValue, newValue);
                }
            }


            // Redraw contour
            for (var y = 0; y < terrainSize.Height; ++y)
            {
                for (var x = 0; x < terrainSize.Width; ++x)
                {
                    var curPixel = sdfPixels[y*terrainSize.Width + x];
                    if (curPixel.R == 128)
                        sdfPixels[y*terrainSize.Width + x] = new Color(255, 0, 0);
                }
            }

            var sdf = new Texture2D(
                Ballz.The().Graphics.GraphicsDevice,
                terrainSize.Width,
                terrainSize.Height);
            sdf.SetData(sdfPixels);


            return sdf;
        }
    }
}