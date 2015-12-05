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
using System.Diagnostics;

namespace Ballz.GameSession.Physics
{
    /// <summary>
    ///     Physics control is called by BallzGame update to simulate discrete GamePhysics.
    /// </summary>
    public class PhysicsControl : GameComponent
    {
        new Ballz Game;
        FarseerPhysics.Dynamics.World PhysicsWorld;

        public PhysicsControl(Ballz game) : base(game)
        {
            Game = game;
        }

       
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
        }
        
        public void UpdateTerrainBody(Terrain terrain)
        {
            if (TerrainBody != null)
                PhysicsWorld.RemoveBody(TerrainBody);

            var outlines = terrain.getOutline();

            TerrainBody = new Body(PhysicsWorld);
            TerrainBody.BodyType = BodyType.Static;
            TerrainBody.Friction = 1.0f;

            foreach (var outline in outlines)
            {
                var vertices = new Vector2[outline.Count];

                for(int i = 0; i < outline.Count; i++)
                {
                    vertices[i] = outline[i] * terrain.Scale;
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
                    body.Friction = 5.0f;
                    body.Restitution = 0.1f;
                    body.Mass = 10f;
                    body.FixedRotation = true;
                }
                else
                    body = PhysicsBodiesByEntityId[e.ID];

                body.SetTransform(e.Position, e.Rotation);
                body.LinearVelocity = e.Velocity;
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
        {using (new PerformanceReporter(Game))
            {
            if (Game.Match.State != Logic.SessionState.Running)
                return;

            var worldState = Game.World;
            float elapsedSeconds = (float)time.ElapsedGameTime.TotalSeconds;
            
            PreparePhysicsEngine(worldState);
            PhysicsStep(worldState, elapsedSeconds);

            // Update shots
            foreach (var shot in worldState.Shots)
            {
                Vector2 targetPos = Vector2.Zero;
                Fixture targetFixture = null;
                Func<Fixture, Vector2, Vector2, float, float> raycastCallback =
                    (Fixture fixture, Vector2 position, Vector2 normal, float fraction) =>
                    {
                        targetFixture = fixture;
                        targetPos = position;
                        return fraction;
                    };
                
                PhysicsWorld.RayCast(raycastCallback, shot.ShotStart, shot.ShotStart + 100 * shot.ShotVelocity);

                // Did the shot hit anything?
                if(targetFixture != null)
                {
                    // Terrain hit? Then add an explosion there.
                    if(targetFixture.Body == TerrainBody)
                    {
                        worldState.StaticGeometry.SubtractCircle(targetPos.X, targetPos.Y, shot.ExplosionRadius);
                    }
                    // Otherwise, find the entity that belongs to the hit body
                    else
                    {
                        int entityId = (from e in PhysicsBodiesByEntityId
                                        where e.Value == targetFixture.Body
                                        select e.Key).FirstOrDefault();
                        
                        var entity = worldState.EntityById(entityId);
                        var ball = entity as Ball;
                        if(ball != null)
                        {
                            ball.Health -= shot.HealthImpactAtDirectHit;
                            if (ball.Health < 0)
                                ball.Health = 0;
                        }
                    }
                }
                
            }

            // Remove all shots
            worldState.Shots.Clear();
        }
        }


        public void HandleMessage(object sender, Message message)
        {
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