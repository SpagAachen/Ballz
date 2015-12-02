using Ballz.GameSession.World;
using Ballz.Messages;
using Ballz.Utils;
using Microsoft.Xna.Framework;
using FarseerPhysics;
using FarseerPhysics.Dynamics;


using System;
using System.Collections.Generic;
using System.Linq;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;

namespace Ballz.GameSession.Physics
{
    /// <summary>
    ///     Physics control is called by BallzGame update to simulate discrete GamePhysics.
    /// </summary>
    public class PhysicsControl : GameComponent
    {
        new Ballz Game;
        FarseerPhysics.Dynamics.World PhysicsWorld;
        InputMessage.MessageType? controlInput = null;
        bool shapesInitialized = false;
        public PhysicsControl(Ballz game) : base(game)
        {
            Game = game;
        }

        Dictionary<InputMessage.MessageType, bool> KeyPressed = new Dictionary<InputMessage.MessageType, bool>();
        
        Dictionary<int, Body> PhysicsBodiesByEntityId = new Dictionary<int, Body>();

        Body TerrainBody;
        int TerrainRevision = -1;

        public override void Initialize()
        {
            PhysicsWorld = new FarseerPhysics.Dynamics.World(new Vector2(0f, -9.82f));

            // Add a ground plate for physics testing
            var ground = new Body(PhysicsWorld);
            ground.BodyType = BodyType.Static;
            ground.CreateFixture(new EdgeShape(new Vector2(-10, 0), new Vector2(20, 0)));

            KeyPressed[InputMessage.MessageType.ControlsAction] = false;
            KeyPressed[InputMessage.MessageType.ControlsUp] = false;
            KeyPressed[InputMessage.MessageType.ControlsDown] = false;
            KeyPressed[InputMessage.MessageType.ControlsLeft] = false;
            KeyPressed[InputMessage.MessageType.ControlsRight] = false;
        }

        public void UpdateTerrainBody(Terrain terrain)
        {
            if (TerrainBody != null)
                PhysicsWorld.RemoveBody(TerrainBody);

            var outlines = terrain.getOutline();

            TerrainBody = new Body(PhysicsWorld);
            TerrainBody.BodyType = BodyType.Static;

            foreach (var outline in outlines)
            {
                var vertices = new Vector2[outline.Count];

                for(int i = 0; i < outline.Count; i++)
                {
                    vertices[i] = outline[i] * 0.03f;
                }

                var shape = new ChainShape(new Vertices(vertices));
                TerrainBody.CreateFixture(shape);
            }
        }

        public void PreparePhysicsEngine(World.World worldState)
        {
            if (worldState.StaticGeometry.Revision != TerrainRevision)
                UpdateTerrainBody(worldState.StaticGeometry);

            TerrainRevision = worldState.StaticGeometry.Revision;

            //TODO: Remove bodies from deleted entities from the PhysicsBodiesByEntityId map

            // Add Bodies for new entities
            foreach (var e in worldState.Entities)
            {
                Body body = null;
                if (!PhysicsBodiesByEntityId.ContainsKey(e.ID))
                {
                    body = new Body(PhysicsWorld);
                    body.BodyType = BodyType.Dynamic;
                    body.CreateFixture(new CircleShape(1.0f, 1.0f));
                    PhysicsBodiesByEntityId[e.ID] = body;
                }
                else
                    body = PhysicsBodiesByEntityId[e.ID];

                body.SetTransform(e.Position, e.Rotation);
                body.LinearVelocity = e.Velocity;

                // TODO: Allow rotation of bodies
                body.AngularVelocity = 0;
            }
        }

        public void PhysicsStep(World.World worldState, float elapsedSeconds)
        {
            // Update the physics world
            PhysicsWorld.Step(elapsedSeconds);
            
            // Sync back the positions and velocities
            foreach (var e in worldState.Entities)
            {
                var body = PhysicsBodiesByEntityId[e.ID];
                
                e.Position = body.Position;
                e.Velocity = body.LinearVelocity;

                const float dg90 = 2 * (float)Math.PI * 90f / 360f;
                if (e.Velocity.LengthSquared() > 0.0001f)
                    e.Rotation = dg90 * (e.Velocity.X > 0 ? 1 : -1);
                else
                    e.Rotation = dg90;
            }
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
                        player.Velocity = new Vector2(player.Velocity.X, 5f);
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

            PreparePhysicsEngine(worldState);

            float intervalSeconds = (float)time.ElapsedGameTime.TotalSeconds;

            PhysicsStep(worldState, intervalSeconds);

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