using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ballz.Messages;
using Ballz.GameSession.World;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using static MathFloat.MathF;

namespace Ballz.GameSession.Renderer
{
    public class DebugRenderer : DrawableGameComponent
    {
        new private Ballz Game;
        BasicEffect LineEffect;
        VertexPositionColor[] sphereVertices;
        List<VertexPositionColor[]> terrainVertices = new List<VertexPositionColor[]>();
        private World.World debugWorld;
        private int terrainRevision = -1;

        public DebugRenderer(Ballz _game) : base(_game)
        {
            Game = _game;
        }

        public override void Draw(GameTime gameTime)
        {
            //DrawSphere(Vector2.Zero, new Vector2(0.0f,1.0f));
            debugWorld = Game.World;
            foreach (Entity ball in debugWorld.Entities)
            {
                if (ball.Disposed)
                    continue;

                DrawSphere(ball.Position, ball.Rotation, ball.Radius, ball.PhysicsBody?.Awake ?? false);
            }

            foreach(var rope in debugWorld.Ropes)
            {
                DrawRope(rope);
            }

            drawTerrain();

            base.Draw(gameTime);
        }

        public override void Initialize()
        {
            debugWorld = Game.World;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            LineEffect = new BasicEffect(Game.GraphicsDevice);
            LineEffect.EnableDefaultLighting();
            LineEffect.DiffuseColor = new Vector3(1, 1, 1);
            LineEffect.VertexColorEnabled = true;
            LineEffect.LightingEnabled = false;
            LineEffect.TextureEnabled = false;

            sphereVertices = new VertexPositionColor[18];

            for (int i = 0; i <= 16; i++)
            {
                float angle = (float)Math.PI * 2.0f * (float)i / 16;
                sphereVertices[i].Color = Color.GreenYellow;
                sphereVertices[i].Position = new Vector3(Sin(angle), Cos(angle), 0);
            }

            sphereVertices[17].Color = Color.GreenYellow;
            sphereVertices[17].Position = Vector3.Zero;

            Matrix terrainWorld = Matrix.CreateScale(0.03f);

            base.LoadContent();
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();
        }

        private void updateTerrain()
        {
            if(terrainRevision != Game.World.StaticGeometry.Revision)
            {
                List<List<Vector2>> outline = Game.World.StaticGeometry.getOutline();
                terrainVertices.Clear();
                foreach (List<Vector2> lineStrip in outline)
                {
                    VertexPositionColor[] lineVertices = new VertexPositionColor[lineStrip.Count];
                    for (int i = 0; i < lineStrip.Count; i++)
                    {
                        lineVertices[i].Color = Color.GreenYellow;
                        lineVertices[i].Position = new Vector3(lineStrip[i],0) * Game.World.StaticGeometry.Scale;
                    }
                    terrainVertices.Add(lineVertices);
                }

                terrainRevision = Game.World.StaticGeometry.Revision;
            }
        }

        public override void Update(GameTime gameTime)
        {
            debugWorld = Game.World;
            updateTerrain();
            base.Update(gameTime);
        }

        public void HandleMessage(object sender, Messages.Message msg)
        {
            if (msg.Kind == Message.MessageType.InputMessage)
            {
                InputMessage ipmsg = (InputMessage)msg;
                if (ipmsg.Kind == InputMessage.MessageType.ControlsConsole && ipmsg.Pressed.HasValue && ipmsg.Pressed.Value)
                {
                    Enabled = !Enabled;
                    Visible = !Visible;
                    debugWorld = Game.World;
                }
            }
        }

        protected override void OnEnabledChanged(object sender, EventArgs args)
        {
            base.OnEnabledChanged(sender, args);
        }
            
        public void DrawSphere(Vector2 position, float direction, float radius, bool awake)
        {
            LineEffect.Projection = Game.Camera.Projection;
            LineEffect.View = Game.Camera.View;
            LineEffect.World = Matrix.CreateScale(radius);
            LineEffect.World *= Matrix.CreateRotationZ(direction);
            LineEffect.World *= Matrix.CreateTranslation((new Vector3(position, 0)));

            if(awake)
                LineEffect.DiffuseColor = new Vector3(1, 0, 0);

            LineEffect.CurrentTechnique.Passes[0].Apply();
            GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, sphereVertices, 0, sphereVertices.Length - 1);
        }

        public void DrawRope(Rope rope)
        {
            foreach(var segment in rope.PhysicsSegments)
            {
                DrawSphere(segment.Position, 0, Rope.SegmentLength * 0.5f, true);
            }
        }

        public void drawTerrain()
        {
            LineEffect.Projection = Game.Camera.Projection;
            LineEffect.View = Game.Camera.View;
            LineEffect.World = Matrix.Identity;
            LineEffect.DiffuseColor = Color.GreenYellow.ToVector3();

            LineEffect.CurrentTechnique.Passes[0].Apply();
            foreach (VertexPositionColor[] lineVertices in terrainVertices)
            {
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, lineVertices, 0, lineVertices.Length - 1);
            }
        }
    }
}
