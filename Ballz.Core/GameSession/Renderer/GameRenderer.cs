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
using Ballz.Renderer;

namespace Ballz.GameSession.Renderer
{
    /// <summary>
    ///     Render system performs all rendering of the Game and is inteded as a module.
    /// </summary>
    public class GameRenderer: BaseRenderer
    {
        Model BallModel, GraveModel;
        Dictionary<string,Texture2D> TeamTextures;
        Texture2D CrosshairTexture;
        Texture2D EarthTexture;
        Texture2D SandTexture;
        Texture2D StoneTexture;

        BasicEffect BallEffect, GraveEffect, RopeEffect;
        Effect CelShading, TerrainEffect;
        RenderTarget2D WorldRenderTarget;

        Random random = new Random();

		Ball CurrentActiveBall = null;

        WaterRenderer WaterRenderer;

        public GameRenderer(Ballz game) : base(game)
        {
            WaterRenderer = new WaterRenderer(game);
            TeamTextures = new Dictionary<string, Texture2D>();
        }


        /// <summary>
        ///     Draw the game for the specified _time.
        /// </summary>
        /// <param name="time">time since start of game (cf BallzGame draw).</param>
        public override void Draw(GameTime time)
        {
            using (new PerformanceReporter(Game))
            {
                base.Draw(time);
                var worldState = Game.Match.World;
                WaterRenderer.PrepareDrawWater(Game.Match.World);
                //GraphicsDevice.SetRenderTarget(WorldRenderTarget);

                Game.Camera.UseBoundary = true;
                Game.Camera.BottomLeftBoundary = new Vector2(-100f, 0f);
                Game.Camera.TopRightBoundary = new Vector2(100f, 100f);

                if (Game.Match.UsePlayerTurns && Game.Match.ActivePlayer?.ActiveBall != null)
                {
                    if (CurrentActiveBall == null)
                        CurrentActiveBall = Game.Match.ActivePlayer?.ActiveBall;

                    if (CurrentActiveBall != null && Game.Match.TurnState == TurnState.Running && Game.Match.ActivePlayer?.ActiveBall != CurrentActiveBall && Game.Match.ActivePlayer?.ActiveBall != null)
                    {
                        CurrentActiveBall = Game.Match.ActivePlayer.ActiveBall;
                        Game.Camera.SwitchTarget(CurrentActiveBall.Position, time);
                    }
                    Game.Camera.SetTargetPosition((Vector2)Game.Match.ActivePlayer.ActiveBall.Position, time);
                }
                else
                {
                    Game.Camera.SetView(Matrix.CreateOrthographicOffCenter(0, 40, 0, 40 / Game.GraphicsDevice.Viewport.AspectRatio, -20, 20));
                }

                var shakeEffect = worldState.GraphicsEvents.FirstOrDefault(e => e is CameraShakeEffect) as CameraShakeEffect;
                if(shakeEffect != null)
                {
                    var intensity = (1 - shakeEffect.GetProgress(Game.Match.GameTime)) * shakeEffect.Intensity;
                    var offSetX = (float)(random.NextDouble() * 0.02 - 0.01) * intensity;
                    var offSetY = (float)(random.NextDouble() * 0.02 - 0.01) * intensity;
                    var view = Game.Camera.View;
                    view.Translation += new Vector3(offSetX, offSetY, 0);
                    Game.Camera.View = view;
                }

                DrawSky();

                //////////////////////////////////////////////////////////////////////////////////////////////
                // Draw Water
                //////////////////////////////////////////////////////////////////////////////////////////////

                WaterRenderer.DrawWater(worldState);

                //////////////////////////////////////////////////////////////////////////////////
                // Draw the Terrain
                //////////////////////////////////////////////////////////////////////////////////

                BallEffect.View = Game.Camera.View;
                BallEffect.Projection = Game.Camera.Projection;

                var tris = worldState.StaticGeometry.GetTriangles();
                VertexPositionColorTexture[] vpc = new VertexPositionColorTexture[tris.Count * 3];

                int i = 0;

                float TerrainTextureScale = 0.015f;

                var terrainSize = new Vector2(worldState.StaticGeometry.width, worldState.StaticGeometry.height);

                foreach (var t in tris)
                {
                    vpc[i + 0].Color = Color.Maroon;
                    vpc[i + 0].Position = new Vector3(t.A.X, t.A.Y, -1);
                    vpc[i + 0].TextureCoordinate = new Vector2(t.A.X, t.A.Y) / terrainSize;
                    vpc[i + 1].Color = Color.Maroon;
                    vpc[i + 1].Position = new Vector3(t.B.X, t.B.Y, -1);
                    vpc[i + 1].TextureCoordinate = new Vector2(t.B.X, t.B.Y) / terrainSize;
                    vpc[i + 2].Color = Color.Maroon;
                    vpc[i + 2].Position = new Vector3(t.C.X, t.C.Y, -1);
                    vpc[i + 2].TextureCoordinate = new Vector2(t.C.X, t.C.Y) / terrainSize;
                    i += 3;
                }

                Matrix terrainWorld = Matrix.CreateScale(worldState.StaticGeometry.Scale);
                TerrainEffect.CurrentTechnique.Passes[0].Apply();
                
                TerrainEffect.Parameters["ModelViewProjection"].SetValue(terrainWorld * Game.Camera.View * Game.Camera.Projection);
                TerrainEffect.Parameters["TerrainTypesTexture"].SetValue(worldState.StaticGeometry.GetTerrainTypeTexture());
                TerrainEffect.Parameters["EarthTexture"].SetValue(EarthTexture);
                TerrainEffect.Parameters["SandTexture"].SetValue(SandTexture);
                TerrainEffect.Parameters["StoneTexture"].SetValue(StoneTexture);
                TerrainEffect.Parameters["TextureScale"].SetValue(TerrainTextureScale);
                //TerrainEffect.Parameters["TerrainSize"].SetValue(terrainSize);

                GraphicsDevice.DrawUserPrimitives<VertexPositionColorTexture>(PrimitiveType.TriangleList, vpc, 0, tris.Count);

                ///////////////////////////////////////////////////////////////////////////////////
                // Draw Ballz and shots
                ///////////////////////////////////////////////////////////////////////////////////

                var blending = new BlendState
                {
                    AlphaSourceBlend = Blend.SourceAlpha,
                    AlphaDestinationBlend = Blend.InverseSourceAlpha,
                    ColorSourceBlend = Blend.SourceAlpha,
                    ColorDestinationBlend = Blend.InverseSourceAlpha,
                };

                SpriteBatch.Begin(blendState: blending);
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

                foreach(var graphicsEvent in worldState.GraphicsEvents)
                {
                    DrawGraphicsEvent(graphicsEvent);
                }

                SpriteBatch.End();              

                GraphicsDevice.SetRenderTarget(null);
                
                PostProcess();

                DrawStatusOverlay();

                if (Ballz.The().MessageOverlay != null)
                {
                    DrawMessageOverlay(Ballz.The().MessageOverlay);
                }
            }
        }

        public void DrawStatusOverlay()
        {
            if (Game.Match.State == SessionState.Finished)
            {
                string msg = "";

                if (Game.Match.Winner != null)
                    msg = Game.Match.Winner.Name + " won the match!";
                else
                    msg = "Draw!";

                DrawMessageOverlay(msg);
            }
            else if (Game.Match.UsePlayerTurns && Game.Match.ActivePlayer != null)
            {
                var screenPos = new Vector2(Game.GraphicsDevice.Viewport.Width - (250 * resolutionFactor), Game.GraphicsDevice.Viewport.Height - 100);
                string msg;

                if (Game.Match.TurnState == TurnState.Running)
                {
                    var timeLeft = (int)(Game.Match.SecondsPerTurn - Game.Match.TurnTime);
                    msg = "Turn: " + Game.Match.ActivePlayer.Name + " / " + timeLeft;
                }
                else
                {
                    msg = "Waiting for turn end";
                }

                SpriteBatch.Begin();
                DrawText(msg, screenPos, 0.5f, Color.Red, centerHorizontal: true);
                SpriteBatch.End();
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

            Matrix world = Matrix.CreateRotationY((float)(2 * Math.PI * ball.ViewRotation * 50f / 360f)) * Matrix.CreateTranslation(new Vector3(ball.Position, 0));
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
                    SpriteBatch.Draw(weaponTexture, position: weaponPosScreen, color: Color.White, rotation: weaponRotation, scale: new Vector2(weaponTextureScale, weaponTextureScale), origin: new Vector2(weaponTexture.Width / 2f, weaponTexture.Height / 2f), effects: effects);

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
                        SpriteBatch.Draw(WhiteTexture, position: aimIndicatorScreen, scale: new Vector2(100, 20), color: new Color(Color.Black, (int)(64*ball.ShootCharge)), rotation: aimRotation, origin: new Vector2(0, 0.5f));
                        SpriteBatch.Draw(WhiteTexture, position: aimIndicatorScreen, scale: aimIndicatorSize, color: new Color(chargeColor), rotation: aimRotation, origin: new Vector2(0, 0.5f));
                    }
                    // Draw crosshair
                    SpriteBatch.Draw(CrosshairTexture, position: aimTargetScreen, color: Color.White, rotation: aimRotation, origin: new Vector2(16, 16));
                }
            }
            else // Player is dead
            {
                GraveModel.Draw(world, Game.Camera.View, Game.Camera.Projection);
            }
            
            var screenPos = WorldToScreen(ball.Position + new Vector2(0, 2.5f));

            DrawText(ball.Health.ToString("0"), screenPos, 0.5f, Color.White, 1, true, true);
            screenPos += new Vector2(0, 25) * resolutionFactor;
            DrawText(ball.Name, screenPos, 0.5f, Color.LawnGreen, 1, true, true);
            screenPos += new Vector2(0, 25) * resolutionFactor;
            DrawText(ball.Player.Name, screenPos, 0.33f, Color.LawnGreen, 1, true, true);

            if (Game.Match.UsePlayerTurns && Game.Match.TurnState == TurnState.Running && Game.Match.ActivePlayer == ball.Player && ball.Player.ActiveBall == ball)
            {
                // Show turn-indicator for a couple of seconds only
                if (Game.Match.TurnTime < 4)
                {
                    screenPos -= new Vector2(0, resolutionFactor *(30 + (float)(15 * Math.Sin(5 * ElapsedTime.TotalSeconds))));
                    SpriteBatch.Draw(Game.Content.Load<Texture2D>("Textures/RedArrow"), screenPos, color: Color.White, origin: new Vector2(29, 38));
                }
            }
        }

        public void DrawGraphicsEvent(GraphicsEvent graphicsEvent)
        {
            var spriteEffect = graphicsEvent as SpriteGraphicsEffect;
            if(spriteEffect != null)
            {
                var progress = spriteEffect.GetProgress(Game.Match.GameTime);
                var texture = Game.Content.Load<Texture2D>("Textures/"+spriteEffect.SpriteName);
                var pos = WorldToScreen(spriteEffect.Position(Game.Match.GameTime));
                var rotation = spriteEffect.Rotation(Game.Match.GameTime);
                var scale = spriteEffect.Scale(Game.Match.GameTime);
                SpriteBatch.Draw(
                    texture,
                    position: pos,
                    rotation: rotation,
                    scale: new Vector2(scale, scale),
                    origin: new Vector2(texture.Width / 2, texture.Height / 2)
                    );
            }

            var textEffect = graphicsEvent as TextEffect;
            if (textEffect != null)
            {
                var progress = textEffect.GetProgress(Game.Match.GameTime);
                var pos = WorldToScreen(textEffect.Position(Game.Match.GameTime));
                var rotation = textEffect.Rotation(Game.Match.GameTime);
                var scale = textEffect.Scale(Game.Match.GameTime);
                var opacity = textEffect.Opacity(Game.Match.GameTime);
                SpriteBatch.DrawString(
                    Font,
                    textEffect.Text,
                    pos,
                    new Color(textEffect.TextColor, (int)((opacity*255) * textEffect.TextColor.A)),
                    rotation,
                    Vector2.Zero,
                    scale * textEffect.TextSize,
                    SpriteEffects.None,
                    0);
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
            /*BallEffect.DiffuseColor = Vector3.Zero;
            Matrix world = Matrix.CreateScale(0.1f) * Matrix.CreateTranslation(new Vector3(shot.Position, 0));
            BallModel.Draw(world, Game.Camera.View, Game.Camera.Projection);*/

            var effects =  SpriteEffects.None;
            var weaponRotation = shot.Velocity.RotationFromDirection();
            /*
            if (ball.AimDirection.X < 0)
            {
                effects = SpriteEffects.FlipHorizontally;
                weaponRotation += (float)Math.PI;
            }*/

            var weaponPosScreen = WorldToScreen(shot.Position);
            var weaponTexture = Game.Content.Load<Texture2D>("Textures/" + shot.ProjectileTexture);
            var weaponTextureScale = 64f / weaponTexture.Width;

            // Draw weapon
            SpriteBatch.Draw(weaponTexture, position: weaponPosScreen, color: Color.White, 
                rotation: weaponRotation, scale: new Vector2(weaponTextureScale, weaponTextureScale), 
                origin: new Vector2(weaponTexture.Width / 2f, weaponTexture.Height / 2f), effects: effects);

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

            EarthTexture = Game.Content.Load<Texture2D>("Textures/Dirt");
            SandTexture = Game.Content.Load<Texture2D>("Textures/Sand");
            StoneTexture = Game.Content.Load<Texture2D>("Textures/Stone");

            TerrainEffect = Game.Content.Load<Effect>("Effects/Terrain");

            RopeEffect = new BasicEffect(Game.GraphicsDevice);
            RopeEffect.LightingEnabled = false;
            RopeEffect.Texture = Game.Content.Load<Texture2D>("Textures/Rope"); 
            RopeEffect.TextureEnabled = true;
            
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
            
            WaterRenderer.LoadContent();

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