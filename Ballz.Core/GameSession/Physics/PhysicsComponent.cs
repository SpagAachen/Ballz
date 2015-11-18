using Ballz.GameSession.World;
using Ballz.Messages;
using Microsoft.Xna.Framework;

using System;
using Physics2DDotNet;
using Physics2DDotNet.Shapes;

namespace Ballz.GameSession.Physics
{
    /// <summary>
    ///     Physics control is called by BallzGame update to simulate discrete GamePhysics.
    /// </summary>
    public class PhysicsControl : GameComponent
    {
        new Ballz Game;
        PhysicsEngine engine;
        public PhysicsControl(Ballz game) : base(game)
        {
            Game = game;
        }

        public override void Initialize()
        {
            // Create engine
            engine = new PhysicsEngine();
            engine.BroadPhase = new Physics2DDotNet.Detectors.SelectiveSweepDetector();
            engine.Solver = new Physics2DDotNet.Solvers.SequentialImpulsesSolver();
        }

        public override void Update(GameTime time)
        {
            float intervalSeconds = (float)World.World.IntervalMs / 1000.0f;

            var headSnapshot = Game.World.GetHeadSnapshot();
            var headTime = Game.World.HeadTime;
            //var elapsedSeconds = (float)time.ElapsedGameTime.TotalSeconds;
            //TODO extract into function

            // Terrain
            var terrain = headSnapshot.StaticGeometry.getTriangles();

            // TODO: Use triangles now! Outline is deprecated
            /*AdvanceMath.Vector2D[] terrainVert = new AdvanceMath.Vector2D[terrain.Count];
            for (int i = 0; i < terrain.Count; i++)
            {
                terrainVert[i] = new AdvanceMath.Vector2D(terrain[i].X, terrain[i].Y);
            }
            */
            //terrainVert[terrain.Count] = new AdvanceMath.Vector2D(terrain[terrain.Count - 1].X, terrain[terrain.Count - 1].Y - 10);

            var terrainShape = new PolygonShape(VertexHelper.CreateRectangle(10, 20), 3);
            //var terrainShape = new PolygonShape(terrainVert, 0.1f);
            /*var terrainCoeff = new Coefficients(1, .5f);
            var terrainBody = new Body(new PhysicsState(), terrainShape, .0f, terrainCoeff, new Lifespan());
            engine.AddBody(terrainBody);
            
            // Entities
            var entityPhysMap = new System.Collections.Generic.Dictionary<Entity, Body>();
            foreach (var e in headSnapshot.Entities)
            {
                IShape shape = null;
                float mass = .0f;
                switch (e.Material.Shape)
                {
                    case PhysicsMaterial.PhysicsShape.Circle:
                        shape = new CircleShape(e.Material.Radius, 16);
                        mass = (float) (e.Material.Density * e.Material.Radius * e.Material.Radius * Math.PI);
                        break;
                    case PhysicsMaterial.PhysicsShape.Polygon:
                        //TODO
                        break;
                    default:
                        //TODO
                        break;                        
                }

                Coefficients coeff = new Coefficients(e.Material.Restitution, e.Material.Friction);
                Body body = new Body(new PhysicsState(), shape, mass, coeff, new Lifespan());
                entityPhysMap.Add(e, body);
                //IShape shape = new CircleShape(e.r)
            }
            */
            for (var remainingSeconds = time.TotalGameTime.TotalSeconds - headTime.TotalSeconds;
                remainingSeconds > 0;
                remainingSeconds -= intervalSeconds)
            {
                //TODO: Check timer
                //engine.Update(intervalSeconds, intervalSeconds);
                //PhysicsTimer timer = new PhysicsTimer(engine.Update, intervalSeconds);

                headSnapshot = (WorldSnapshot)headSnapshot.Clone();

                foreach (var e in headSnapshot.Entities)
                {
                    e.Position = e.Position + e.Velocity * intervalSeconds;
                    e.Velocity += new Vector2(0, -10) * intervalSeconds;

                    if (e.Position.Y < 0.5)
                    {
                        e.Velocity *= new Vector2(1, -0.95f);
                        e.Position = new Vector2(e.Position.X, 0.5f);
                    }
                    if (e.Position.X < 0.5)
                    {
                        e.Velocity *= new Vector2(-0.95f, 1);
                        e.Position = new Vector2(0.5f, e.Position.Y);
                    }
                    if (e.Position.X > 9.5)
                    {
                        e.Velocity *= new Vector2(-0.95f, 1);
                        e.Position = new Vector2(9.5f, e.Position.Y);
                    }
                }

                Game.World.AddDiscreteSnapshot(headSnapshot);
            }
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
            }
        }
    }
}