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
        public Ballz Game;
        BasicEffect ParticleEffect;

        public WaterRenderer(Ballz game)
        {
            Game = game;
        }


        public void LoadContent()
        {
            ParticleEffect = new BasicEffect(Game.GraphicsDevice);
            ParticleEffect.EnableDefaultLighting();
            ParticleEffect.DiffuseColor = new Vector3(1, 1, 1);
            ParticleEffect.VertexColorEnabled = true;
            ParticleEffect.LightingEnabled = false;
            ParticleEffect.TextureEnabled = false;
        }

        const float DebugParticleSize = 0.05f;

        public void DrawWaterDebug(World.World world)
        {
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

        public void DrawWater(World.World world)
        {
            
        }
    }
}
