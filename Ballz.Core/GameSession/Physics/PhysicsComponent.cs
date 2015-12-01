using Ballz.GameSession.World;
using Ballz.Messages;
using Ballz.Utils;
using Microsoft.Xna.Framework;

using System;
using System.Linq;
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

        public Dictionary<Entity, Body> PreparePhysicsEngine(World.World worldState)
        {
            //TODO remove later
            engine = new PhysicsEngine();
            engine.BroadPhase = new Physics2DDotNet.Detectors.SelectiveSweepDetector();
            engine.Solver = new Physics2DDotNet.Solvers.SequentialImpulsesSolver();
            PhysicsLogic logGravity = (PhysicsLogic)new GravityField(new AdvanceMath.Vector2D(0f, -1f), new Lifespan());
            engine.AddLogic(logGravity);

            //TODO extract into function

            // Terrain
            var terrains = worldState.StaticGeometry.getOutline();
            
           for (int i = 0; i < terrains.Count; i++)
            {
                var terrain = terrains[i];
                var terrainPhys = new AdvanceMath.Vector2D[terrain.Count];
                for (int j = 0; j < terrain.Count; j++)
                {
                    var v = terrain[j];
                    terrainPhys[j] = new AdvanceMath.Vector2D(v.X * 0.03f, v.Y * 0.03f);
                }
                var terrainShape = new PolygonShape(terrainPhys, 3f);
                var terrainCoeff = new Coefficients(1, .5f);
                var terrainBody = new Body(new PhysicsState(), terrainShape, float.PositiveInfinity, terrainCoeff, new Lifespan());
                engine.AddBody(terrainBody);
            }

            // Entities
            var entityPhysMap = new System.Collections.Generic.Dictionary<Entity, Body>();
            foreach (var e in worldState.Entities)
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
            var worldState = Game.World;

            var player = worldState.EntityById(Game.Match.PlayerBallId);
            // Apply input messages
            if (player != null)
            {
                if (KeyPressed[InputMessage.MessageType.ControlsLeft])
                    player.Velocity = new Vector2(-2f, player.Velocity.Y);
                if (KeyPressed[InputMessage.MessageType.ControlsRight])
                    player.Velocity = new Vector2(2f, player.Velocity.Y);

                switch (controlInput)
                {
                    case InputMessage.MessageType.ControlsUp:
                        player.Velocity = new Vector2(player.Velocity.X, 2f);
                        break;
                    case InputMessage.MessageType.ControlsAction:
                        worldState.Shots.Add(new Shot
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

            var entityPhysMap = PreparePhysicsEngine(worldState);

            float intervalSeconds = (float)time.ElapsedGameTime.TotalSeconds;


            engine.Update(intervalSeconds, intervalSeconds);

            foreach (var e in worldState.Entities)
            {
                if (!entityPhysMap.ContainsKey(e))
                {
                    continue;
                }
                var physE = entityPhysMap[e];
                var state = physE.State;

                e.Position = state.Position.ToXna();
                e.Velocity = state.Velocity.ToXna();

                const float dg90 = 2 * (float)Math.PI * 90f / 360f;
                if (e.Velocity.LengthSquared() > 0.0001f)
                    e.Rotation = dg90 * (e.Velocity.X > 0 ? 1 : -1);
                else
                    e.Rotation = dg90;
            }

            // Update shots
            foreach (var shot in worldState.Shots)
            {
                //TODO: Compute actual shot target position
                Vector2 targetPos = shot.ShotStart + 3 * shot.ShotVelocity;
                worldState.StaticGeometry.SubtractCircle(targetPos.X / 0.03f, targetPos.Y / 0.03f, shot.ExplosionRadius / 0.03f);
            }
            // Remove all shots
            worldState.Shots.Clear();
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