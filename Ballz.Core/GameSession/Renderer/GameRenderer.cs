using Ballz.GameSession.World;
using Ballz.Messages;
using Ballz.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using System.Collections.Generic;
using static MathFloat.MathF;

namespace Ballz.GameSession.Renderer
{
    /// <summary>
    ///     Render system performs all rendering of the Game and is inteded as a module.
    /// </summary>
    public class GameRenderer : DrawableGameComponent
    {
        Model BallModel, GraveModel;
        Dictionary<string,Texture2D> TeamTextures;
        Texture2D CrosshairTexture;
        Texture2D TerrainTexture;
        Texture2D WhiteTexture;
        BasicEffect BallEffect, TerrainEffect, GraveEffect, RopeEffect;
        Effect CelShading;
        RenderTarget2D WorldRenderTarget;
        SpriteBatch spriteBatch;
        VertexPositionTexture[] quad;
        SpriteFont font;

		Ball CurrentActiveBall = null;

        new Ballz Game;

        TimeSpan elTime;

        WaterRenderer WaterRenderer;

        public GameRenderer(Ballz game) : base(game)
        {
            Game = game;
            WaterRenderer = new WaterRenderer(game);
            TeamTextures = new Dictionary<string, Texture2D>();
            Game.Camera.SetAspectRatio(Game.GraphicsDevice.Viewport.AspectRatio);
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

        /// <summary>
        ///     Draw the game for the specified _time.
        /// </summary>
        /// <param name="time">time since start of game (cf BallzGame draw).</param>
        public override void Draw(GameTime time)
        {
            using (new PerformanceReporter(Game))
            {
                WaterRenderer.PrepareDrawWater(Game.World);
                //GraphicsDevice.SetRenderTarget(WorldRenderTarget);
                GraphicsDevice.Clear(Color.CornflowerBlue);
                elTime = time.TotalGameTime;
                Game.Camera.SetProjection(Matrix.Identity);

                try
                {
					if ( Game.Match.ActivePlayer.ActiveBall != CurrentActiveBall && Game.Match.ActivePlayer.ActiveBall != null)
					{
						CurrentActiveBall = Game.Match.ActivePlayer.ActiveBall;
						Game.Camera.SwitchTarget(CurrentActiveBall.Position, time);
					}
                    Game.Camera.SetTargetPosition((Vector2)Game.Match.ActivePlayer.ActiveBall?.Position, time);
                }
                catch(Exception) {
                    Game.Camera.SetView(Matrix.CreateOrthographicOffCenter(0, 40, 0, 40 / Game.GraphicsDevice.Viewport.AspectRatio, -20, 20));
                }

                BallEffect.View = Game.Camera.View;
                BallEffect.Projection = Game.Camera.Projection;

                var worldState = Game.World;

                spriteBatch.Begin();
                spriteBatch.Draw(WhiteTexture, new Rectangle(0, 0, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height), Color.CornflowerBlue);
                spriteBatch.End();

                var tris = worldState.StaticGeometry.GetTriangles();
                VertexPositionColorTexture[] vpc = new VertexPositionColorTexture[tris.Count * 3];

                int i = 0;

                float TerrainTextureScale = 0.01f;

                foreach (var t in tris)
                {
                    vpc[i + 0].Color = Color.Maroon;
                    vpc[i + 0].Position = new Vector3(t.A.X, t.A.Y, -1);
                    vpc[i + 0].TextureCoordinate = new Vector2(t.A.X, t.A.Y) * TerrainTextureScale;
                    vpc[i + 1].Color = Color.Maroon;
                    vpc[i + 1].Position = new Vector3(t.B.X, t.B.Y, -1);
                    vpc[i + 1].TextureCoordinate = new Vector2(t.B.X, t.B.Y) * TerrainTextureScale;
                    vpc[i + 2].Color = Color.Maroon;
                    vpc[i + 2].Position = new Vector3(t.C.X, t.C.Y, -1);
                    vpc[i + 2].TextureCoordinate = new Vector2(t.C.X, t.C.Y) * TerrainTextureScale;
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

                var blending = new BlendState
                {
                    AlphaSourceBlend = Blend.SourceAlpha,
                    AlphaDestinationBlend = Blend.InverseSourceAlpha,
                    ColorSourceBlend = Blend.SourceAlpha,
                    ColorDestinationBlend = Blend.InverseSourceAlpha,
                };

                spriteBatch.Begin(blendState: blending);
                foreach (var entity in worldState.Entities)
                {
                    if (entity.Disposed)
                        continue;

                    var ball = entity as Ball;
                    if (ball != null)
                        DrawBall(ball);
                    var shot = entity as Shot;
                    if (shot != null)
                        DrawShot(shot);
                }

                spriteBatch.End();

                WaterRenderer.DrawWater(worldState);

                GraphicsDevice.SetRenderTarget(null);
                
                PostProcess();

                DrawMessageOverlay();
            }
        }

        public void PostProcess()
        {
            /*CelShading.Techniques[0].Passes[0].Apply();

            CelShading.Parameters["InputTexture"].SetValue(WorldRenderTarget);

            Game.GraphicsDevice.RasterizerState = new RasterizerState
            {
                CullMode = CullMode.None
            };
            Game.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, quad, 0, 2);*/
        }

        public void DrawBall(Ball ball)
        {
            if (ball.AttachedRope != null)
                DrawRope(ball.AttachedRope);

            BallEffect.DiffuseColor = Vector3.One;
            if(ball.Player.TeamName != null)
                BallEffect.Texture = TeamTextures[ball.Player.TeamName];

            Vector2 nV = ball.Direction;
            Matrix world = Matrix.CreateRotationY((float)(2 * Math.PI * 50 * nV.X / 360.0)) * Matrix.CreateTranslation(new Vector3(ball.Position, 0));
            BallEffect.World = world;
            GraveEffect.World = world * Matrix.CreateScale(0.3f);

            if (ball.Health > 0)
            {
                BallModel.Draw(world, Game.Camera.View, Game.Camera.Projection);

                var aimTarget = ball.Position + ball.AimDirection * 2;
                var aimTargetScreen = WorldToScreen(aimTarget);
                var aimRotation = ball.AimDirection.RotationFromDirection();

                var effects =  SpriteEffects.None;

                if (!string.IsNullOrEmpty(ball.HoldingWeapon))
                {
                    var weaponRotation = aimRotation;
                    if (ball.AimDirection.X < 0)
                    {
                        effects = SpriteEffects.FlipHorizontally;
                        weaponRotation += (float)Math.PI;
                    }

                    var weaponPosScreen = WorldToScreen(ball.Position - new Vector2(0, 0.33f));
                    var weaponTexture = Game.Content.Load<Texture2D>("Textures/" + ball.HoldingWeapon);
                    var weaponTextureScale = 256f / weaponTexture.Width;

                    // Draw weapon
                    spriteBatch.Draw(weaponTexture, position: weaponPosScreen, color: Color.White, rotation: weaponRotation, scale: new Vector2(weaponTextureScale, weaponTextureScale), origin: new Vector2(weaponTexture.Width / 2f, weaponTexture.Height / 2f), effects: effects);

                }

                if (ball.IsAiming)
                {
                    int width = (int)(ball.ShootCharge * 100);
                    var aimIndicator = ball.Position + ball.AimDirection * 2.1f;
                    var aimIndicatorScreen = WorldToScreen(aimIndicator);
                    var aimIndicatorSize = new Vector2(width, 20);

                    if (ball.ShootCharge > 0)
                    {
                        var chargeColor = GetChargeColor(ball.ShootCharge);

                        // Draw charge indicator
                        spriteBatch.Draw(WhiteTexture, position: aimIndicatorScreen, scale: new Vector2(100, 20), color: new Color(Color.Black, (int)(64*ball.ShootCharge)), rotation: aimRotation, origin: new Vector2(0, 0.5f));
                        spriteBatch.Draw(WhiteTexture, position: aimIndicatorScreen, scale: aimIndicatorSize, color: new Color(chargeColor), rotation: aimRotation, origin: new Vector2(0, 0.5f));
                    }
                    // Draw crosshair
                    spriteBatch.Draw(CrosshairTexture, position: aimTargetScreen, color: Color.White, rotation: aimRotation, origin: new Vector2(16, 16));
                }
            }
            else // Player is dead
            {
                GraveModel.Draw(world, Game.Camera.View, Game.Camera.Projection);
            }
            
            var screenPos = WorldToScreen(ball.Position + new Vector2(0, 2f));

            DrawText(ball.Player.Name, screenPos, 0.5f, Color.LawnGreen, 1, true, true);
            screenPos += new Vector2(0, 20);
            DrawText(ball.Health.ToString("0"), screenPos, 0.5f, Color.White, 1, true, true);

            if (Game.Match.UsePlayerTurns && Game.Match.ActivePlayer == ball.Player && ball.Player.ActiveBall == ball)
            {
                // Show turn-indicator for a couple of seconds only
                var timeLeft = Game.Match.TurnTimeLeft;
                if (Session.SecondsPerTurn - timeLeft < 4)
                {
                    screenPos -= new Vector2(0, 30 + (float)(15 * Math.Sin(5 * elTime.TotalSeconds)));
                    spriteBatch.Draw(Game.Content.Load<Texture2D>("Textures/RedArrow"), screenPos, color: Color.White, origin: new Vector2(29, 38));
                }
            }
        }

        /// <summary>
        /// Returns a nice color between red and green for given inputs from [0..1].
        /// </summary>
        private Vector4 GetChargeColor(float charge)
        {
            var c0 = new Vector4(1, 0, 0, 1);
            var c1 = new Vector4(1, 0.8f, 0, 1);
            var c2 = new Vector4(0, 0.8f, 0, 1);

            if (charge < 0.5f)
            {
                var t = Max(0, Min(charge * 2, 1f));
                return c0 * (1 - t) + c1 * t;
            }
            else
            {
                var t = Max(0, Min((charge - 0.5f) * 2, 1f));
                return c1 * (1 - t) + c2 * t;
            }
        }

        public void DrawShot(Shot shot)
        {
            BallEffect.DiffuseColor = Vector3.Zero;
            Matrix world = Matrix.CreateScale(0.1f) * Matrix.CreateTranslation(new Vector3(shot.Position, 0));
            BallModel.Draw(world, Game.Camera.View, Game.Camera.Projection);
        }

        const float RopeWidth = Rope.Diameter;

        public void DrawRope(Rope rope)
        {
            //var segmentPositions = (from s in rope.PhysicsSegments select s.Position).ToArray();
            //var segmentPositions = (from s in rope.PhysicsSegments select s.GetWorldPoint(new Vector2(0, 0.5f))).ToArray();

            var segmentPositionsList = (from s in rope.PhysicsSegmentJoints select s.WorldAnchorA).ToList();
            segmentPositionsList.Insert(0, rope.AttachedPosition);
            segmentPositionsList.Add(rope.AttachedEntity.Position);
            var segmentPositions = segmentPositionsList.ToArray();

            var triangleCount = (segmentPositions.Length - 1) * 2;
            
            VertexPositionColorTexture[] vpc = new VertexPositionColorTexture[triangleCount * 3];
            
            float u = 0;

            for(int i = 0; i < segmentPositions.Length - 1; i++)
            {
                var p0 = segmentPositions[i + 1];
                var p1 = segmentPositions[i];
                var d = Vector2.Normalize(p1 - p0);
                var n = new Vector2(d.Y, -d.X);
                
                var u0 = u - d.Length() * 0.05f;
                var u1 = u + (p1 - p0).Length() + d.Length() * 0.05f;

                p0 -= d * 0.05f;
                p1 += d * 0.05f;

                var t00 = p0 + n * RopeWidth;
                var t10 = p1 + n * RopeWidth;
                var t01 = p0 - n * RopeWidth;
                var t11 = p1 - n * RopeWidth;

                vpc[i * 6 + 0].Color = Color.White;
                vpc[i * 6 + 0].Position = new Vector3(t11, -1);
                vpc[i * 6 + 0].TextureCoordinate = new Vector2(1, u1 / (2f * RopeWidth));
                vpc[i * 6 + 1].Color = Color.White;
                vpc[i * 6 + 1].Position = new Vector3(t10, -1);
                vpc[i * 6 + 1].TextureCoordinate = new Vector2(0, u1 / (2f * RopeWidth));
                vpc[i * 6 + 2].Color = Color.White;
                vpc[i * 6 + 2].Position = new Vector3(t00, -1);
                vpc[i * 6 + 2].TextureCoordinate = new Vector2(0, u0 / (2f * RopeWidth));

                vpc[i * 6 + 3].Color = Color.White;
                vpc[i * 6 + 3].Position = new Vector3(t01, -1);
                vpc[i * 6 + 3].TextureCoordinate = new Vector2(1, u0 / (2f * RopeWidth));
                vpc[i * 6 + 4].Color = Color.White;
                vpc[i * 6 + 4].Position = new Vector3(t11, -1);
                vpc[i * 6 + 4].TextureCoordinate = new Vector2(1, u1 / (2f * RopeWidth));
                vpc[i * 6 + 5].Color = Color.White;
                vpc[i * 6 + 5].Position = new Vector3(t00, -1);
                vpc[i * 6 + 5].TextureCoordinate = new Vector2(0, u0 / (2f * RopeWidth));

                u = u1;
            }
            
            RopeEffect.World = Matrix.Identity;
            RopeEffect.View = Game.Camera.View;
            RopeEffect.Projection = Game.Camera.Projection;
            RopeEffect.CurrentTechnique.Passes[0].Apply();
            GraphicsDevice.SamplerStates[0] = new SamplerState
            {
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap
            };

            GraphicsDevice.DrawUserPrimitives<VertexPositionColorTexture>(PrimitiveType.TriangleList, vpc, 0, triangleCount);
        }

        public void DrawMessageOverlay()
        {
            spriteBatch.Begin();
            if (Game.Match.State == Logic.SessionState.Finished)
            {
                string msg = "";

                if (Game.Match.Winner != null)
                    msg = Game.Match.Winner.Name + " won the match!";
                else
                    msg = "Draw!";
                
                spriteBatch.Draw(WhiteTexture, new Rectangle(0, 0, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height), new Color(Color.Black, 0.5f));

                var screenPos = new Vector2(Game.GraphicsDevice.Viewport.Width / 2, Game.GraphicsDevice.Viewport.Height / 2);
                DrawText(msg, screenPos, 1f, Color.Red, centerHorizontal: true);
            }
            else if(Game.Match.UsePlayerTurns && Game.Match.ActivePlayer != null)
            {
                var screenPos = new Vector2(Game.GraphicsDevice.Viewport.Width - 250, Game.GraphicsDevice.Viewport.Height - 100);

                var msg = "Turn: " + Game.Match.ActivePlayer.Name + " / " + (int)Game.Match.TurnTimeLeft;

                DrawText(msg, screenPos, 1f, Color.Black, centerHorizontal: true);
            }

            spriteBatch.End();
        }

        public void DrawText(string text, Vector2 position, float size, Color color, int shadowOffset=2, bool centerVertical = false, bool centerHorizontal = false)
        {
            if(centerVertical || centerHorizontal)
            {
                var dimensions = font.MeasureString(text);
                if(centerHorizontal)
                    position.X -= (int)Math.Round(size * (float)dimensions.X / 2f);
                if (centerVertical)
                    position.Y -= (int)Math.Round(size * (float)dimensions.Y / 2f);
            }

            if(shadowOffset > 0)
            {
                position += new Vector2(shadowOffset);
                spriteBatch.DrawString(font, text, position, new Color(Color.Black, 0.5f), 0, Vector2.Zero, size, SpriteEffects.None, 0);
                position -= new Vector2(shadowOffset);
            }

            spriteBatch.DrawString(font, text, position, color, 0, Vector2.Zero, size, SpriteEffects.None, 0);
        }
        
        protected override void LoadContent()
        {
            //load a texture for each team
            foreach (string s in Game.Teamnames)
            {
                TeamTextures.Add(s,Game.Content.Load<Texture2D>("Textures/Teams/"+s));
            }

            CrosshairTexture = Game.Content.Load<Texture2D>("Textures/Crosshair");

            BallEffect = new BasicEffect(Game.GraphicsDevice);
            BallEffect.EnableDefaultLighting();

            BallEffect.Texture = TeamTextures.Values.ElementAt(0);    //default texture
            BallEffect.TextureEnabled = true;
            BallEffect.DirectionalLight0.Direction = new Vector3(1, -1, -1);
            BallEffect.AmbientLightColor = new Vector3(0.3f, 0.3f, 0.3f);
            BallEffect.PreferPerPixelLighting = true;
            BallEffect.SpecularColor = new Vector3(0.5f, 0.5f, 0.5f);
            BallEffect.SpecularPower = 4;

            BallModel = Game.Content.Load<Model>("Models/Ball");
            BallModel.Meshes[0].MeshParts[0].Effect = BallEffect;

            TerrainTexture = Game.Content.Load<Texture2D>("Textures/Dirt");

            TerrainEffect = new BasicEffect(Game.GraphicsDevice);
            TerrainEffect.LightingEnabled = false;
            TerrainEffect.Texture = TerrainTexture;
            TerrainEffect.TextureEnabled = true;

            RopeEffect = new BasicEffect(Game.GraphicsDevice);
            RopeEffect.LightingEnabled = false;
            RopeEffect.Texture = Game.Content.Load<Texture2D>("Textures/Rope"); 
            RopeEffect.TextureEnabled = true;

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

            //CelShading = Game.Content.Load<Effect>("Effects/CelShading");

            {
                WhiteTexture = new Texture2D(Game.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
                Color[] color = new Color[1];
                color[0] = Color.White;
                WhiteTexture.SetData(color);
            }
            
            WaterRenderer.LoadContent();
            quad = new VertexPositionTexture[]
            {
                new VertexPositionTexture(new Vector3(0, 0, 0.5f), new Vector2(0, 1)),
                new VertexPositionTexture(new Vector3(1, 1, 0.5f), new Vector2(1, 0)),
                new VertexPositionTexture(new Vector3(0, 1, 0.5f), new Vector2(0, 0)),
                new VertexPositionTexture(new Vector3(0, 0, 0.5f), new Vector2(0, 1)),
                new VertexPositionTexture(new Vector3(1, 0, 0.5f), new Vector2(1, 1)),
                new VertexPositionTexture(new Vector3(1, 1, 0.5f), new Vector2(1, 0)),
            };

            WorldRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
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