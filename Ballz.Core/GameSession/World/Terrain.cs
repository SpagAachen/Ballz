using System;
using System.Linq;
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

        private List<Triangle> triangles = new List<Triangle>();

        // Deprecated!!
        private List<List<Vector2>> outlines = new List<List<Vector2>>();
        private List<Triangle> physicsTris = new List<Triangle>();

		private Texture2D terrainData = null;
		private bool[,] terrainBitmap = null;

		private bool up2date = false;
		// We might need that later on...
		//private Texture2D terrainSDF = null;

		public Terrain (Texture2D terrainTexture)
		{
			terrainData = terrainTexture;
			//terrainSDF = ExtractSignedDistanceField (terrainData);

			int Width = terrainData.Width;
			int Height = terrainData.Height;

			terrainBitmap = new bool[Width, Height];

			Color[] pixels = new Color[Width * Height];
			terrainData.GetData<Color> (pixels);


			for (int y = 0; y < Height; ++y) {
				for (int x = 0; x < Width; ++x) {

					Color curPixel = pixels [y * Width + x];

					// Note that we flip the y coord here
					if(curPixel == Color.White)
						terrainBitmap [x, Height-y-1] = true;
				}
			}

            update();
		}

		/*
		// Debug
		public void storeCurrentBitmapToFile(string filename)
		{
			System.Drawing.Bitmap wusa = new System.Drawing.Bitmap(terrainBitmap.GetLength(0), terrainBitmap.GetLength(1), System.Drawing.Imaging.PixelFormat.Format24bppRgb);

			for (int y = 0; y < terrainBitmap.GetLength(1); ++y)
				for (int x = 0; x < terrainBitmap.GetLength(0); ++x)
				{
					wusa.SetPixel(x, y, terrainBitmap[x, y] ? System.Drawing.Color.White : System.Drawing.Color.Black);
				}

			wusa.Save(filename);
		}
		*/

        /*
        // Deprecated!
		private void extractOutline()
		{
			// Do not extract outline if outline is correct already
			if (up2date)
				return;
			
			outline.Clear ();

            // TODO: VertexHelper does not support holes in terrain
			Physics2DDotNet.Shapes.ArrayBitmap ab = new Physics2DDotNet.Shapes.ArrayBitmap(terrainBitmap);
			AdvanceMath.Vector2D[] geometry = Physics2DDotNet.Shapes.VertexHelper.CreateFromBitmap(ab);
            foreach(AdvanceMath.Vector2D vec in geometry)
			    outline.Add(new Vector2(vec.X,vec.Y));

			up2date = true;
		}
        */

		private float distance(float x1, float y1, float x2, float y2)
		{
			return (float)Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
		}

		public void SubtractCircle(float x, float y, float radius)
		{
			// Compute bounding box
			int tlx = (int)Math.Floor(x - radius);
			int tly = (int)Math.Floor(y - radius);
			int brx = (int)Math.Ceiling(x + radius);
			int bry = (int)Math.Ceiling(y + radius);

			// Terrain width/height
			int Width = terrainBitmap.GetLength(0);
			int Height = terrainBitmap.GetLength(1);

			// Iterate over bounding box part of bitmap
			for (int j = Math.Max(0, tly); j < Math.Min(Height, bry); ++j) {
				for (int i = Math.Max(0, tlx); i < Math.Min(Width, brx); ++i) {

					if (distance(i, j, x, y) > radius)
						continue;

					// Subtract dirt (if any)
					terrainBitmap [i, j] = false;

				}
			}

			up2date = false;
			
		}

        public void AddCircle(float x, float y, float radius)
        {
            // Compute bounding box
            int tlx = (int)Math.Floor(x - radius);
            int tly = (int)Math.Floor(y - radius);
            int brx = (int)Math.Ceiling(x + radius);
            int bry = (int)Math.Ceiling(y + radius);

            // Terrain width/height
            int Width = terrainBitmap.GetLength(0);
            int Height = terrainBitmap.GetLength(1);

            // Iterate over bounding box part of bitmap
            for (int j = Math.Max(0, tly); j < Math.Min(Height, bry); ++j) {
                for (int i = Math.Max(0, tlx); i < Math.Min(Width, brx); ++i) {

                    if (distance(i, j, x, y) > radius)
                        continue;

                    // Subtract dirt (if any)
                    terrainBitmap [i, j] = true;

                }
            }

            up2date = false;

        }


        public class GridEdge
        {
            public IntVector2 a, b;
            public GridEdge(IntVector2 a, IntVector2 b)
            {
                this.a = a; this.b = b;
            }

            public override bool Equals(object obj)
            {
                var g = obj as GridEdge;
                if (g == null)
                    return false;
                else
                    return a.x == g.a.x
                        && a.y == g.a.y
                        && b.x == g.b.x
                        && b.y == g.b.y;
            }
        }

        public class Edge
        {
            public GridEdge a, b;
            public Edge(GridEdge a, GridEdge b, int x, int y)
            {
                if(a == null)
                    throw new InvalidOperationException("Fuck");
                this.a = a; this.b = b;
                this.x = x; this.y = y;
            }

            public bool outlineHandled = false;

            public int x, y;

            public Vector2[] ExtractLine()
            {
                Vector2[] ret = new Vector2[2];
                ret[0] = 0.5f * (new Vector2(a.a.x, a.a.y) + new Vector2(a.b.x, a.b.y));
                ret[1] = 0.5f * (new Vector2(b.a.x, b.a.y) + new Vector2(b.b.x, b.b.y));

                return ret;
            }

            public Edge[] getNeighbors(Edge[,,] outlineEdgeStuff)
            {
                var neighborCellEdges = new List<Edge>(8);

                if(x > 0)
                {
                    neighborCellEdges.Add(outlineEdgeStuff[x - 1, y, 0]);
                    neighborCellEdges.Add(outlineEdgeStuff[x - 1, y, 1]);
                }

                if(x < outlineEdgeStuff.GetLength(0) - 1)
                {
                    neighborCellEdges.Add(outlineEdgeStuff[x + 1, y, 0]);
                    neighborCellEdges.Add(outlineEdgeStuff[x + 1, y, 1]);
                }

                if(y > 0)
                {
                    neighborCellEdges.Add(outlineEdgeStuff[x, y - 1, 0]);
                    neighborCellEdges.Add(outlineEdgeStuff[x, y - 1, 1]);
                }

                if(y < outlineEdgeStuff.GetLength(1) - 1)
                {
                    neighborCellEdges.Add(outlineEdgeStuff[x, y + 1, 0]);
                    neighborCellEdges.Add(outlineEdgeStuff[x, y + 1, 1]);
                }

                return (from e in neighborCellEdges
                         where e != null && (e.a.Equals(a) || e.b.Equals(a)
                                          || e.a.Equals(b) || e.b.Equals(b))
                         select e)
                        .ToArray();
            }
        }

        public void AddEdges(Edge[,,] outlineEdgeStuff, int x, int y, GridEdge a, GridEdge b)
        {
            if(outlineEdgeStuff[x, y, 0] == null)
            {
                outlineEdgeStuff[x, y, 0] = new Edge(a, b, x, y);
            }
            else
            {
                outlineEdgeStuff[x, y, 1] = new Edge(a, b, x, y);
            }
        }

        public List<Vector2> FollowOutline(Edge[,,] outlineEdgeStuff, Edge edge)
        {
            List<Vector2> result = new List<Vector2>(100);
            Edge lastEdge = null;
            while(!edge.outlineHandled)
            {
                edge.outlineHandled = true;

                var neighbors = edge.getNeighbors(outlineEdgeStuff);
                var nextEdge = (from e in neighbors
                    where e != lastEdge
                    select e)
                    .FirstOrDefault();

                if (nextEdge == null)
                {
                    //TODO(ks) better solution than that
                    //return new List<Vector2>();
                    
                    return result;
                }
                
                if(nextEdge.outlineHandled)
                {
                    if(result.Count > 0)
                        result.Add(result[0]);
                    return result;
                }

                if (edge.a.Equals(nextEdge.a) || edge.a.Equals(nextEdge.b))
                    result.Add(edge.ExtractLine()[0]);
                else
                    result.Add(edge.ExtractLine()[1]);

                lastEdge = edge;
                edge = nextEdge;
            }

            return result;
        }

        public struct Triangle
        {
            public Vector2 a,b,c;
            public Triangle(Vector2 a, Vector2 b, Vector2 c)
            {
                this.a = a; this.b = b; this.c = c;
            }
        }
        private void extractTrianglesAndOutline()
        {
            // Do not extract triangles if they are correct already
            if (up2date)
                return;

            triangles.Clear();
            physicsTris.Clear();
            outlines.Clear();

            int width = terrainBitmap.GetLength(0);
            int height = terrainBitmap.GetLength(1);


            // reserve max. memory
            List<List<IntVector2>> fullCells = new List<List<IntVector2>>(height);
            /*Dictionary<GridEdge, List<Edge>> outlineEdgeStuff = new Dictionary<GridEdge, List<Edge>>();*/

            Edge[,,] outlineEdgeStuff = new Edge[width, height, 2];


            // Iterate over all bitmap pixels
            for (int y = 1; y < height; ++y)
            {
                fullCells.Add(new List<IntVector2>(width));
                for (int x = 1; x < width; ++x)
                {
                    
                    /*
                     *      0 -- 3
                     *      |    |
                     *      1 -- 2
                     * 
                     */

                    // TODO: upper-left row/column
                    bool _0 = terrainBitmap[x-1, y-1];
                    bool _1 = terrainBitmap[x-1, y];
                    bool _2 = terrainBitmap[x, y];
                    bool _3 = terrainBitmap[x, y-1];

                    Vector2 v0 = new Vector2(x-1, y-1);
                    Vector2 v1 = new Vector2(x-1, y);
                    Vector2 v2 = new Vector2(x, y);
                    Vector2 v3 = new Vector2(x, y-1);


                    GridEdge e01 = null;
                    GridEdge e12 = null;
                    GridEdge e32 = null;
                    GridEdge e03 = null;


                    int mscase = (_0?1:0) + (_1?2:0) + (_2?4:0) + (_3?8:0);

                    if(mscase != 0 && mscase != 15)
                    {
                        e01 = new GridEdge(new IntVector2(x - 1, y - 1), new IntVector2(x - 1, y));
                        e12 = new GridEdge(new IntVector2(x - 1, y), new IntVector2(x, y));
                        e32 = new GridEdge(new IntVector2(x, y-1), new IntVector2(x, y));
                        e03 = new GridEdge(new IntVector2(x - 1, y - 1), new IntVector2(x, y-1));
                    }

                    switch(mscase)
                    {
                        case 0:
                            // No triangles at all
                            break;
                        case 1:
                            triangles.Add(new Triangle(v0, 0.5f * (v0 + v1), 0.5f * (v0 + v3)));
                            AddEdges(outlineEdgeStuff, x, y, e01, e03);
                            break;
                        case 2:
                            triangles.Add(new Triangle(v1, 0.5f * (v1 + v2), 0.5f * (v0 + v1)));
                            AddEdges(outlineEdgeStuff, x, y, e12, e01);
                            break;
                        case 3:
                            triangles.Add(new Triangle(v0, v1, 0.5f * (v1 + v2)));
                            triangles.Add(new Triangle(0.5f * (v1 + v2), 0.5f * (v0 + v3), v0));
                            AddEdges(outlineEdgeStuff, x, y, e03, e12);
                            break;
                        case 4:
                            triangles.Add(new Triangle(v2, 0.5f * (v2 + v3), 0.5f * (v1 + v2)));
                            AddEdges(outlineEdgeStuff, x, y, e32, e12);
                            break;
                        case 5:
                            triangles.Add(new Triangle(v0, 0.5f * (v0 + v1), 0.5f * (v0 + v3)));
                            triangles.Add(new Triangle(v2, 0.5f * (v2 + v3), 0.5f * (v1 + v2)));
                            AddEdges(outlineEdgeStuff, x, y, e01, e03);
                            AddEdges(outlineEdgeStuff, x, y, e32, e12);
                            break;
                        case 6:
                            triangles.Add(new Triangle(v2, 0.5f * (v0 + v1), v1));
                            triangles.Add(new Triangle(v2, 0.5f * (v2 + v3), 0.5f * (v0 + v1)));
                            AddEdges(outlineEdgeStuff, x, y, e01, e32);
                            break;
                        case 7:
                            triangles.Add(new Triangle(v1, 0.5f * (v0 + v3), v0));
                            triangles.Add(new Triangle(v1, 0.5f * (v2 + v3), 0.5f * (v0 + v3)));
                            triangles.Add(new Triangle(v1, v2, 0.5f * (v2 + v3)));
                            AddEdges(outlineEdgeStuff, x, y, e03, e32);
                            break;
                        case 8:
                            triangles.Add(new Triangle(v3, 0.5f * (v0 + v3), 0.5f * (v2 + v3)));
                            AddEdges(outlineEdgeStuff, x, y, e03, e32);
                            break;
                        case 9:
                            triangles.Add(new Triangle(v0, 0.5f * (v2 + v3), v3));
                            triangles.Add(new Triangle(v0, 0.5f * (v0 + v1), 0.5f * (v2 + v3)));
                            AddEdges(outlineEdgeStuff, x, y, e32, e01);
                            break;
                        case 10: 
                            triangles.Add(new Triangle(v1, 0.5f * (v1 + v2), 0.5f * (v0 + v1)));
                            triangles.Add(new Triangle(v3, 0.5f * (v0 + v3), 0.5f * (v2 + v3)));
                            AddEdges(outlineEdgeStuff, x, y, e12, e01);
                            AddEdges(outlineEdgeStuff, x, y, e03, e32);
                            break;
                        case 11: 
                            triangles.Add(new Triangle(v0, v1, 0.5f * (v1 + v2)));
                            triangles.Add(new Triangle(v0, 0.5f * (v1 + v2), 0.5f * (v2 + v3)));
                            triangles.Add(new Triangle(v0, 0.5f * (v2 + v3), v3));
                            AddEdges(outlineEdgeStuff, x, y, e12, e32);
                            break;
                        case 12: 
                            triangles.Add(new Triangle(v3, 0.5f * (v0 + v3), 0.5f * (v1 + v2)));
                            triangles.Add(new Triangle(v3, 0.5f * (v1 + v2), v2));
                            AddEdges(outlineEdgeStuff, x, y, e03, e12);
                            break;
                        case 13: 
                            triangles.Add(new Triangle(v3, v0, 0.5f * (v0 + v1)));
                            triangles.Add(new Triangle(v3, 0.5f * (v0 + v1), 0.5f * (v1 + v2)));
                            triangles.Add(new Triangle(v3, 0.5f * (v1 + v2), v2));

                            AddEdges(outlineEdgeStuff, x, y, e01, e12);
                            break;
                        case 14: 
                            triangles.Add(new Triangle(v2, 0.5f * (v0 + v1), v1));
                            triangles.Add(new Triangle(v2, 0.5f * (v0 + v3), 0.5f * (v0 + v1)));
                            triangles.Add(new Triangle(v2, v3, 0.5f * (v0 + v3)));
                            AddEdges(outlineEdgeStuff, x, y, e01, e03);
                            break;
                        case 15:
                            fullCells[fullCells.Count - 1].Add(new IntVector2(x, y));
                            // Fill the whole quad
                            //triangles.Add(new Triangle(v0, v1, v3));
                            //triangles.Add(new Triangle(v3, v1, v2));
                            break;
                        default:
                            break;
                    }
                }
            }
                
            for (int y = 1; y < height; ++y)
            {
                for (int x = 1; x < width; ++x)
                {
                    if (outlineEdgeStuff[x, y, 0] != null && outlineEdgeStuff[x, y, 0].outlineHandled == false)
                    {
                        var outline = FollowOutline(outlineEdgeStuff, outlineEdgeStuff[x, y, 0]);
                        outlines.Add(outline);
                    }
                    if (outlineEdgeStuff[x, y, 1] != null && outlineEdgeStuff[x, y, 1].outlineHandled == false)
                    {
                        var outline = FollowOutline(outlineEdgeStuff, outlineEdgeStuff[x, y, 1]);
                        outlines.Add(outline);
                    }
                }
            }


            foreach (var tri in triangles)
            {
                physicsTris.Add(tri);
            }


            foreach (var line in fullCells)
            {
                int triStartX = -1;
                int triEndX   = -1;

                bool startNew = true;

                for (int i = 0; i < line.Count; ++i)
                {
                    var cell = line[i];


                    if(startNew)
                    {
                        triStartX = cell.x - 1;
                        triEndX = cell.x;
                    }

                    bool createTriangle = (cell.x - 1 != triEndX) || i == line.Count-1;

                    // Shift current end pointer?
                    if (!createTriangle || i == line.Count - 1)
                    {
                        triEndX = cell.x;
                        startNew = false;
                    }

                    if(createTriangle)
                    {
                        triangles.Add(new Triangle(new Vector2(triStartX, cell.y-1), new Vector2(triStartX, cell.y), new Vector2(triEndX, cell.y-1)));
                        triangles.Add(new Triangle(new Vector2(triStartX, cell.y), new Vector2(triEndX, cell.y), new Vector2(triEndX, cell.y-1)));

                        startNew = true;
                    } 
                }
            }



            up2date = true;
        }


        public void update()
        {
            if (up2date)
                return;

            extractTrianglesAndOutline();
        }

        public List<Triangle> getTriangles()
        {
            // This is a no-op if triangles are up to date
            update();

            return triangles;
        }
            
        public List<Triangle> getOutlineTriangles()
        {
            // This is a no-op if outline is up to date
            update();

            return physicsTris;
        }

        public List<List<Vector2>> getOutline()
        {
            // This is a no-op if outline is up to date
            update();

            return outlines;
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