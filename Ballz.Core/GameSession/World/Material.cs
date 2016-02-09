using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Ballz.GameSession.World
{
    public class PhysicsMaterial
    {
        public static PhysicsMaterial Ball
        {
            get
            {
                PhysicsMaterial mat = new PhysicsMaterial();
                mat.Shape = PhysicsShape.Circle;
                //TODO: add reasonable Values here
                mat.Radius = 1.0f;
                mat.Density = 20f;
                mat.Restitution = .5f;
                mat.Friction = .5f;
                mat.Dampening = .5f;
                return mat;
            }
        }

        public enum PhysicsShape
        {
            Polygon,
            Circle
        }

        public float Density
        {
            get;
            set;
        }

        public float Friction
        {
            get;
            set;
        }

        public float Restitution
        {
            get;
            set;
        }

        public float Dampening
        {
            get;
            set;
        }

        public float Radius
        {
            get;
            set;
        }

        public List<Vector2> Vertices
        {
            get;
            set;
        }

        public PhysicsShape Shape
        {
            get;
            set;
        }
    }
}
