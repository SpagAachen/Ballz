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
    public class GameRenderer : DrawableGameComponent
    {
        Model BallModel;
        BasicEffect BallEffect;
		SpriteBatch spriteBatch;

        Ballz Game;

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
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(
                    1.0472f,
                    Game.GraphicsDevice.DisplayMode.AspectRatio,
                    0.1f,
                    1000.0f
                );

            Matrix view = Matrix.CreateLookAt(
                    new Vector3(0, 0, 10),
                    new Vector3(5, 5, 0),
                    Vector3.UnitY
                );
            
            BallEffect.View = view;
            BallEffect.Projection = projection;

            var snapshot = Game.World.GetSnapshot(time);

			/*
            BallEffect.DiffuseColor = new Vector3(1, 1, 1);
            foreach (var p in snapshot.StaticGeometry.outline)
            {
                Matrix world = Matrix.CreateScale(0.03f) * Matrix.CreateTranslation(new Vector3(p*0.03f, 0));
                BallEffect.World = world;
                BallModel.Draw(world, view, projection);
            }
			*/

			VertexPositionColor[] vpc = new VertexPositionColor[snapshot.StaticGeometry.outline.Count+1];
			BallEffect.DiffuseColor = new Vector3(1, 1, 1);

			Vector2 last = snapshot.StaticGeometry.outline [snapshot.StaticGeometry.outline.Count - 1];

			int i = 0;
			foreach (var p in snapshot.StaticGeometry.outline)
			{

					vpc[i].Color = Color.PapayaWhip;
					vpc[i].Position = new Vector3(p.X, p.Y, -1);

					++i;
			}
				vpc[i].Color = vpc[0].Color;
				vpc[i].Position = vpc[0].Position;

			Matrix terrainWorld = Matrix.CreateScale (0.05f);
			BallEffect.World = terrainWorld;
			BallEffect.VertexColorEnabled = true;
			BallEffect.CurrentTechnique.Passes [0].Apply();
			//BallModel.Draw(world, view, projection);
			GraphicsDevice.DrawUserPrimitives<VertexPositionColor> (PrimitiveType.LineStrip, vpc, 0, snapshot.StaticGeometry.outline.Count);


            BallEffect.DiffuseColor = new Vector3(1, 0, 0);
            foreach (var entity in snapshot.Entities)
            {
                Matrix world = Matrix.CreateTranslation(new Vector3(entity.Position, 0));
                BallEffect.World = world;
                BallModel.Draw(world, view, projection);
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

			spriteBatch = new SpriteBatch (Game.GraphicsDevice);

            base.LoadContent();
        }

        public void HandleMessage(object sender, Message message)
        {
            //throw new NotImplementedException ();
            if (message.Kind != Message.MessageType.LogicMessage)
                return;

            //todo check content of logicmessage as soon as it is implemented
            Enabled = !Enabled;
            Visible = !Visible;
        }
    }
}