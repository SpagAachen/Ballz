using Ballz.Messages;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Ballz.GameSession.Renderer
{
    /// <summary>
    ///     Render system performs all rendering of the Game and is inteded as a module.
    /// </summary>
    public class GameRenderer : DrawableGameComponent
    {
        Model BallModel;
        BasicEffect BallEffect;

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
                    new Vector3(0, 3, 3),
                    Vector3.Zero,
                    Vector3.UnitY
                );
            
            BallEffect.View = view;
            BallEffect.Projection = projection;

            var snapshot = Game.World.GetSnapshot(time);

            BallEffect.DiffuseColor = new Vector3(1, 1, 1);
            foreach (var p in snapshot.StaticGeometry.outline)
            {
                Matrix world = Matrix.CreateTranslation(new Vector3(p*0.01f, 0));
                BallEffect.World = world;
                BallModel.Draw(world, view, projection);
            }

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
            BallModel = Game.Content.Load<Model>("Ball");
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