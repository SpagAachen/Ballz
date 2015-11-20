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
        BasicEffect BallEffect;
        SpriteBatch spriteBatch;
        
        Ballz Game;

        Matrix ProjectionMatrix;
        Matrix ViewMatrix;

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
            ProjectionMatrix = Matrix.Identity;
            ViewMatrix = Matrix.CreateOrthographicOffCenter(0, 10 * Game.GraphicsDevice.DisplayMode.AspectRatio, 0, 10, -10, 10);

            BallEffect.View = ViewMatrix;
            BallEffect.Projection = ProjectionMatrix;

            var snapshot = Game.World.GetSnapshot(time);

            // Debug
            snapshot.StaticGeometry.SubtractCircle((float)(new Random()).NextDouble() * ((int)time.TotalGameTime.TotalMilliseconds * 1321 % 640), (float)(new Random()).NextDouble() * ((int)time.TotalGameTime.TotalMilliseconds * 1701 % 480), (float)(new Random()).NextDouble() * 25.0f);
            snapshot.StaticGeometry.AddCircle((float)(new Random()).NextDouble() * ((int)time.TotalGameTime.TotalMilliseconds * 1711 % 640), (float)(new Random()).NextDouble() * ((int)time.TotalGameTime.TotalMilliseconds * 14307 % 480), (float)(new Random()).NextDouble() * 15.0f);

            var tris = snapshot.StaticGeometry.getTriangles();
            VertexPositionColor[] vpc = new VertexPositionColor[tris.Count * 3];
           
            int i = 0;
            foreach (var t in tris)
            {
                vpc[i+0].Color = Color.Maroon;
                vpc[i+0].Position = new Vector3(t.a.X, t.a.Y, -1);
                vpc[i+1].Color = Color.Maroon;
                vpc[i+1].Position = new Vector3(t.b.X, t.b.Y, -1);
                vpc[i+2].Color = Color.Maroon;
                vpc[i+2].Position = new Vector3(t.c.X, t.c.Y, -1);
                i+=3;
            }

            Matrix terrainWorld = Matrix.CreateScale(0.03f);
            LineEffect.World = terrainWorld;
            LineEffect.View = ViewMatrix;
            LineEffect.Projection = ProjectionMatrix;
            LineEffect.CurrentTechnique.Passes[0].Apply();

            GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, vpc, 0, tris.Count);

            foreach (var entity in snapshot.Entities)
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