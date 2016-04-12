using Ballz.GameSession.World;
using Ballz.Messages;
using Ballz.Utils;
using FarseerPhysics;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static MathFloat.MathF;

namespace Ballz.GameSession.Physics
{
    /// <summary>
    /// Physics control is called by BallzGame update to simulate discrete GamePhysics.
    /// </summary>
    /// <remarks>
    /// In each update step, this component syncs the state of all entities into the physics world,
    /// runs a simulation step, and syncs positions and velocities back to the entities.
    /// </remarks>
    public class PhysicsControl : GameComponent
    {
        new Ballz Game;

        /// <summary>
        /// The main acces point to the physics engine.
        /// </summary>
        FarseerPhysics.Dynamics.World PhysicsWorld;

        public PhysicsControl(Ballz game) : base(game)
        {
            Game = game;
        }
        
        Dictionary<Body, int> EntityIdByPhysicsBody = new Dictionary<Body, int>();

        Dictionary<Body, Rope> RopesByPhysicsBody = new Dictionary<Body, Rope>();

        /// <summary>
        /// List of the physics bodies that represent the terrain
        /// </summary>
        List<Body> TerrainBodies = new List<Body>();

        Terrain Terrain = null;

        /// <summary>
        /// Terrain revision of the physics terrain shapes in <see cref="TerrainBodies"/>
        /// </summary>
        int TerrainRevision = -1;

        public override void Initialize()
        {
            PhysicsWorld = new FarseerPhysics.Dynamics.World(new Vector2(0f, -9.82f));

            // Add a ground plate for physics testing
            var ground = new Body(PhysicsWorld);
            ground.BodyType = BodyType.Static;
            ground.CreateFixture(new EdgeShape(new Vector2(-10, 0), new Vector2(20, 0)));
        }

        /// <summary>
        /// Removes the old terrain physics bodies and replaces them with new ones,
        /// generated from the given terrain.
        /// </summary>
        public void UpdateTerrainBody(Terrain terrain)
        {
            Terrain = terrain;
            foreach (var body in TerrainBodies)
            {
                var fixtures = body.FixtureList.ToArray();
                foreach (var fixture in fixtures)
                {
                    fixture.Dispose();
                }

                body.Dispose();
            }

            TerrainBodies.Clear();

            // Update the terrain explicitly
            terrain.Update();
            var outlines = terrain.GetOutline();
            
            foreach (var outline in outlines)
            {
                var vertices = new Vector2[outline.Count];

                for (int i = 0; i < outline.Count; i++)
                {
                    vertices[i] = outline[i] * terrain.Scale;
                }

                var shape = new ChainShape(new Vertices(vertices));

                var body = new Body(PhysicsWorld);
                body.BodyType = BodyType.Static;
                body.Friction = 1.0f;
                body.CreateFixture(shape);

                TerrainBodies.Add(body);
            }
        }

        public void RemoveBody(Body body)
        {
            var fixtures = body.FixtureList.ToArray();
            foreach (var fixture in fixtures)
            {
                fixture.Dispose();
            }

            body.Dispose();
        }

        /// <summary>
        /// Removes the physics body of the given entity from the physics world.
        /// </summary>
        public void RemoveEntity(Entity e, World.World worldState)
        {
            e.Dispose();
            worldState.Entities.Remove(e);
            if(e.PhysicsBody != null)
            {
                EntityIdByPhysicsBody.Remove(e.PhysicsBody);
                RemoveBody(e.PhysicsBody);
                e.PhysicsBody = null;
            }
        }

        public void AddRope(Rope rope)
        {
            float ropeLength = (rope.AttachedEntity.Position - rope.AttachedPosition).Length();
            int segmentCount = (int)Round(1 + (ropeLength / Rope.SegmentLength));

            if (segmentCount < 2)
                segmentCount = 2;

            var ropeSegmentDir = Vector2.Normalize(rope.AttachedEntity.Position - rope.AttachedPosition);

            // TODO: fix the RotationFromDirection function!
            var ropeRotation = ropeSegmentDir.RotationFromDirection();

            rope.PhysicsSegments.Clear();

            for (int i = 0; i<segmentCount; i++)
            {
                var ropeSegmentLength = i > 0 ? Rope.SegmentLength : Rope.Diameter;
                var ropeSegmentVector = ropeSegmentDir * ropeSegmentLength;
                var segmentStart  = rope.AttachedPosition + ropeSegmentVector * i;
                var segmentCenter = rope.AttachedPosition + ropeSegmentVector * (i + 0.5f);

                var boxWidth = Rope.Diameter;
                var boxHeight = i > 0 ? Rope.SegmentLength : Rope.Diameter;

                //var segment = BodyFactory.CreateCircle(PhysicsWorld, Rope.JointRadius, Rope.SegmentDensity);// BodyFactory.CreateRectangle(PhysicsWorld, 0.05f, Rope.SegmentLength, 0.5f);
                var segment = BodyFactory.CreateRectangle(PhysicsWorld, boxWidth, boxHeight, Rope.SegmentDensity);
                segment.BodyType = BodyType.Dynamic;
                segment.Friction = 0.5f;
                segment.Restitution = 0.2f;
                segment.AngularDamping = 1.1f;
                segment.LinearDamping = 1.1f;
                segment.CollisionCategories = Category.Cat3;
                segment.SetTransform(segmentCenter, -ropeRotation -  0.5f * (float)Math.PI);

                rope.PhysicsSegments.Add(segment);

                if (i > 0)
                {
                    var segmentJoint = JointFactory.CreateRevoluteJoint(PhysicsWorld, rope.PhysicsSegments[i - 1], segment, new Vector2(0, i == 1 ? Rope.Diameter/2 : Rope.SegmentLength/2), new Vector2(0, -Rope.SegmentLength/2));
                    segmentJoint.CollideConnected = false;
                    rope.PhysicsSegmentJoints.Add(segmentJoint);
                }
                else
                {
                    segment.BodyType = BodyType.Static;
                }
             }
            //var ballAnchor = rope.AttachedEntity.Position + new Vector2(0, rope.AttachedEntity.Radius + 1f);
            var ballJoint = JointFactory.CreateRevoluteJoint(PhysicsWorld, rope.PhysicsSegments.Last(), rope.AttachedEntity.PhysicsBody, new Vector2(0, Rope.SegmentLength/2), Vector2.Zero);
            ballJoint.CollideConnected = false;
            rope.PhysicsEntityJoint = ballJoint;
            
            rope.AttachedEntity.PhysicsBody.FixedRotation = false;
            //rope.AttachedEntity.PhysicsBody.Mass = 2f;
        }

        public void RemoveRope(Rope rope)
        {
            foreach (var joint in rope.PhysicsSegmentJoints)
            {
                PhysicsWorld.RemoveJoint(joint);
            }

            rope.PhysicsSegmentJoints.Clear();

            foreach (var body in rope.PhysicsSegments)
            {
                RopesByPhysicsBody.Remove(body);
                RemoveBody(body);
            }

            rope.PhysicsSegments.Clear();
            if (rope.AttachedEntity is Ball)
            {
                (rope.AttachedEntity as Ball).PhysicsBody.Mass = 10f;
                (rope.AttachedEntity as Ball).PhysicsBody.FixedRotation = true;
            }
        }

        public void ShortenRope(Rope rope)
        {
            if (rope.PhysicsSegments.Count <= 2)
            {
                // Rope cannot be any shorter as 2 segments is minimum
                return;
            }

            // Only allow shortening rope if all joints are restful
            for(int i = 0; i < rope.PhysicsSegmentJoints.Count; ++i)
            {
                var joint = rope.PhysicsSegmentJoints[i];
                if (Vector2.Distance(joint.WorldAnchorA, joint.WorldAnchorB) > 0.1f * Rope.SegmentLength)
                    return;
            }

            PhysicsWorld.RemoveJoint(rope.PhysicsEntityJoint);
            PhysicsWorld.RemoveJoint(rope.PhysicsSegmentJoints.Last());
            rope.PhysicsSegmentJoints.Remove(rope.PhysicsSegmentJoints.Last());


            RemoveBody(rope.PhysicsSegments.Last());
            rope.PhysicsSegments.Remove(rope.PhysicsSegments.Last());

            var ballJoint = JointFactory.CreateRevoluteJoint(PhysicsWorld, rope.PhysicsSegments.Last(), rope.AttachedEntity.PhysicsBody, Vector2.Zero, Vector2.Zero);
            ballJoint.CollideConnected = false;
            rope.PhysicsEntityJoint = ballJoint;

        }

        public void LoosenRope(Rope rope)
        {
            if (rope.PhysicsSegments.Count * Rope.SegmentLength >= Rope.MaxLength)
            {
                // Maximum length reached, rope cannot be made any longer
                return;
            }

            var lastSegment = rope.PhysicsSegments.Last();
            var ropeSegmentVector = Vector2.Normalize(rope.AttachedEntity.Position - lastSegment.Position) * Rope.SegmentLength;
            PhysicsWorld.RemoveJoint(rope.PhysicsEntityJoint);

            //var newSegment = BodyFactory.CreateCircle(PhysicsWorld, Rope.SegmentWidth, Rope.SegmentDensity);// BodyFactory.CreateRectangle(PhysicsWorld, 0.05f, Rope.SegmentLength, 0.5f);
            var newSegment = BodyFactory.CreateRectangle(PhysicsWorld, Rope.Diameter, Rope.SegmentLength, Rope.SegmentDensity);
            newSegment.BodyType = BodyType.Dynamic;
            newSegment.AngularDamping = 0.1f;
            newSegment.LinearDamping = 0.1f;
            newSegment.Friction = 0.5f;
            newSegment.Restitution = 0.2f;
            newSegment.CollisionCategories = Category.Cat3;
            newSegment.Position = lastSegment.Position + ropeSegmentVector;

            rope.PhysicsSegments.Add(newSegment);

            var segmentJoint = JointFactory.CreateRevoluteJoint(PhysicsWorld, lastSegment, newSegment, new Vector2(0, Rope.SegmentLength/2), new Vector2(0, -Rope.SegmentLength/2) );
            segmentJoint.CollideConnected = false;
            rope.PhysicsSegmentJoints.Add(segmentJoint);

            var ballJoint = JointFactory.CreateRevoluteJoint(PhysicsWorld, rope.PhysicsSegments.Last(), rope.AttachedEntity.PhysicsBody, Vector2.Zero, Vector2.Zero);
            ballJoint.CollideConnected = false;
            rope.PhysicsEntityJoint = ballJoint;
        }

        /// <summary>
        /// Syncs the given world state into the physics world.
        /// Creates new physics bodies for entities that don't have one yet.
        /// </summary>
        public void PreparePhysicsEngine(World.World worldState)
        {
            if (worldState.StaticGeometry.Revision != TerrainRevision)
                UpdateTerrainBody(worldState.StaticGeometry);

            TerrainRevision = worldState.StaticGeometry.Revision;

            // Add Bodies for new entities
            var entities = worldState.Entities.ToArray();
            foreach (var e in entities)
            {
                if(e.Disposed || e.Position.LengthSquared() > 100 * 100)
                {
                    RemoveEntity(e, worldState);
                    continue;
                }

                Body body = e.PhysicsBody;
                if (body == null)
                {
                    body = new Body(PhysicsWorld);
                    body.BodyType = e.IsStatic ? BodyType.Static : BodyType.Dynamic;
                    body.SleepingAllowed = true;
                    body.Awake = true;
                    body.CreateFixture(new CircleShape(e.Radius, 1.0f));
                    e.PhysicsBody = body;
                    EntityIdByPhysicsBody[body] = e.ID;
                    if (e is Ball)
                    {
                        body.Friction = 2.0f;
                        body.Restitution = 0.1f;
                        body.FixedRotation = true;
                        body.Mass = 10f;
                    }

                    if (e is Shot)
                    {
                        var shot = e as Shot;
                        body.OnCollision += (a, b, contact) =>
                        {
                            Vector2 normal;
                            FixedArray2<Vector2> points;
                            contact.GetWorldManifold(out normal, out points);

                            Fixture targetFixture;
                            if (contact.FixtureA.Body == shot.PhysicsBody)
                                targetFixture = contact.FixtureB;
                            else
                                targetFixture = contact.FixtureA;
                                
                            if (TerrainBodies.Contains(targetFixture.Body))
                            {
                                // Shot collides with terrain
                                shot.OnTerrainCollision(worldState.StaticGeometry, points[0]);
                            }
                            else if (EntityIdByPhysicsBody.ContainsKey(targetFixture.Body))
                            {
                                int entityId = EntityIdByPhysicsBody[targetFixture.Body];
                                Entity entity = worldState.EntityById(entityId);

                                // Mutual collision
                                entity.OnEntityCollision(shot);
                                shot.OnEntityCollision(entity);
                            }

                            return true;
                        };
                    }
                }
                
                if (body.Position != e.Position || body.Rotation != e.Rotation)
                    body.SetTransform(e.Position, e.Rotation);
                if (body.LinearVelocity != e.Velocity)
                    // Apparently, the physics engine likes applying an impulse better than overwriting the velocity.
                    // So, apply an impulse that changes the old velocity to the new one.
                    body.ApplyLinearImpulse(body.Mass * (e.Velocity - body.LinearVelocity));
            }
        }

        /// <summary>
        /// Runs a single simulation step in the physics engine and syncs the state of the physics bodies back to the given world.
        /// </summary>
        public void PhysicsStep(World.World worldState, float elapsedSeconds)
        {
            // Update the physics world
            PhysicsWorld.Step(elapsedSeconds);
            worldState.Water.Step(worldState, elapsedSeconds);

            // Sync back the positions and velocities
            foreach (var e in worldState.Entities)
            {
                var body = e.PhysicsBody;

                if (e.PhysicsBody == null || e.Disposed)
                    continue;

                e.Position = body.Position;
                e.Velocity = body.LinearVelocity;

                if (e is Ball)
                {
                    float eps = 0.01f;
                    if (e.Velocity.X > eps)
                        e.Rotation = (float)Math.PI * 0.5f;
                    else if (e.Velocity.X < -eps)
                        e.Rotation = -(float)Math.PI * 0.5f;
                }
            }
        }

        public override void Update(GameTime time)
        {
            using (new PerformanceReporter(Game))
            {
                if (Game.Match.State != Logic.SessionState.Running)
                    return;

                var worldState = Game.World;
                float elapsedSeconds = (float)time.ElapsedGameTime.TotalSeconds;

                PreparePhysicsEngine(worldState);
                PhysicsStep(worldState, elapsedSeconds);

                // Update shots
                var shots = (from e in worldState.Entities
                             where e is Shot
                             select (e as Shot))
                             .ToArray();

                foreach (var shot in shots)
                {
                    if (shot.ShotType == Shot.ShotType_T.InstantHit)
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

                        PhysicsWorld.RayCast(raycastCallback, shot.Position, shot.Position + 100 * shot.Velocity);
                        if (TerrainBodies.Contains(targetFixture.Body))
                        {
                            // Shot collides with terrain
                            shot.OnTerrainCollision(worldState.StaticGeometry, targetPos);
                        }
                        else if (EntityIdByPhysicsBody.ContainsKey(targetFixture.Body))
                        {
                            int entityId = EntityIdByPhysicsBody[targetFixture.Body];
                            Entity entity = worldState.EntityById(entityId);

                            // Mutual collision
                            entity.OnEntityCollision(shot);
                            shot.OnEntityCollision(entity);
                        }

                        worldState.Entities.Remove(shot);
                    }
                    else
                    {
                        shot.update(elapsedSeconds);
                    }
                }
            }
        }

        public class RaycastResult
        {
            public bool HasHit { get; set; } = false;

            public Entity Entity { get; set; } = null;

            public Vector2 Position { get; set; } = Vector2.Zero;

            public Vector2 Normal { get; set; } = Vector2.Zero;
        }

        public RaycastResult Raycast(Vector2 rayStart, Vector2 rayEnd)
        {
            float closestFraction = float.PositiveInfinity;
            RaycastResult closestHit = new RaycastResult();

            PhysicsWorld.RayCast((Fixture fixture, Vector2 position, Vector2 normal, float fraction) =>
            {
                Entity hitEntity = null;
                if (EntityIdByPhysicsBody.ContainsKey(fixture.Body))
                {
                    hitEntity = Game.World.EntityById(EntityIdByPhysicsBody[fixture.Body]);
                }

                if (fraction < closestFraction)
                {
                    closestFraction = fraction;
                    closestHit = new RaycastResult
                    {
                        HasHit = true,
                        Position = position,
                        Normal = normal,
                        Entity = hitEntity
                    };
                }

                return fraction;
            }, rayStart, rayEnd);

            return closestHit;
        }
        
        public bool IsEmpty(Vector2 Position)
        {
            var fixture = PhysicsWorld.TestPoint(Position);
            int terrainPosX = (int)(Position.X / Terrain.Scale);
            int terrainPosY = (int)(Position.Y / Terrain.Scale);
            bool hasTerrain = false;
            if (terrainPosX > 0 && terrainPosX < Terrain.width && terrainPosY > 0 && terrainPosY < Terrain.height)
            {
                hasTerrain = Terrain.PublicShape.TerrainBitmap[terrainPosX, terrainPosY];
            }
            
            return fixture == null && !hasTerrain;
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