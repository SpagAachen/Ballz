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
            //snapshot.StaticGeometry.subtractCircle(1.0f * ((int)time.TotalGameTime.TotalMilliseconds * 1321 % 640), 1.0f * ((int)time.TotalGameTime.TotalMilliseconds * 1701 % 480), (float)(new Random()).NextDouble() * 10.0f);


            var outline = snapshot.StaticGeometry.getOutline();
            VertexPositionColor[] vpc = new VertexPositionColor[outline.Count + 1];
            BallEffect.DiffuseColor = new Vector3(1, 1, 1);

            Vector2 last = outline[outline.Count - 1];

            int i = 0;
            foreach (var p in outline)
            {
                vpc[i].Color = Color.PapayaWhip;
                vpc[i].Position = new Vector3(p.X, p.Y, -1);
                ++i;
            }
            vpc[i].Color = vpc[0].Color;
            vpc[i].Position = vpc[0].Position;

            Matrix terrainWorld = Matrix.CreateScale(0.03f);
            LineEffect.World = terrainWorld;
            LineEffect.CurrentTechnique.Passes[0].Apply();

            GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineStrip, vpc, 0, outline.Count);



            BallEffect.DiffuseColor = new Vector3(1, 0, 0);
            foreach (var entity in snapshot.Entities)
            {
                Matrix world = Matrix.CreateTranslation(new Vector3(entity.Position, 0));
                BallEffect.World = world;
                BallModel.Draw(world, ViewMatrix, ProjectionMatrix);
                DrawSphere(entity.Position, new Vector2(1, 0));
            }
        }

        protected override void LoadContent()
        {
            BallEffect = new BasicEffect(Game.GraphicsDevice);
            BallEffect.EnableDefaultLighting();
            BallEffect.DiffuseColor = new Vector3(1, 0, 0);
            BallEffect.DirectionalLight0.Direction = new Vector3(1, -1, 0);

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