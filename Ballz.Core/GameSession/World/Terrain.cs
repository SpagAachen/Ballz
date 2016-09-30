using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ballz.GameSession.World
{
    /// <summary>
    /// Represents the Terrain.
    /// </summary>
    public class Terrain
    {
        public static readonly bool decimateOutlines = true;

        public enum TerrainType
        {
            Air = 0,
            Earth = 1,
            Stone = 2,
            Sand = 3,
            Water = 4,
            VegetationSeed = 5,
            SpawnPoint = 6,
            //...
        }
        
        public class TerrainShape
        {
            /// <summary>
            /// Triangles for rendering.
            /// </summary>
            /// <remarks>
            /// Positions are in Pixel units. Use <see cref="Terrain.Scale"/> to transform into world coordinates.
            /// </remarks>
            public List<Triangle> Triangles = new List<Triangle>();

            /// <summary>
            /// Outline line segments for use in physics.
            /// </summary>
            /// <remarks>
            /// Positions are in Pixel units. Use <see cref="Terrain.Scale"/> to transform into world coordinates.
            /// </remarks>
            public List<List<Vector2>> Outlines = new List<List<Vector2>>();

            /// <summary>
            /// A bitmap of the terrain shape. True means earth, False means air.
            /// </summary>
            //public bool[,] TerrainBitmap = null;

            public TerrainType[,] TerrainBitmap = null;
        }

        public class TerrainModification
        {
            public float X;
            public float Y;
            public float Radius;
            public bool Subtract;
        }

        public event EventHandler<TerrainModification> TerrainModified;

        public bool[,] WaterSpawnBitmap = null;

        /// <summary>
        /// The revision number of the <see cref="PublicShape"/> triangles and outline.
        /// </summary>
        public int Revision { get; private set; } = 0;

        /// <summary>
        /// The revision number of the <see cref="PublicShape"/> terrainBitmap.
        /// </summary>
        private int WorkingRevision = 1;

        /// <summary>
        /// The world scale of the terrain, in meters per pixel.
        /// </summary>
        public float Scale = 0.082f;

        public Vector2 gravityPoint = new Vector2(0, float.MinValue);//0.082f * new Vector2(224, 250-66);

        /// <summary>
        /// The most recent terrain shape. Will be updated by the background worker once <see cref="WorkingShape"/> is finished.
        /// </summary>
        /// <remarks>
        /// Changes can be made to the terrainBitmap of this shape. They will be pulled by the background worker, which will process
        /// the changes and write the resulting triangles and outline back to this shape when finished.
        /// Use a lock on the publicShape object when accessing it.
        /// </remarks>
        public TerrainShape PublicShape = new TerrainShape();

        /// <summary>
        /// A "work-in-progress" terrain shape that is used by the background worker that updates the shape.
        /// </summary>
        private TerrainShape WorkingShape = null;

        // Internal terrain bitmaps
        private Texture2D terrainData = null;
        private float[,] terrainSmoothmap = null;
        private float[,] terrainSmoothmapCopy = null;

        // Terrain size
        public int width { get; } = -1;
        public int height { get; } = -1;

        // Internal members to avoid reallocation
        private Border[,] bordersH = null;
        private Border[,] bordersV = null;
        private List<List<IntVector2>> fullCells = null;
        private List<Edge> allEdges = null;
        
        public Terrain(Texture2D terrainTexture)
        {
            terrainData = terrainTexture;

            width = terrainData.Width;
            height = terrainData.Height;

            PublicShape.TerrainBitmap = new TerrainType[width, height];
            terrainSmoothmap = new float[width, height];
            terrainSmoothmapCopy = new float[width, height];
            WaterSpawnBitmap = new bool[width, height];

            Color[] pixels = new Color[width * height];
            terrainData.GetData<Color>(pixels);
            
            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    Color curPixel = pixels[y * width + x];

                    // Note that we flip the y coord here
                    if (curPixel == new Color(127, 64, 0))
                    {
                        PublicShape.TerrainBitmap[x, height - y - 1] = TerrainType.Earth;
                        terrainSmoothmap[x, height - y - 1] = 1.0f;
                    }
                    else if (curPixel == new Color(127, 127, 127))
                    {
                        PublicShape.TerrainBitmap[x, height - y - 1] = TerrainType.Stone;
                    }
                    else if (curPixel == Color.Yellow)
                    {
                        PublicShape.TerrainBitmap[x, height - y - 1] = TerrainType.Sand;
                    }
                    else if (curPixel == Color.Blue)
                    {
                        WaterSpawnBitmap[x, height - y - 1] = true;
                        PublicShape.TerrainBitmap[x, height - y - 1] = TerrainType.Water;
                    }
                    else if (curPixel == Color.Lime)
                    {
                        PublicShape.TerrainBitmap[x, height - y - 1] = TerrainType.VegetationSeed;
                    }
                    else if (curPixel == Color.Red)
                    {
                        PublicShape.TerrainBitmap[x, height - y - 1] = TerrainType.SpawnPoint;
                    }
                }
            }

            // Pre-allocate internal members
            bordersH = new Border[width, height];
            bordersV = new Border[width, height];
            fullCells = new List<List<IntVector2>>(height);
            allEdges = new List<Edge>();

            // Let sand fall down
            UpdateTerrain(0, 0, width);

            Update();
        }

        public bool IsWaterSpawn(float x, float y)
        {
            x /= Scale;
            y /= Scale;
            if (x < 0 || x >= width || y < 0 || y >= height)
                return false;
            else
                return WaterSpawnBitmap[(int)x, (int)y];
        }
        
        private float Distance(float x1, float y1, float x2, float y2)
        {
            return (float)Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
        }

        public void SubtractCircle(float x, float y, float radius)
        {
            x /= Scale;
            y /= Scale;
            radius /= Scale;

            ApplyModification(new TerrainModification
            {
                X = x,
                Y = y,
                Radius = radius,
                Subtract = true
            });
        }

        public void AddCircle(float x, float y, float radius)
        {
            x /= Scale;
            y /= Scale;
            radius /= Scale;

            ApplyModification(new TerrainModification
            {
                X = x,
                Y = y,
                Radius = radius,
                Subtract = false
            });
        }

        public void ApplyModification(TerrainModification mod)
        {
            TerrainModified?.Invoke(this, mod);

            // Compute bounding box
            int tlx = (int)Math.Floor(mod.X - mod.Radius);
            int tly = (int)Math.Floor(mod.Y - mod.Radius);
            int brx = (int)Math.Ceiling(mod.X + mod.Radius);
            int bry = (int)Math.Ceiling(mod.Y + mod.Radius);

            // Clamp to world boundaries
            tlx = Math.Min(Math.Max(0, tlx), width - 1);
            tly = Math.Min(Math.Max(0, tly), height - 1);
            brx = Math.Min(Math.Max(0, brx), width - 1);
            bry = Math.Min(Math.Max(0, bry), height - 1);


            // Iterate over bounding box part of bitmap
            for (int j = tly; j < bry; ++j)
            {
                for (int i = tlx; i < brx; ++i)
                {
                    if (Distance(i, j, mod.X, mod.Y) > mod.Radius)
                        continue;

                    TerrainType cur = PublicShape.TerrainBitmap[i, j];

                    if (cur == TerrainType.Earth || cur == TerrainType.Sand)
                    {
                        if (mod.Subtract)
                            PublicShape.TerrainBitmap[i, j] = TerrainType.Air;
                        else
                            PublicShape.TerrainBitmap[i, j] = TerrainType.Earth;
                    }
                }
            }

            // Let sand "fall down" etc.
            UpdateTerrain(tly, tlx, brx);

            WorkingRevision++;
        }

        public class Border
        {
            public Edge E0 = null, E1 = null;

            public Border(Edge edge0)
            {
                if (edge0 == null)
                    throw new InvalidOperationException("Trying to add null-edge to border!");

                E0 = edge0;
            }

            public bool Valid()
            {
                return E0 != null || E1 != null;
            }
        }

        public class Edge
        {
            public Border B0, B1;

            public int AX, AY;
            public int BX, BY;

            public Vector2 A;
            public Vector2 B;

            public Edge(int ax, int ay, int bx, int by)
            {
                this.AX = ax; this.AY = ay;
                this.BX = bx; this.BY = by;

                A = 0.5f * new Vector2(ax, ay);
                B = 0.5f * new Vector2(bx, by);
            }

            public Edge(int ax, int ay, int bx, int by, Vector2 a, Vector2 b)
            {
                this.A = a; this.B = b;
                this.AX = ax; this.AY = ay;
                this.BX = bx; this.BY = by;
            }
        }

        private float area(Vector2 a, Vector2 b, Vector2 c)
        {
            var ab = b - a;
            var ac = c - a;

            var cross = ab.X * ac.Y - ab.Y * ac.X;

            return 0.5f * Math.Abs(cross);
        }

        private void DecimateOutline(List<Vector2> outline)
        {
            // Simple version
            /*
            const int rem = 10;
            for (int i = outline.Count - 2; i > 0; --i)
            {
                if(i % rem > 0)
                    outline.RemoveAt(i);
            }
            */

            // At around 0.1 and lower, the physics outline cannot be distinguished from the rendering
            // At around 0.7 and larger, the decimation has quite a visible effect
            const float thresh = 0.5f;
            const int maxiters = 3; // Will not take more than 3 runs anyway  
            bool changed;
            for(int k = 0; k < maxiters; ++k)
            {
                changed = false;
                for (int i = outline.Count - 1; i > 2; --i)
                {
                    Vector2 a = outline[i-0];
                    Vector2 b = outline[i-1];
                    Vector2 c = outline[i-2];

                    float A = area(a, b, c);
                    if (A < thresh)
                    {
                        outline.RemoveAt(i-1);
                        changed = true;
                    }
                }

                // Early out
                if (!changed)
                {
                    //Console.WriteLine("Early out after " + k + " iters.");
                    break;
                }
            }
        }

        private void ExtractOutline(Edge edge)
        {
            var o1 = ExtractOutlineInternal(edge);
            if (o1 == null)
                return; // no outline at all

            // check non-closed case
            if (edge.B0 != null || edge.B1 != null)
            {
                var o2 = ExtractOutlineInternal(edge);
                o1.Reverse();
                o1.AddRange(o2);
            }


            //int countbefore = o1.Count;
            if(decimateOutlines)
                DecimateOutline(o1);

            //Console.WriteLine("Removed " + (float)(countbefore - o1.Count) / (float)(countbefore) * 100.0f + " %");

            WorkingShape.Outlines.Add(o1);
        }

        private List<Vector2> ExtractOutlineInternal(Edge edge)
        {
            if (edge == null)
                return null;

            var firstEdge = edge;

            var result = new List<Vector2>(100);

            while (edge != null)
            {
                // follow first border
                Border b = edge.B0 ?? edge.B1;
                if (b == null)
                    break;

                // null border in edge (and add result)
                if (edge.B0 == b)
                {
                    edge.B0 = null;
                    result.Add(edge.A);
                }
                else if (edge.B1 == b)
                {
                    edge.B1 = null;
                    result.Add(edge.B);
                }
                else
                    throw new InvalidOperationException("lolnope1");

                // null edge in border
                if (b.E0 == edge)
                {
                    b.E0 = null;
                }
                else if (b.E1 == edge)
                {
                    b.E1 = null;
                }
                else
                    throw new InvalidOperationException("lolnope2");

                // follow next edge
                edge = b.E0 ?? b.E1;
                if (edge == null)
                    break;

                // null border in edge
                if (edge.B0 == b)
                {
                    edge.B0 = null;
                    if (edge == firstEdge) // closed loop
                        result.Add(edge.B);
                }
                else if (edge.B1 == b)
                {
                    edge.B1 = null;
                    if (edge == firstEdge) // closed loop
                        result.Add(edge.A);
                }
                else
                    throw new InvalidOperationException("lolnope3");

                // null edge in border
                if (b.E0 == edge)
                {
                    b.E0 = null;
                }
                else if (b.E1 == edge)
                {
                    b.E1 = null;
                }
                else
                    throw new InvalidOperationException("lolnope4");
            }

            return result;
        }

        public Vector2 Mix(Vector2 a, Vector2 b, float wa, float wb)
        {
            float weight = (0.5f - wa) / (wb - wa);

            return a + weight * (b - a);
        }

        public struct Triangle
        {
            public Vector2 A, B, C;

            public Triangle(Vector2 a, Vector2 b, Vector2 c)
            {
                this.A = a; this.B = b; this.C = c;
            }
        }
        
        private void SmoothTerrain(float[] gauss)
        {
            int halfGauss = gauss.GetLength(0) / 2;

            // Iterate over all bitmap pixels
            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    float sum = 0.0f;
                    float weight = 0.0f;
                    for (int i = -halfGauss; i <= halfGauss; ++i)
                    {
                        int index = x + i;
                        if (index >= 0 && index < width)
                        {
                            float val = gauss[i + halfGauss];

                            var cur = WorkingShape.TerrainBitmap[index, y];
                            sum += ((cur == TerrainType.Earth || cur == TerrainType.Sand || cur == TerrainType.Stone )? 1.0f : 0.0f) * val;
                            weight += val;
                        }
                    }

                    float newVal = sum / weight;
                    terrainSmoothmap[x, y] = newVal;
                    terrainSmoothmapCopy[x, y] = newVal;
                }
            }

            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    float sum = 0.0f;
                    float weight = 0.0f;
                    for (int i = -halfGauss; i <= halfGauss; ++i)
                    {
                        int index = y + i;
                        if (index >= 0 && index < height)
                        {
                            float val = gauss[i + halfGauss];

                            sum += terrainSmoothmapCopy[x, index] * val;
                            weight += val;
                        }
                    }

                    terrainSmoothmap[x, y] = sum / weight;
                }
            }
        }

        private void PerformMarchingSquares()
        {
            // Clear internal members
            WorkingShape.Triangles.Clear();
            allEdges.Clear();
            fullCells.Clear();

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

                    float w0 = terrainSmoothmap[x - 1, y - 1];
                    float w1 = terrainSmoothmap[x - 1, y];
                    float w2 = terrainSmoothmap[x, y];
                    float w3 = terrainSmoothmap[x, y - 1];

                    bool _0 = w0 > 0.5f;
                    bool _1 = w1 > 0.5f;
                    bool _2 = w2 > 0.5f;
                    bool _3 = w3 > 0.5f;

                    int mscase = (_0 ? 1 : 0) + (_1 ? 2 : 0) + (_2 ? 4 : 0) + (_3 ? 8 : 0);

                    // Early-out?
                    if (mscase == 0)
                        continue;

                    // Early-in?
                    if (mscase == 15)
                    {
                        fullCells[fullCells.Count - 1].Add(new IntVector2(x, y));
                    }
                    else
                    {
                        Vector2 v0 = new Vector2(x - 1, y - 1);
                        Vector2 v1 = new Vector2(x - 1, y);
                        Vector2 v2 = new Vector2(x, y);
                        Vector2 v3 = new Vector2(x, y - 1);
                        
                        switch (mscase)
                        {
                            case 0:
                                // No triangles at all
                                break;
                            case 1:
                                WorkingShape.Triangles.Add(new Triangle(v0, Mix(v0, v1, w0, w1), Mix(v0, v3, w0, w3)));
                                //allEdges.Add(new Edge(2*x-1, 2*(y-1), 2*(x-1), 2*y-1));
                                allEdges.Add(new Edge(2 * x - 1, 2 * (y - 1), 2 * (x - 1), 2 * y - 1, Mix(v0, v1, w0, w1), Mix(v0, v3, w0, w3)));
                                break;
                            case 2:
                                WorkingShape.Triangles.Add(new Triangle(v1, Mix(v1, v2, w1, w2), Mix(v0, v1, w0, w1)));
                                allEdges.Add(new Edge(2 * (x - 1), 2 * y - 1, 2 * x - 1, 2 * y, Mix(v1, v2, w1, w2), Mix(v0, v1, w0, w1)));
                                break;
                            case 3:
                                WorkingShape.Triangles.Add(new Triangle(v0, v1, Mix(v1, v2, w1, w2)));
                                WorkingShape.Triangles.Add(new Triangle(Mix(v1, v2, w1, w2), Mix(v0, v3, w0, w3), v0));
                                allEdges.Add(new Edge(2 * x - 1, 2 * (y - 1), 2 * x - 1, 2 * y, Mix(v1, v2, w1, w2), Mix(v0, v3, w0, w3)));
                                break;
                            case 4:
                                WorkingShape.Triangles.Add(new Triangle(v2, Mix(v2, v3, w2, w3), Mix(v1, v2, w1, w2)));
                                allEdges.Add(new Edge(2 * x - 1, 2 * y, 2 * x, 2 * y - 1, Mix(v2, v3, w2, w3), Mix(v1, v2, w1, w2)));
                                break;
                            case 5:
                                WorkingShape.Triangles.Add(new Triangle(v0, Mix(v0, v1, w0, w1), Mix(v0, v3, w0, w3)));
                                WorkingShape.Triangles.Add(new Triangle(v2, Mix(v2, v3, w2, w3), Mix(v1, v2, w1, w2)));
                                allEdges.Add(new Edge(2 * x - 1, 2 * (y - 1), 2 * (x - 1), 2 * y - 1, Mix(v0, v1, w0, w1), Mix(v0, v3, w0, w3)));
                                allEdges.Add(new Edge(2 * x - 1, 2 * y, 2 * x, 2 * y - 1, Mix(v2, v3, w2, w3), Mix(v1, v2, w1, w2)));
                                break;
                            case 6:
                                WorkingShape.Triangles.Add(new Triangle(v2, Mix(v0, v1, w0, w1), v1));
                                WorkingShape.Triangles.Add(new Triangle(v2, Mix(v2, v3, w2, w3), Mix(v0, v1, w0, w1)));
                                allEdges.Add(new Edge(2 * (x - 1), 2 * y - 1, 2 * x, 2 * y - 1, Mix(v2, v3, w2, w3), Mix(v0, v1, w0, w1)));
                                break;
                            case 7:
                                WorkingShape.Triangles.Add(new Triangle(v1, Mix(v0, v3, w0, w3), v0));
                                WorkingShape.Triangles.Add(new Triangle(v1, Mix(v2, v3, w2, w3), Mix(v0, v3, w0, w3)));
                                WorkingShape.Triangles.Add(new Triangle(v1, v2, Mix(v2, v3, w2, w3)));
                                allEdges.Add(new Edge(2 * x - 1, 2 * (y - 1), 2 * x, 2 * y - 1, Mix(v2, v3, w2, w3), Mix(v0, v3, w0, w3)));
                                break;
                            case 8:
                                WorkingShape.Triangles.Add(new Triangle(v3, Mix(v0, v3, w0, w3), Mix(v2, v3, w2, w3)));
                                allEdges.Add(new Edge(2 * x - 1, 2 * (y - 1), 2 * x, 2 * y - 1, Mix(v0, v3, w0, w3), Mix(v2, v3, w2, w3)));
                                break;
                            case 9:
                                WorkingShape.Triangles.Add(new Triangle(v0, Mix(v2, v3, w2, w3), v3));
                                WorkingShape.Triangles.Add(new Triangle(v0, Mix(v0, v1, w0, w1), Mix(v2, v3, w2, w3)));
                                allEdges.Add(new Edge(2 * (x - 1), 2 * y - 1, 2 * x, 2 * y - 1, Mix(v0, v1, w0, w1), Mix(v2, v3, w2, w3)));
                                break;
                            case 10:
                                WorkingShape.Triangles.Add(new Triangle(v1, Mix(v1, v2, w1, w2), Mix(v0, v1, w0, w1)));
                                WorkingShape.Triangles.Add(new Triangle(v3, Mix(v0, v3, w0, w3), Mix(v2, v3, w2, w3)));
                                allEdges.Add(new Edge(2 * (x - 1), 2 * y - 1, 2 * x - 1, 2 * y, Mix(v1, v2, w1, w2), Mix(v0, v1, w0, w1)));
                                allEdges.Add(new Edge(2 * x - 1, 2 * (y - 1), 2 * x, 2 * y - 1, Mix(v0, v3, w0, w3), Mix(v2, v3, w2, w3)));
                                break;
                            case 11:
                                WorkingShape.Triangles.Add(new Triangle(v0, v1, Mix(v1, v2, w1, w2)));
                                WorkingShape.Triangles.Add(new Triangle(v0, Mix(v1, v2, w1, w2), Mix(v2, v3, w2, w3)));
                                WorkingShape.Triangles.Add(new Triangle(v0, Mix(v2, v3, w2, w3), v3));
                                allEdges.Add(new Edge(2 * x - 1, 2 * y, 2 * x, 2 * y - 1, Mix(v1, v2, w1, w2), Mix(v2, v3, w2, w3)));
                                break;
                            case 12:
                                WorkingShape.Triangles.Add(new Triangle(v3, Mix(v0, v3, w0, w3), Mix(v1, v2, w1, w2)));
                                WorkingShape.Triangles.Add(new Triangle(v3, Mix(v1, v2, w1, w2), v2));
                                allEdges.Add(new Edge(2 * x - 1, 2 * (y - 1), 2 * x - 1, 2 * y, Mix(v0, v3, w0, w3), Mix(v1, v2, w1, w2)));
                                break;
                            case 13:
                                WorkingShape.Triangles.Add(new Triangle(v3, v0, Mix(v0, v1, w0, w1)));
                                WorkingShape.Triangles.Add(new Triangle(v3, Mix(v0, v1, w0, w1), Mix(v1, v2, w1, w2)));
                                WorkingShape.Triangles.Add(new Triangle(v3, Mix(v1, v2, w1, w2), v2));
                                allEdges.Add(new Edge(2 * (x - 1), 2 * y - 1, 2 * x - 1, 2 * y, Mix(v0, v1, w0, w1), Mix(v1, v2, w1, w2)));
                                break;
                            case 14:
                                WorkingShape.Triangles.Add(new Triangle(v2, Mix(v0, v1, w0, w1), v1));
                                WorkingShape.Triangles.Add(new Triangle(v2, Mix(v0, v3, w0, w3), Mix(v0, v1, w0, w1)));
                                WorkingShape.Triangles.Add(new Triangle(v2, v3, Mix(v0, v3, w0, w3)));
                                allEdges.Add(new Edge(2 * x - 1, 2 * (y - 1), 2 * (x - 1), 2 * y - 1, Mix(v0, v3, w0, w3), Mix(v0, v1, w0, w1)));
                                break;
                            case 15:
                                fullCells[fullCells.Count - 1].Add(new IntVector2(x, y));
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        private void ExtractOutlineFromEdges()
        {
            // Clear old borders and outline
            Array.Clear(bordersH, 0, width * height);
            Array.Clear(bordersV, 0, width * height);
            WorkingShape.Outlines.Clear();

            foreach (var edge in allEdges)
            {
                IntVector2 edgeCoordsA = new IntVector2(edge.AX / 2, edge.AY / 2);
                IntVector2 edgeCoordsB = new IntVector2(edge.BX / 2, edge.BY / 2);
                if (edge.AX % 2 == 0)
                {
                    if (bordersV[edgeCoordsA.X, edgeCoordsA.Y] == null)
                        bordersV[edgeCoordsA.X, edgeCoordsA.Y] = new Border(edge);
                    else
                        bordersV[edgeCoordsA.X, edgeCoordsA.Y].E1 = edge;

                    edge.B0 = bordersV[edgeCoordsA.X, edgeCoordsA.Y];
                }
                else
                {
                    if (bordersH[edgeCoordsA.X, edgeCoordsA.Y] == null)
                        bordersH[edgeCoordsA.X, edgeCoordsA.Y] = new Border(edge);
                    else
                        bordersH[edgeCoordsA.X, edgeCoordsA.Y].E1 = edge;

                    edge.B0 = bordersH[edgeCoordsA.X, edgeCoordsA.Y];
                }

                if (edge.BX % 2 == 0)
                {
                    if (bordersV[edgeCoordsB.X, edgeCoordsB.Y] == null)
                        bordersV[edgeCoordsB.X, edgeCoordsB.Y] = new Border(edge);
                    else
                        bordersV[edgeCoordsB.X, edgeCoordsB.Y].E1 = edge;

                    edge.B1 = bordersV[edgeCoordsB.X, edgeCoordsB.Y];
                }
                else
                {
                    if (bordersH[edgeCoordsB.X, edgeCoordsB.Y] == null)
                        bordersH[edgeCoordsB.X, edgeCoordsB.Y] = new Border(edge);
                    else
                        bordersH[edgeCoordsB.X, edgeCoordsB.Y].E1 = edge;

                    edge.B1 = bordersH[edgeCoordsB.X, edgeCoordsB.Y];
                }
            }

            foreach (Border b in bordersH)
            {
                if (b == null)
                    continue;

                if (b.E0 != null)
                {
                    ExtractOutline(b.E0);
                }

                if (b.E1 != null)
                {
                    ExtractOutline(b.E1);
                }
            }

            foreach (Border b in bordersV)
            {
                ExtractOutline(b?.E0);
                ExtractOutline(b?.E1);
            }
        }

        private void ExtractTrianglesFromCells()
        {
            int triStartX = -1;
            bool startNew = true;
            foreach (var line in fullCells)
            {
                for (int i = 0; i < line.Count; ++i)
                {
                    var cell = line[i];

                    bool createTriangle = false;
                    if (i == 0 || startNew)
                    {
                        // new start
                        triStartX = cell.X - 1;
                        startNew = false;
                    }

                    if (i == line.Count - 1)
                    {
                        // need to close segment here
                        createTriangle = true;
                    }
                    else
                    {
                        if (cell.X + 1 < line[i + 1].X)
                        {
                            // gap
                            createTriangle = true;
                            startNew = true;
                        }
                    }

                    if (createTriangle)
                    {
                        WorkingShape.Triangles.Add(new Triangle(new Vector2(triStartX, cell.Y - 1), new Vector2(triStartX, cell.Y), new Vector2(cell.X, cell.Y - 1)));
                        WorkingShape.Triangles.Add(new Triangle(new Vector2(triStartX, cell.Y), new Vector2(cell.X, cell.Y), new Vector2(cell.X, cell.Y - 1)));
                    }
                }
            }
        }

        // Let sand fall down etc
        private void UpdateTerrain(int minY, int minX, int maxX)
        {
            minY = Math.Max(0, minY);
            
            // For every column check whether there is air below sand so that the latter moves downwards
            for (int x = minX; x < maxX; ++x)
            {
                int lowestAirPosition = height;
                for (int y = minY; y < height; ++y)
                {
                    TerrainType cur = PublicShape.TerrainBitmap[x, y];

                    //PublicShape.TerrainBitmap[x, y] = TerrainType.Air;

                    if (cur == TerrainType.Air)
                    {
                        if (lowestAirPosition == height)
                        {
                            lowestAirPosition = y;
                        }
                    }
                    else if (cur == TerrainType.Sand)
                    {
                        if (lowestAirPosition < height)
                        {
                            // Fill lowest air with sand
                            PublicShape.TerrainBitmap[x, lowestAirPosition] = TerrainType.Sand;

                            ++lowestAirPosition;

                            // This position is now air
                            PublicShape.TerrainBitmap[x, y] = TerrainType.Air;
                        }
                    }
                    else
                    {
                        // Reset
                        lowestAirPosition = height;
                    }
                }
            }
         
        }

        private void ExtractTrianglesAndOutline()
        {   
            // Smooth the terrain bitmap
            SmoothTerrain(new float[] { 1, 1, 1, 1, 1 });
            //smoothTerrain(new float[]{1,1,1}); // Use this one if performance is an issue

            // Extract dirt cells and outline triangles/edges from the bitmap
            PerformMarchingSquares();

            // Extract the outline line segments from the aforementioned edges
            ExtractOutlineFromEdges();

            // Combine full dirt cells and create a couple of triangles from them
            ExtractTrianglesFromCells();
        }
        
        public void Update()
        {
            if (WorkingRevision == Revision || WorkingShape != null)
                return;

            int currentRevision = WorkingRevision;

            lock (PublicShape)
            {
                WorkingShape = new TerrainShape
                {
                        TerrainBitmap = PublicShape.TerrainBitmap.Clone() as TerrainType[,]
                };
            }

            // Enqueue update-task
            var Task = new Task(() =>
            {
                ExtractTrianglesAndOutline();
                BuildTerrainTypeTexture();

                lock (PublicShape)
                {
                    // Copy new triangles/outline to TerrainShape
                    PublicShape = WorkingShape;
                    WorkingShape = null;
                    Revision = currentRevision;
                }
            });

            Task.Start();

            if (currentRevision == 1)
                Task.Wait();
        }

        public List<Triangle> GetTriangles(bool enforceUpdate = false)
        {
            Update();

            lock (PublicShape)
            {
                return PublicShape.Triangles;
            }
        }
        
        public List<List<Vector2>> GetOutline(bool enforceUpdate = false)
        {
            Update();

            lock (PublicShape)
            {
                return PublicShape.Outlines;
            }
        }

        Texture2D textureCache;

        public Texture2D GetTerrainTypeTexture()
        {
            Update();

            return textureCache;
        }

        public void BuildTerrainTypeTexture()
        {
            textureCache = new Texture2D(Ballz.The().GraphicsDevice, width, height);
            var typeWeights = new Color[width * height];
            TerrainType[,] types = PublicShape.TerrainBitmap;

            // Build type weights as color values
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int i = x + y * width;
                    var t = types[x, y];
                    Color c;
                    switch (t)
                    {
                        case TerrainType.Earth:
                            c = new Color(255, 0, 0);
                            break;
                        case TerrainType.Sand:
                            c = new Color(0, 255, 0);
                            break;
                        case TerrainType.Stone:
                            c = new Color(0, 0, 255);
                            break;
                        default:
                            c = new Color(0, 0, 0);
                            break;
                    }

                    typeWeights[i] = c;
                }
            }

            // Smooth everything with a 7 pixel gauss kernel (makes non-pixely material edges)
            float[] gauss = new float[]
            {
                0.071303f,
                0.131514f,
                0.189879f,
                0.214607f,
                0.189879f,
                0.131514f,
                0.071303f
            };

            // Gauss X step
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int i = x + y * width;

                    Vector3 c = Vector3.Zero;

                    for (int d = -3; d <= 3; d++)
                    {
                        int ixy = Math.Min(Math.Max(x + d, 0), width - 1) + y * width;
                        c += typeWeights[ixy].ToVector3() * gauss[d + 3];
                    }

                    typeWeights[i] = new Color(c);
                }
            }

            // Gauss Y step
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int i = x + y * width;

                    Vector3 c = Vector3.Zero;

                    for (int d = -3; d <= 3; d++)
                    {
                        int ixy = x + Math.Min(Math.Max(y + d, 0), height - 1) * width;
                        c += typeWeights[ixy].ToVector3() * gauss[d + 3];
                    }

                    typeWeights[i] = new Color(c);
                }
            }

            textureCache.SetData(typeWeights);
        }
        
        public struct IntVector2
        {
            public int X, Y;

            public IntVector2(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }

            public static Vector2 operator *(IntVector2 v, float f) => new Vector2(v.X * f, v.Y * f);
        }

        /*
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
        }
        */
    }
}