﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ballz.Renderer
{
    public class BaseRenderer : DrawableGameComponent
    {
        protected SpriteBatch SpriteBatch;
        protected VertexPositionTexture[] Quad;
        protected SpriteFont Font;

        protected Color SkyColor = new Color(52, 109, 213);
        protected Texture2D SkyTexture;
        protected Texture2D CloudTexture;
        protected Texture2D WhiteTexture;
        protected float CloudSpeed = 10f;

        protected TimeSpan ElapsedTime;

        protected new Ballz Game;

        public BaseRenderer(Ballz game): base(game)
        {
            Game = game;
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            ElapsedTime = gameTime.TotalGameTime;
            Game.Camera.SetAspectRatio(Game.GraphicsDevice.Viewport.AspectRatio);
            Game.Camera.SetProjection(Matrix.Identity);
        }

        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(Game.GraphicsDevice);
            Font = Game.Content.Load<SpriteFont>("Fonts/MenuFont");

            SkyTexture = Game.Content.Load<Texture2D>("Textures/Sky");
            CloudTexture = Game.Content.Load<Texture2D>("Textures/Clouds");

            Quad = new VertexPositionTexture[]
            {
                new VertexPositionTexture(new Vector3(0, 0, 0.5f), new Vector2(0, 1)),
                new VertexPositionTexture(new Vector3(1, 1, 0.5f), new Vector2(1, 0)),
                new VertexPositionTexture(new Vector3(0, 1, 0.5f), new Vector2(0, 0)),
                new VertexPositionTexture(new Vector3(0, 0, 0.5f), new Vector2(0, 1)),
                new VertexPositionTexture(new Vector3(1, 0, 0.5f), new Vector2(1, 1)),
                new VertexPositionTexture(new Vector3(1, 1, 0.5f), new Vector2(1, 0)),
            };

            {
                WhiteTexture = new Texture2D(Game.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
                Color[] color = new Color[1];
                color[0] = Color.White;
                WhiteTexture.SetData(color);
            }

            base.LoadContent();
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
        
        public void DrawText(string text, Vector2 position, float size, Color color, int shadowOffset = 2, bool centerVertical = false, bool centerHorizontal = false)
        {
            if (centerVertical || centerHorizontal)
            {
                var dimensions = Font.MeasureString(text);
                if (centerHorizontal)
                    position.X -= (int)Math.Round(size * (float)dimensions.X / 2f);
                if (centerVertical)
                    position.Y -= (int)Math.Round(size * (float)dimensions.Y / 2f);
            }

            if (shadowOffset > 0)
            {
                position += new Vector2(shadowOffset);
                SpriteBatch.DrawString(Font, text, position, new Color(Color.Black, (color.A/255f) * 0.5f), 0, Vector2.Zero, size, SpriteEffects.None, 0);
                position -= new Vector2(shadowOffset);
            }

            SpriteBatch.DrawString(Font, text, position, color, 0, Vector2.Zero, size, SpriteEffects.None, 0);
        }

        public void DrawSky()
        {
            var cloudPosition = ElapsedTime.TotalSeconds * CloudSpeed;
            GraphicsDevice.Clear(SkyColor);
            SpriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.LinearWrap);
            SpriteBatch.Draw(
                SkyTexture,
                new Rectangle(0, GraphicsDevice.Viewport.Height - SkyTexture.Height, GraphicsDevice.Viewport.Width, SkyTexture.Height),
                new Rectangle((int)cloudPosition, 0, GraphicsDevice.Viewport.Width, SkyTexture.Height),
                Color.White
                );
            SpriteBatch.Draw(
                CloudTexture,
                new Rectangle(0, GraphicsDevice.Viewport.Height - CloudTexture.Height - 80, GraphicsDevice.Viewport.Width, CloudTexture.Height),
                new Rectangle((int)(cloudPosition * 2), 0, GraphicsDevice.Viewport.Width, CloudTexture.Height),
                Color.White
                );
            SpriteBatch.End();
        }
    }
}