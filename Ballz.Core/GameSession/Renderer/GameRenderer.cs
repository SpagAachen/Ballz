using Ballz.Messages;
using Ballz.GameSession.World;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;

using static MathFloat.MathF;
using Ballz.Utils;

namespace Ballz.GameSession.Renderer
{
    /// <summary>
    ///     Render system performs all rendering of the Game and is inteded as a module.
    /// </summary>
    public class GameRenderer : DrawableGameComponent
    {
        Model BallModel, GraveModel;
        Texture2D GermoneyTexture;
        Texture2D CrosshairTexture;
        Texture2D TerrainTexture;
        BasicEffect BallEffect, TerrainEffect, GraveEffect;
        SpriteBatch spriteBatch;

        private SpriteFont font;

        new Ballz Game;

        TimeSpan lastModification;

        public GameRenderer(Ballz game) : base(game)
        {
            Game = game;
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

        /// <summary>
        ///     Draw the game for the specified _time.
        /// </summary>
        /// <param name="time">time since start of game (cf BallzGame draw).</param>
        public override void Draw(GameTime time)
        { using (new PerformanceReporter(Game))
            {
                if (lastModification == null)
                    lastModification = time.TotalGameTime;
                Game.Camera.setProjection(Matrix.Identity);

                if (Game.Match.State == Logic.SessionState.Finished)
                {
                    string msg = "";

                    if (Game.Match.Winner != null)
                        msg = Game.Match.Winner.Name + " won the match!";
                    else
                        msg = "Draw!";

                    spriteBatch.Begin();
                    var screenPos = new Vector2(30, Game.GraphicsDevice.Viewport.Height / 2);
                    spriteBatch.DrawString(font, msg, screenPos, Color.Black, 0, Vector2.Zero, 1.0f, SpriteEffects.None, 0);
                    screenPos += new Vector2(2, 2);
                    spriteBatch.DrawString(font, msg, screenPos, Color.Red, 0, Vector2.Zero, 1.0f, SpriteEffects.None, 0);
                    spriteBatch.End();
                }

                Game.Camera.setView(Matrix.CreateOrthographicOffCenter(0, 40, 0, 40 / Game.GraphicsDevice.Viewport.AspectRatio, -20, 20));

                BallEffect.View = Game.Camera.View;
                BallEffect.Projection = Game.Camera.Projection;

                var worldState = Game.World;

                var tris = worldState.StaticGeometry.getTriangles();
                VertexPositionColorTexture[] vpc = new VertexPositionColorTexture[tris.Count * 3];
            
                int i = 0;

                float TerrainTextureScale = 0.01f;

                foreach (var t in tris)
                {
                    vpc[i + 0].Color = Color.Maroon;
                    vpc[i + 0].Position = new Vector3(t.a.X, t.a.Y, -1);
                    vpc[i + 0].TextureCoordinate = new Vector2(t.a.X, t.a.Y) * TerrainTextureScale;
                    vpc[i + 1].Color = Color.Maroon;
                    vpc[i + 1].Position = new Vector3(t.b.X, t.b.Y, -1);
                    vpc[i + 1].TextureCoordinate = new Vector2(t.b.X, t.b.Y) * TerrainTextureScale;
                    vpc[i + 2].Color = Color.Maroon;
                    vpc[i + 2].Position = new Vector3(t.c.X, t.c.Y, -1);
                    vpc[i + 2].TextureCoordinate = new Vector2(t.c.X, t.c.Y) * TerrainTextureScale;
                    i += 3;
                }

                Matrix terrainWorld = Matrix.CreateScale(worldState.StaticGeometry.Scale);
                TerrainEffect.World = terrainWorld;
                TerrainEffect.View = Game.Camera.View;
                TerrainEffect.Projection = Game.Camera.Projection;
                TerrainEffect.CurrentTechnique.Passes[0].Apply();
                GraphicsDevice.SamplerStates[0] = new SamplerState
                {
                    AddressU = TextureAddressMode.Wrap,
                    AddressV = TextureAddressMode.Wrap
                };

                GraphicsDevice.DrawUserPrimitives<VertexPositionColorTexture>(PrimitiveType.TriangleList, vpc, 0, tris.Count);

                spriteBatch.Begin();
                foreach (var entity in worldState.Entities)
                {
                    Vector2 nV = entity.Direction;
                    Matrix world = Matrix.CreateRotationY((float)(2 * Math.PI * 50 * nV.X / 360.0)) * Matrix.CreateTranslation(new Vector3(entity.Position, 0));
                    BallEffect.World = world;
                    GraveEffect.World = world;


                    var ball = entity as Ball;
                    if (ball != null)
                    {
                        if (ball.Health > 0)
                            BallModel.Draw(world, Game.Camera.View, Game.Camera.Projection);
                        else
                            GraveModel.Draw(world, Game.Camera.View, Game.Camera.Projection);

                        if (ball.IsAiming)
                        {
                            var aimTarget = ball.Position + ball.AimDirection * 2;
                            var aimTargetScreen = WorldToScreen(aimTarget);
                            var crossHairRectangle = new Rectangle(aimTargetScreen.ToPoint() - new Point(16, 16), new Point(32, 32));
                            spriteBatch.Draw(CrosshairTexture, crossHairRectangle, Color.White);
                        }

                        var screenPos = WorldToScreen(ball.Position + new Vector2(0.33f, 1.5f));
                    
                        spriteBatch.DrawString(font, ball.Player.Name, screenPos, Color.Black, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
                        screenPos += new Vector2(2, 2);
                        spriteBatch.DrawString(font, ball.Player.Name, screenPos, Color.MediumSpringGreen, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);

                        screenPos = WorldToScreen(ball.Position + new Vector2(0.33f, 1.5f));
                        screenPos += new Vector2(0, 20);
                        spriteBatch.DrawString(font, ball.Health.ToString("0"), screenPos, Color.Black, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
                        screenPos += new Vector2(2, 2);
                        spriteBatch.DrawString(font, ball.Health.ToString("0"), screenPos, Color.White, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);


                    }
                }
                spriteBatch.End();
            }
        }

        protected override void LoadContent()
        {
            GermoneyTexture = Game.Content.Load<Texture2D>("Textures/Germoney");
            CrosshairTexture = Game.Content.Load<Texture2D>("Textures/Crosshair");

            BallEffect = new BasicEffect(Game.GraphicsDevice);
            BallEffect.EnableDefaultLighting();
            BallEffect.Texture = GermoneyTexture;
            BallEffect.TextureEnabled = true;
            BallEffect.DirectionalLight0.Direction = new Vector3(1, -1, -1);
            BallEffect.AmbientLightColor = new Vector3(0.3f, 0.3f, 0.3f);
            BallEffect.PreferPerPixelLighting = true;

            BallModel = Game.Content.Load<Model>("Models/Ball");
            BallModel.Meshes[0].MeshParts[0].Effect = BallEffect;

            TerrainTexture = Game.Content.Load<Texture2D>("Textures/Dirt");

            TerrainEffect = new BasicEffect(Game.GraphicsDevice);
            TerrainEffect.LightingEnabled = false;
            TerrainEffect.Texture = TerrainTexture;
            TerrainEffect.TextureEnabled = true;

            spriteBatch = new SpriteBatch(Game.GraphicsDevice);
            font = Game.Content.Load<SpriteFont>("Fonts/Menufont");

            GraveEffect = new BasicEffect(Game.GraphicsDevice);
            GraveEffect.EnableDefaultLighting();
            GraveEffect.Texture = Game.Content.Load<Texture2D>("Textures/RIP");
            GraveEffect.TextureEnabled = true;
            GraveEffect.DirectionalLight0.Direction = new Vector3(1,-1,-1);
            GraveEffect.AmbientLightColor = new Vector3(0.3f);
            GraveEffect.PreferPerPixelLighting = true;

            GraveModel = Game.Content.Load<Model>("Models/RIP");
            GraveModel.Meshes[0].MeshParts[0].Effect = GraveEffect;

            //PrepareDebugRendering();

            base.LoadContent();
        }

        public void HandleMessage(object sender, Message message)
        {
            //throw new NotImplementedException ();
            if (message.Kind != Message.MessageType.LogicMessage)
                return;
            LogicMessage msg = (LogicMessage)message;

            //see if the message was meant for us
            if (msg.Kind == LogicMessage.MessageType.GameMessage)
            {
                Enabled = !Enabled;
                Visible = !Visible;
            }
        }
    }
}