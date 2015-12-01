using Ballz.GameSession.World;
using Ballz.Messages;
using Microsoft.Xna.Framework;

using System;
using Physics2DDotNet;
using Physics2DDotNet.Shapes;
using Physics2DDotNet.PhysicsLogics;
using System.Collections.Generic;

namespace Ballz.GameSession.Physics
{
    /// <summary>
    ///     Physics control is called by BallzGame update to simulate discrete GamePhysics.
    /// </summary>
    public class PhysicsControl : GameComponent
    {
        new Ballz Game;
        PhysicsEngine engine;
        InputMessage.MessageType? controlInput = null;
        public PhysicsControl(Ballz game) : base(game)
        {
            Game = game;
        }

        Dictionary<InputMessage.MessageType, bool> KeyPressed = new Dictionary<InputMessage.MessageType, bool>();

        public override void Initialize()
        {
            // Create engine

            engine = new PhysicsEngine();
            engine.BroadPhase = new Physics2DDotNet.Detectors.SelectiveSweepDetector();
            engine.Solver = new Physics2DDotNet.Solvers.SequentialImpulsesSolver();

            KeyPressed[InputMessage.MessageType.ControlsAction] = false;
            KeyPressed[InputMessage.MessageType.ControlsUp] = false;
            KeyPressed[InputMessage.MessageType.ControlsDown] = false;
            KeyPressed[InputMessage.MessageType.ControlsLeft] = false;
            KeyPressed[InputMessage.MessageType.ControlsRight] = false;
        }

        public Dictionary<Entity, Body> PreparePhysicsEngine(WorldSnapshot newSnapshot)
        {
            //TODO remove later
            engine = new PhysicsEngine();
            engine.BroadPhase = new Physics2DDotNet.Detectors.SelectiveSweepDetector();
            engine.Solver = new Physics2DDotNet.Solvers.SequentialImpulsesSolver();
            PhysicsLogic logGravity = (PhysicsLogic)new GravityField(new AdvanceMath.Vector2D(0f, -1f), new Lifespan());
            engine.AddLogic(logGravity);
            
            var headTime = Game.World.HeadTime;
            //var elapsedSeconds = (float)time.ElapsedGameTime.TotalSeconds;
            //TODO extract into function

            // Terrain
            var terrain = newSnapshot.StaticGeometry.getOutlineTriangles();

            //TODO: Testshape
            IShape shapeTest = new PolygonShape(VertexHelper.CreateRectangle(200, 1), 0.3f);
            var coeffTest = new Coefficients(.0f, 10f);
            var stateTest = new PhysicsState();
            stateTest.Position = new ALVector2D(.0f, 10, 0);
            var bodyTest = new Body(stateTest, shapeTest, float.PositiveInfinity, coeffTest, new Lifespan());
            bodyTest.IgnoresGravity = true;

            engine.AddBody(bodyTest);

            // TODO: Use triangles now! Outline is deprecated

            for (int i = 0; i < terrain.Count && i < 0; i++)
            {
                //terrainVert[i] = new AdvanceMath.Vector2D(terrain[i].X, terrain[i].Y);
                var tri = terrain[i];
                AdvanceMath.Vector2D[] terrainPhys = new AdvanceMath.Vector2D[3];
                terrainPhys[0] = new AdvanceMath.Vector2D(tri.a.X, tri.a.Y);
                terrainPhys[1] = new AdvanceMath.Vector2D(tri.b.X, tri.b.Y);
                terrainPhys[2] = new AdvanceMath.Vector2D(tri.c.X, tri.c.Y);

                var terrainShape = new PolygonShape(terrainPhys, 0.3f);
                var terrainCoeff = new Coefficients(1, .5f);
                var terrainBody = new Body(new PhysicsState(), terrainShape, .0f, terrainCoeff, new Lifespan());
                engine.AddBody(terrainBody);
            }

            // Entities
            var entityPhysMap = new System.Collections.Generic.Dictionary<Entity, Body>();
            foreach (var e in newSnapshot.Entities)
            {
                IShape shape = null;
                float mass = .0f;
                switch (e.Material.Shape)
                {
                    case PhysicsMaterial.PhysicsShape.Circle:
                        shape = new CircleShape(e.Material.Radius, 16);
                        mass = (float)(e.Material.Density * e.Material.Radius * e.Material.Radius * Math.PI);
                        break;
                    case PhysicsMaterial.PhysicsShape.Polygon:
                        //TODO
                        continue;
                    //break;
                    default:
                        //TODO
                        continue;
                        //break;                        
                }

                var coeff = new Coefficients(e.Material.Restitution, e.Material.Friction);
                var state = new PhysicsState();
                state.Position = new ALVector2D(.0f, e.Position.X, e.Position.Y);
                state.Velocity = new ALVector2D(.0f, e.Velocity.X, e.Velocity.Y);

                Body body = new Body(state, shape, mass, coeff, new Lifespan());
                engine.AddBody(body);
                entityPhysMap.Add(e, body);
                //IShape shape = new CircleShape(e.r)
            }

            return entityPhysMap;
        }

        public override void Update(GameTime time)
        {
            var headSnapshot = Game.World.GetHeadSnapshot();
            var newSnapshot = (WorldSnapshot)headSnapshot.Clone();

            var player = newSnapshot.EntityById(Game.Match.PlayerBallId);
            // Apply input messages
            if(player != null)
            {
                if(KeyPressed[InputMessage.MessageType.ControlsLeft])
                        player.Velocity = new Vector2(-2f, player.Velocity.Y);
                if (KeyPressed[InputMessage.MessageType.ControlsRight])
                    player.Velocity = new Vector2(2f, player.Velocity.Y);

                switch (controlInput)
                {
                    case InputMessage.MessageType.ControlsUp:
                        player.Velocity = new Vector2(player.Velocity.X, 2f);
                        break;
                    case InputMessage.MessageType.ControlsAction:
                        newSnapshot.Shots.Add(new Shot
                        {
                            ExplosionRadius = 0.5f,
                            HealthImpactAtDirectHit = 25,
                            IsInstantShot = true,
                            ShotStart = player.Position,
                            ShotVelocity = player.Direction
                        });
                        break;
                    default:
                        break;
                }
            }

            controlInput = null;

            var entityPhysMap = PreparePhysicsEngine(newSnapshot);

            float intervalSeconds = (float)World.World.IntervalMs / 1000.0f;

            // Update Entity positions
            for (var remainingSeconds = time.ElapsedGameTime.TotalSeconds;
                remainingSeconds > 0;
                remainingSeconds -= intervalSeconds)
            {

                engine.Update(intervalSeconds, intervalSeconds);

                foreach (var e in newSnapshot.Entities)
                {
                    if (!entityPhysMap.ContainsKey(e))
                    {
                        continue;
                    }
                    var physE = entityPhysMap[e];
                    var state = physE.State;
                    
                    e.Position = new Vector2(state.Position.X, state.Position.Y);
                    e.Velocity = new Vector2(state.Velocity.X, state.Velocity.Y);

                    const float dg90 = 2 * (float)Math.PI * 90f / 360f;
                    if (e.Velocity.LengthSquared() > 0.0001f)
                        e.Rotation = dg90 * (e.Velocity.X > 0 ? 1 : -1);
                    else
                        e.Rotation = dg90;
                }

                Game.World.AddDiscreteSnapshot(newSnapshot);
            }

            // Update shots
            foreach(var shot in newSnapshot.Shots)
            {
                //TODO: Compute actual shot target position
                Vector2 targetPos = shot.ShotStart + 3 * shot.ShotVelocity;
                newSnapshot.StaticGeometry.SubtractCircle(targetPos.X / 0.03f, targetPos.Y / 0.03f, shot.ExplosionRadius / 0.03f);
            }
            // Remove all shots
            newSnapshot.Shots.Clear();
        }

        private void processInput(InputMessage message)
        {
            if (message.Pressed.HasValue)
            {
                KeyPressed[message.Kind] = message.Pressed.Value;

                if (message.Pressed.Value)
                    controlInput = message.Kind;
            }
        }

        public void HandleMessage(object sender, Message message)
        {
            if (message.Kind == Message.MessageType.InputMessage)
            {
                InputMessage msg = (InputMessage)message;

                processInput(msg);
            }

            if (message.Kind == Message.MessageType.LogicMessage)
            {
                LogicMessage msg = (LogicMessage)message;

                if (msg.Kind == LogicMessage.MessageType.GameMessage)
                {
                    Enabled = !Enabled;
                }
            }

        }
    }
}