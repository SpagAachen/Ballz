using Ballz.GameSession.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ballz.GameSession.Renderer
{
    public class WaterRenderer
    {
        public const int WaterGridSize = 5;
        public const float MaxWaterVelocity = 0.01f;

        public Ballz Game;
        VertexPositionTexture[] quad;

        Effect VectorFieldEffect;
        Effect WaterEffect;

        public WaterRenderer(Ballz game)
        {
            Game = game;
        }


        public void LoadContent()
        {
            VectorFieldEffect = Game.Content.Load<Effect>("Effects/VectorField");
            WaterEffect = Game.Content.Load<Effect>("Effects/Water");

            quad = new VertexPositionTexture[]
            {
                new VertexPositionTexture(new Vector3(0, 0, 0.5f), new Vector2(0, 0)),
                new VertexPositionTexture(new Vector3(1, 1, 0.5f), new Vector2(1, 1)),
                new VertexPositionTexture(new Vector3(0, 1, 0.5f), new Vector2(0, 1)),
                new VertexPositionTexture(new Vector3(0, 0, 0.5f), new Vector2(0, 0)),
                new VertexPositionTexture(new Vector3(1, 0, 0.5f), new Vector2(1, 0)),
                new VertexPositionTexture(new Vector3(1, 1, 0.5f), new Vector2(1, 1)),
            };
        }

        Texture2D WaterTexture = null;
        public Texture2D WaterToTexture(Water water)
        {
            var w = water.Width / WaterGridSize;
            var h = water.Height / WaterGridSize;

            if (WaterTexture == null)
                WaterTexture = new Texture2D(Ballz.The().GraphicsDevice, w, h, false, SurfaceFormat.Color);

            Color[] waterColors = new Color[w * h];
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                {
                    var velocity = water.Velocity(x * WaterGridSize, y * WaterGridSize);
                    velocity /= MaxWaterVelocity;

                    velocity *= 0.5f;
                    velocity += new Vector2(0.5f);

                    if (velocity.X > 1)
                        velocity.X = 1;
                    if (velocity.Y > 1)
                        velocity.Y = 1;

                    waterColors[y * w + x] = new Color(new Vector4(velocity.X, velocity.Y, 0, water[x * WaterGridSize, y * WaterGridSize]));
                }

            WaterTexture.SetData(waterColors);

            return WaterTexture;
        }

        public Vector2 WorldToScreen(Vector3 Position)
        {
            var screenSpace = Vector4.Transform(Position, (Game.Camera.Projection * Game.Camera.View));
            screenSpace /= screenSpace.W;
            return new Vector2
            {
                X = (0.5f + 0.5f * screenSpace.X) * Game.GraphicsDevice.Viewport.Width,
                Y = (1 - (0.5f + 0.5f * screenSpace.Y)) * Game.GraphicsDevice.Viewport.Height,
            };
        }

        public Vector2 WorldToScreen(Vector2 Position)
        {
            return WorldToScreen(new Vector3(Position, 0));
        }

        public void DrawWaterDebug(World.World world)
        {
            var blending = new BlendState();
            blending.AlphaSourceBlend = Blend.SourceAlpha;
            blending.AlphaDestinationBlend = Blend.InverseSourceAlpha;
            blending.ColorSourceBlend = Blend.SourceAlpha;
            blending.ColorDestinationBlend = Blend.InverseSourceAlpha;

            var water = world.Water;

            var w = water.Width / WaterRenderer.WaterGridSize;
            var h = water.Height / WaterRenderer.WaterGridSize;

            var WaterTexture = WaterToTexture(water);

            //var pos = new Vector2(x * debugWorld.StaticGeometry.Scale, y * debugWorld.StaticGeometry.Scale);
            //var sPos = WorldToScreen(pos);

            var topLeft = WorldToScreen(new Vector2(0, h) * world.StaticGeometry.Scale);
            var bottomRight = WorldToScreen(new Vector2(w, 0) * world.StaticGeometry.Scale);

            var destRect = new Rectangle(topLeft.ToPoint(), (bottomRight - topLeft).ToPoint());

            VectorFieldEffect.Techniques[0].Passes[0].Apply();
            VectorFieldEffect.Parameters["VectorField"].SetValue(WaterTexture);
            VectorFieldEffect.Parameters["ArrowSymbol"].SetValue(Game.Content.Load<Texture2D>("Textures/Arrow"));
            VectorFieldEffect.Parameters["GridSize"].SetValue(new Vector2(w, h));
            
            Game.GraphicsDevice.RasterizerState = new RasterizerState
            {
                CullMode = CullMode.None
            };
            Game.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, quad, 0, 2);
        }

        public void DrawWater(World.World world)
        {
            var blending = new BlendState();
            blending.AlphaSourceBlend = Blend.SourceAlpha;
            blending.AlphaDestinationBlend = Blend.InverseSourceAlpha;
            blending.ColorSourceBlend = Blend.SourceAlpha;
            blending.ColorDestinationBlend = Blend.InverseSourceAlpha;

            var water = world.Water;

            var w = water.Width / WaterGridSize;
            var h = water.Height / WaterGridSize;

            var WaterTexture = WaterToTexture(water);
            
            var topLeft = WorldToScreen(new Vector2(0, h) * world.StaticGeometry.Scale);
            var bottomRight = WorldToScreen(new Vector2(w, 0) * world.StaticGeometry.Scale);

            var destRect = new Rectangle(topLeft.ToPoint(), (bottomRight - topLeft).ToPoint());

            WaterEffect.Techniques[0].Passes[0].Apply();

            WaterEffect.Parameters["WaterTexture"].SetValue(WaterTexture);

            Game.GraphicsDevice.RasterizerState = new RasterizerState
            {
                CullMode = CullMode.None
            };
            Game.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, quad, 0, 2);
        }
    }
}
