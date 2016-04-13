﻿using Ballz.GameSession.World;
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
        public static readonly bool NOWATER = true;

        public Ballz Game;
        BasicEffect ParticleEffect;
        Effect WaterEffect;
        SpriteBatch spriteBatch;
        Texture2D MetaBallTexture;
        const int MetaBallRadius = 32;
        const int MetaBallWidth = MetaBallRadius * 2;
        RenderTarget2D WaterRenderTarget;
        VertexPositionTexture[] FullscreenQuad;

        public WaterRenderer(Ballz game)
        {
            Game = game;
        }

        public void LoadContent()
        {
            if (NOWATER)
                return;
            
            spriteBatch = new SpriteBatch(Game.GraphicsDevice);

            ParticleEffect = new BasicEffect(Game.GraphicsDevice);
            ParticleEffect.EnableDefaultLighting();
            ParticleEffect.DiffuseColor = new Vector3(1, 1, 1);
            ParticleEffect.VertexColorEnabled = true;
            ParticleEffect.LightingEnabled = false;
            ParticleEffect.TextureEnabled = false;

            WaterEffect = Game.Content.Load<Effect>("Effects/Water");

            WaterRenderTarget = new RenderTarget2D(Game.GraphicsDevice, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height);

            MetaBallTexture = new Texture2D(Game.GraphicsDevice, MetaBallWidth, MetaBallWidth);
            var metaBallData = new byte[MetaBallWidth * MetaBallWidth * 4];
            int i = 0;
            for(int x = 0; x < MetaBallWidth; x++)
            {
                for(int y = 0; y < MetaBallWidth; y++)
                {
                    var dx = x - MetaBallRadius;
                    var dy = y - MetaBallRadius;
                    var distance = Math.Sqrt(dx * dx + dy * dy) / MetaBallRadius;
                    var blob = 1 - (0.1 + 0.9 * distance);
                    var b = (byte)(127 * Math.Max(0, Math.Min(1, blob)));
                    metaBallData[i++] = b;
                    metaBallData[i++] = b;
                    metaBallData[i++] = b;
                    metaBallData[i++] = 255;// (byte)(d > 14 ? 0 : 128);
                }
            }
            MetaBallTexture.SetData(metaBallData);

            FullscreenQuad = new VertexPositionTexture[]
            {
                new VertexPositionTexture(new Vector3(0, 0, 0.5f), new Vector2(0, 1)),
                new VertexPositionTexture(new Vector3(1, 1, 0.5f), new Vector2(1, 0)),
                new VertexPositionTexture(new Vector3(0, 1, 0.5f), new Vector2(0, 0)),
                new VertexPositionTexture(new Vector3(0, 0, 0.5f), new Vector2(0, 1)),
                new VertexPositionTexture(new Vector3(1, 0, 0.5f), new Vector2(1, 1)),
                new VertexPositionTexture(new Vector3(1, 1, 0.5f), new Vector2(1, 0)),
            };
        }

        const float DebugParticleSize = 0.05f;

        public void DrawWaterDebug(World.World world)
        {
            if (NOWATER)
                return;
            
            var blending = new BlendState();
            blending.AlphaSourceBlend = Blend.SourceAlpha;
            blending.AlphaDestinationBlend = Blend.InverseSourceAlpha;
            blending.ColorSourceBlend = Blend.SourceAlpha;
            blending.ColorDestinationBlend = Blend.InverseSourceAlpha;

            var water = world.Water;
            var particles = water.Particles;

            VertexPositionColor[] lines = new VertexPositionColor[particles.Length * 6];

            for(int i = 0; i < particles.Length; i++)
            {
                var bottom = particles[i] + new Vector2(0, -DebugParticleSize);
                var right = particles[i] + new Vector2(-DebugParticleSize, DebugParticleSize);
                var left = particles[i] + new Vector2(DebugParticleSize, DebugParticleSize);
                lines[i * 6 + 0].Position = new Vector3(bottom, 0f);
                lines[i * 6 + 1].Position = new Vector3(right, 0f);
                lines[i * 6 + 2].Position = new Vector3(right, 0f);
                lines[i * 6 + 3].Position = new Vector3(left, 0f);
                lines[i * 6 + 4].Position = new Vector3(left, 0f);
                lines[i * 6 + 5].Position = new Vector3(bottom, 0f);

                lines[i * 6 + 0].Color = Color.White;
                lines[i * 6 + 1].Color = Color.White;
                lines[i * 6 + 2].Color = Color.White;
                lines[i * 6 + 3].Color = Color.White;
                lines[i * 6 + 4].Color = Color.White;
                lines[i * 6 + 5].Color = Color.White;
            }

            Game.GraphicsDevice.RasterizerState = new RasterizerState
            {
                CullMode = CullMode.None
            };
            Game.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, lines, 0, particles.Length * 3);
        }

        public Vector2 WorldToScreen(Vector3 Position)
        {
            var screenSpace = Vector4.Transform(Position, Game.Camera.Projection * Game.Camera.View);
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

        public void PrepareDrawWater(World.World world)
        {
            if (NOWATER)
                return;
            
            var blending = BlendState.Additive;

            var water = world.Water;
            var particles = water.Particles;

            Game.GraphicsDevice.SetRenderTarget(WaterRenderTarget);
            Game.GraphicsDevice.Clear(Color.Transparent);

            spriteBatch.Begin(SpriteSortMode.Deferred, blending);

            for (int i = 0; i < particles.Length; i++)
            {
                var pos = WorldToScreen(particles[i]);
                spriteBatch.Draw(MetaBallTexture, position: pos, origin: new Vector2(MetaBallRadius, MetaBallRadius));
            }

            spriteBatch.End();

            Game.GraphicsDevice.SetRenderTarget(null);
        }

        public void DrawWater(World.World world)
        {
            if (NOWATER)
                return;
            
            var waterDomainScreenPos = WorldToScreen(Vector2.Zero);
            WaterEffect.Parameters["WaterTexture"].SetValue(WaterRenderTarget);
            WaterEffect.Techniques[0].Passes[0].Apply();
            var oldRasterizerState = Game.GraphicsDevice.RasterizerState;
            Game.GraphicsDevice.RasterizerState = new RasterizerState
            {
                CullMode = CullMode.None,
            };
            var oldDepthState = Game.GraphicsDevice.DepthStencilState;
            Game.GraphicsDevice.DepthStencilState = new DepthStencilState
            {
                DepthBufferEnable = false,
                DepthBufferWriteEnable = false
            };
            Game.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, FullscreenQuad, 0, 2);
            Game.GraphicsDevice.DepthStencilState = oldDepthState;
            Game.GraphicsDevice.RasterizerState = oldRasterizerState;
        }
    }
}
