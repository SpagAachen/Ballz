namespace Ballz.GameSession.World
{
   using System;
   using Microsoft.Xna.Framework;
   using Microsoft.Xna.Framework.Graphics;
   using Microsoft.Xna.Framework.Storage;


   /// <summary>
   /// Terrain represents the Terrain for our Gameworld.
   /// </summary>
   public class Terrain
   {
      public Terrain ()
      {
      }


      public Texture2D ExtractSignedDistanceField (Texture2D terrainTex)
      {
         Rectangle terrainSize = terrainTex.Bounds;
         Color[] pixels = new Color[terrainSize.Width * terrainSize.Height];
         terrainTex.GetData<Color> (pixels);


         Color[] sdfPixels = new Color[terrainSize.Width * terrainSize.Height];

         // Left-to-right
         for (int y = 0; y < terrainSize.Height; ++y) {
            for (int x = 0; x < terrainSize.Width; ++x) {
					
               Color curPixel = pixels [y * terrainSize.Width + x];
               bool dirt = curPixel.R == curPixel.G && curPixel.G == curPixel.B && curPixel.R == 255;
               int value = 0;

               if (x == 0) { // border case
                  if (dirt)
                     value = 127;
                  else {
                     value = -128;
                  }
               } else {
                  int valueOnTheLeft = sdfPixels [y * terrainSize.Width + x - 1].R - 128;
                  if (dirt)
                     value = Math.Max (0, Math.Min (valueOnTheLeft + 1, 127));
                  else {
                     value = Math.Min (-1, Math.Max (valueOnTheLeft - 1, -128));
                  }
               }
						
               value += 128;
               sdfPixels [y * terrainSize.Width + x] = new Color (value, value, value);
            }
         }

         // Right-to-left
         for (int y = 0; y < terrainSize.Height; ++y) {
            for (int x = terrainSize.Width - 1; x >= 0; --x) {

               Color curPixel = pixels [y * terrainSize.Width + x];
               bool dirt = curPixel.R == curPixel.G && curPixel.G == curPixel.B && curPixel.R == 255;
               int value = 0;

               if (x == terrainSize.Width - 1) { // border case
                  if (dirt)
                     value = 127;
                  else {
                     value = -128;
                  }
               } else {
                  int valueOnTheRight = sdfPixels [y * terrainSize.Width + x + 1].R - 128;
                  if (dirt)
                     value = Math.Max (0, Math.Min (valueOnTheRight + 1, 127));
                  else {
                     value = Math.Min (-1, Math.Max (valueOnTheRight - 1, -128));
                  }
               }

               value += 128;
               int lastValue = sdfPixels [y * terrainSize.Width + x].R;
               int newValue = dirt ? Math.Min (lastValue, value) : Math.Max (lastValue, value);
               sdfPixels [y * terrainSize.Width + x] = new Color (newValue, newValue, newValue);

            }
         }


         // Top-to-bottom
         for (int y = 0; y < terrainSize.Height; ++y) {
            for (int x = 0; x < terrainSize.Width; ++x) {

               Color curPixel = pixels [y * terrainSize.Width + x];
               bool dirt = curPixel.R == curPixel.G && curPixel.G == curPixel.B && curPixel.R == 255;
               int value = 0;

               if (y == 0) { // border case
                  if (dirt)
                     value = 127;
                  else {
                     value = -128;
                  }
               } else {
                  int valueOnTop = sdfPixels [(y - 1) * terrainSize.Width + x].R - 128;
                  if (dirt)
                     value = Math.Max (0, Math.Min (valueOnTop + 1, 127));
                  else {
                     value = Math.Min (-1, Math.Max (valueOnTop - 1, -128));
                  }
               }

               value += 128;
               int lastValue = sdfPixels [y * terrainSize.Width + x].R;
               int newValue = dirt ? Math.Min (lastValue, value) : Math.Max (lastValue, value);
               sdfPixels [y * terrainSize.Width + x] = new Color (newValue, newValue, newValue);

            }
         }


         // Bottom-to-top
         for (int y = terrainSize.Height - 1; y >= 0; --y) {
            for (int x = 0; x < terrainSize.Width; ++x) {

               Color curPixel = pixels [y * terrainSize.Width + x];
               bool dirt = curPixel.R == curPixel.G && curPixel.G == curPixel.B && curPixel.R == 255;
               int value = 0;

               if (y == terrainSize.Height - 1) { // border case
                  if (dirt)
                     value = 127;
                  else {
                     value = -128;
                  }
               } else {
                  int valueOnBottom = sdfPixels [(y + 1) * terrainSize.Width + x].R - 128;
                  if (dirt)
                     value = Math.Max (0, Math.Min (valueOnBottom + 1, 127));
                  else {
                     value = Math.Min (-1, Math.Max (valueOnBottom - 1, -128));
                  }
               }
						
               value += 128;
               int lastValue = sdfPixels [y * terrainSize.Width + x].R;
               int newValue = dirt ? Math.Min (lastValue, value) : Math.Max (lastValue, value);
               sdfPixels [y * terrainSize.Width + x] = new Color (newValue, newValue, newValue);
            }
         }



         // Redraw contour
         for (int y = 0; y < terrainSize.Height; ++y) {
            for (int x = 0; x < terrainSize.Width; ++x) {

               Color curPixel = sdfPixels [y * terrainSize.Width + x];
               if (curPixel.R == 128)
                  sdfPixels [y * terrainSize.Width + x] = new Color (255, 0, 0);

            }
         }

         Texture2D sdf = new Texture2D (Ballz.The ().graphics.GraphicsDevice, 
                            terrainSize.Width, terrainSize.Height);
         sdf.SetData (sdfPixels);




         return sdf;
      }
   }
}

