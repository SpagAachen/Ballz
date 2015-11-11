using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ballz.GameSession.World
{
    /// <summary>
    ///     Terrain represents the Terrain for our Gameworld.
    /// </summary>
    public class Terrain
    {
		public List<Vector2> outline = new List<Vector2> ();

		private Texture2D terrainData = null;

		// We might need that later on...
		private Texture2D terrainSDF = null;

		public Terrain (Texture2D terrainTexture)
		{
			terrainData = terrainTexture;
			terrainSDF = ExtractSignedDistanceField (terrainData);
		}

		public void ExtractOutline()
		{
			int Width = terrainSDF.Width;
			int Height = terrainSDF.Height;

			bool[,] values = new bool[Width, Height];

			Color[] sdfpixels = new Color[Width * Height];
			terrainSDF.GetData<Color> (sdfpixels);


			for (int y = 0; y < Height; ++y) {
				for (int x = 0; x < Width; ++x) {

					Color curPixel = sdfpixels [y * Width + x];
					bool dirt = 
						curPixel.R == curPixel.G &&
						curPixel.G == curPixel.B &&
						curPixel.R == 128;
					
					if (dirt)
						values [x, Height-y-1] = true;
				}
			}

			outline.Clear ();

			Physics2DDotNet.Shapes.ArrayBitmap ab = new Physics2DDotNet.Shapes.ArrayBitmap(values);
			AdvanceMath.Vector2D[] geometry = Physics2DDotNet.Shapes.VertexHelper.CreateFromBitmap(ab);
			foreach(AdvanceMath.Vector2D vec in geometry)
			{
				outline.Add(new Vector2(vec.X,vec.Y));
			}
		}


		public struct IntVector2  {
			public int x, y;

			public IntVector2 (int x, int y) {
				this.x = x;
				this.y = y;
			}   
		}


		private void sdfOneIter(bool[] dirtPixels, float[] sdfPixels, int width, int height, IntVector2 dir)
		{
			// TODO(ks): allow other sdf-vectors besides -1/0/1 combinations

			var from = new IntVector2 (dir.x > 0 ? 0 : width-1, dir.y > 0 ? 0 : height-1);
			var to = new IntVector2 (dir.x <= 0 ? 0 : width-1, dir.y <= 0 ? 0 : height-1);

			var offset = new IntVector2 (0, 0);
			var size = new IntVector2 (0, 0);
			if (from.x < to.x) {
				size.x = to.x + 1;
				offset.x = 1;
			} else {
				size.x = from.x + 1;
				offset.x = -1;
			}
			if (from.y < to.y) {
				size.y = to.y + 1;
				offset.y = 1;
			} else {
				size.y = from.y + 1;
				offset.y = -1;
			}


			int borderX = dir.x == 0 ? -1 : dir.x > 0 ? Math.Min(from.x, to.x) : Math.Max(from.x, to.x);
			int borderY = dir.y == 0 ? -1 : dir.y > 0 ? Math.Min(from.y, to.y) : Math.Max(from.y, to.y);
			for (int y = from.y; offset.y > 0 ? y <= to.y : y >= to.y; y += offset.y) {
				for (int x = from.x; offset.x > 0 ? x <= to.x : x >= to.x; x += offset.x) {

					int curIndex = y * size.x + x;
					bool dirt = dirtPixels [curIndex];
					float value = 0;

					float increment = Math.Abs (dir.x) + Math.Abs (dir.y) > 1 ? (float)Math.Sqrt(2.0) : 1.0f;

					if (x == borderX || y == borderY) { // border case
						if (dirt)
							value = 127;
						else
							value = -128;
					} else {
						float valueOnTheLeft = sdfPixels [(y - dir.y) * size.x + x - dir.x];
						if (dirt)
							value = Math.Max(0.0f, Math.Min (valueOnTheLeft + increment, 127.0f));
						else
							value = Math.Min(-1.0f, Math.Max (valueOnTheLeft - increment, -128.0f));
					}

					float lastValue = sdfPixels [curIndex];
					float newValue = dirt ? Math.Min (lastValue, value) : Math.Max (lastValue, value);
					sdfPixels [curIndex] = newValue;
				}
			}

		}





		public Texture2D ExtractSignedDistanceField(Texture2D terrainTex)
		{
			int Width = terrainTex.Width;
			int Height = terrainTex.Height;

			Color[] pixels = new Color[Width * Height];
			terrainTex.GetData<Color> (pixels);


			bool[] dirtPixels = new bool[Width * Height];
			float[] sdfPixels = new float[Width * Height];

			// Initialize with "dirt"
			for (int y = 0; y < Height; ++y) {
				for (int x = 0; x < Width; ++x) {

					Color curPixel = pixels [y * Width + x];
					bool dirt = curPixel.R == curPixel.G && curPixel.G == curPixel.B && curPixel.R == 255;
					dirtPixels [y * Width + x] = dirt;

					sdfPixels [y * Width + x] = dirt ? 127 : -128;
				}
			}
				
			for (int i = -1; i <= 1; ++i)
				for (int j = -1; j <= 1; ++j) {

					// omit no-op
					if (i == 0 && j == 0)
						continue;

					sdfOneIter (dirtPixels, sdfPixels, Width, Height, new IntVector2 (i, j));
				}
					




			Texture2D sdf = new Texture2D (Ballz.The().Graphics.GraphicsDevice, Width, Height);

			Color[] sdfColorPixels = new Color[Width * Height];
			for (int y = 0; y < Height; ++y) {
				for (int x = 0; x < Width; ++x) {
					int colVal = (int)(sdfPixels [y * Width + x] + 128.0f);
					sdfColorPixels [y * Width + x] = new Color (colVal, colVal, colVal);
				}
			}
			sdf.SetData (sdfColorPixels);

			return sdf;


			/*
			var width = terrainSize.Width;
			var height = terrainSize.Height;
			float scale = 2f;
			RenderTarget2D partyspass = new RenderTarget2D (Ballz.The().Graphics.GraphicsDevice, (int)(scale * width), (int)(scale * height));

			Ballz.The().Graphics.GraphicsDevice.SetRenderTarget (partyspass);
			SpriteBatch spriteBatch = new SpriteBatch (Ballz.The().Graphics.GraphicsDevice);

			spriteBatch.Begin (0, null, SamplerState.LinearClamp);
			spriteBatch.Draw (sdf, new Vector2(0f, 0f), null, null, null, 0, new Vector2 (scale, scale));
			spriteBatch.End ();

			Ballz.The().Graphics.GraphicsDevice.SetRenderTarget (null);

			Texture2D sdf2 = new Texture2D (Ballz.The().Graphics.GraphicsDevice, 
				(int)(scale * width), (int)(scale * height));

			Color[] wurst = new Color[(int)(scale * width) * (int)(scale * height)];
			//partyspass.GetData<Color> (wurst);


			System.Drawing.Bitmap wursti = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

			for (int y = 0; y < terrainSize.Height; ++y) {
				for (int x = 0; x < terrainSize.Width; ++x) {

					int col = (int)(sdfPixels [y * terrainSize.Width + x] + 128.0f);
					wursti.SetPixel (x, y, System.Drawing.Color.FromArgb(col, col, col));

				}
			}

			System.Drawing.Bitmap newimg = new System.Drawing.Bitmap((int)(wursti.Width * scale), (int)(wursti.Height * scale));
			using(System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(newimg))
			{
				// Here you set your interpolation mode
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bicubic;
				// Scale the image, by drawing it on the larger bitmap
				g.DrawImage(wursti, new System.Drawing.Rectangle(System.Drawing.Point.Empty, newimg.Size));
			}


			for (int y = 0; y <  (int)(scale * height); ++y) {
				for (int x = 0; x <  (int)(scale * width); ++x) {

					//Color curPixel = sdfPixels [y * (int)(scale * width) + x];
					//wursti.SetPixel (x, y, System.Drawing.Color.FromArgb(curPixel.R, curPixel.G, curPixel.B));

					System.Drawing.Color curPixel = newimg.GetPixel(x, y);

					const int thresh = 130;
					wurst [y * (int)(scale * width) + x] = new Color (curPixel.R > thresh ? curPixel.R / 2 : 0 * curPixel.R, curPixel.R > thresh ? 70 : 0 * curPixel.G, curPixel.R > thresh ? 0 : 0 * curPixel.B);

				}
			}

			sdf2.SetData<Color> (wurst);

			return sdf2;
			*/
		}

    }
}