using Ballz.Messages;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;

namespace Ballz.GameSession.Renderer
{
    /// <summary>
    ///     Render system performs all rendering of the Game and is inteded as a module.
    /// </summary>
    public partial class GameRenderer : DrawableGameComponent
    {
        Model BallModel;
        Texture2D GermoneyTexture;
        Texture2D TerrainTexture;
        BasicEffect BallEffect;
        BasicEffect TerrainEffect;
        SpriteBatch spriteBatch;

        new Ballz Game;

        Matrix ProjectionMatrix;
        Matrix ViewMatrix;
        TimeSpan lastModification;

        public GameRenderer(Ballz game) : base(game)
        {
            Game = game;
        }

        /// <summary>
        ///     Draw the game for the specified _time.
        /// </summary>
        /// <param name="time">time since start of game (cf BallzGame draw).</param>
        public override void Draw(GameTime time)
        {
            if (lastModification == null)
                lastModification = time.TotalGameTime;
            ProjectionMatrix = Matrix.Identity;

            ViewMatrix = Matrix.CreateOrthographicOffCenter(0, 10 * Game.GraphicsDevice.Viewport.AspectRatio, 0, 10, -10, 10);

            BallEffect.View = ViewMatrix;
            BallEffect.Projection = ProjectionMatrix;

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

            Matrix terrainWorld = Matrix.CreateScale(0.03f);
            TerrainEffect.World = terrainWorld;
            TerrainEffect.View = ViewMatrix;
            TerrainEffect.Projection = ProjectionMatrix;
            TerrainEffect.CurrentTechnique.Passes[0].Apply();
            GraphicsDevice.SamplerStates[0] = new SamplerState
            {
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap
            };

            GraphicsDevice.DrawUserPrimitives<VertexPositionColorTexture>(PrimitiveType.TriangleList, vpc, 0, tris.Count);

            foreach (var entity in worldState.Entities)
            {
                Vector2 nV = Vector2.Normalize(entity.Velocity);
                Matrix world = Matrix.CreateRotationY((float)(2 * Math.PI * 50 * nV.X / 360.0)) * Matrix.CreateTranslation(new Vector3(entity.Position, 0));
                BallEffect.World = world;
                BallModel.Draw(world, ViewMatrix, ProjectionMatrix);
                //DrawSphere(entity.Position, new Vector2(1, 0));
            }
        }

        protected override void LoadContent()
        {
            GermoneyTexture = Game.Content.Load<Texture2D>("Textures/Germoney");

            BallEffect = new BasicEffect(Game.GraphicsDevice);
            BallEffect.EnableDefaultLighting();
            BallEffect.Texture = GermoneyTexture;
            BallEffect.TextureEnabled = true;
            BallEffect.DirectionalLight0.Direction = new Vector3(1, -1, -1);
            BallEffect.AmbientLightColor = new Vector3(0.3f, 0.3f, 0.3f);
            BallEffect.PreferPerPixelLighting = true;

            BallModel = Game.Content.Load<Model>("Ball");
            BallModel.Meshes[0].MeshParts[0].Effect = BallEffect;

            TerrainTexture = Game.Content.Load<Texture2D>("Textures/Dirt");

            TerrainEffect = new BasicEffect(Game.GraphicsDevice);
            TerrainEffect.LightingEnabled = false;
            TerrainEffect.Texture = TerrainTexture;
            TerrainEffect.TextureEnabled = true;

            spriteBatch = new SpriteBatch(Game.GraphicsDevice);

            PrepareDebugRendering();

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