using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ballz.GameSession.World
{
    public class Rope
    {
        public Entity AttachedEntity;
        public Vector2 AttachedPosition;

        public const float MaxLength = 15f;
        public const float SegmentLength = 0.75f;

        public List<FarseerPhysics.Dynamics.Body> PhysicsSegments { get; set; } = new List<FarseerPhysics.Dynamics.Body>();

        public List<FarseerPhysics.Dynamics.Joints.Joint> PhysicsSegmentJoints { get; set; } = new List<FarseerPhysics.Dynamics.Joints.Joint>();

        public FarseerPhysics.Dynamics.Joints.Joint PhysicsEntityJoint { get; set; }
    }
}
