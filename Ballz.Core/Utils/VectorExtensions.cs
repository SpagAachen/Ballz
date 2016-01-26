using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

using static MathFloat.MathF;

namespace Ballz.Utils
{

    public static class VectorExtensions
    {

        public static Vector2 Rotate(this Vector2 v, float radians)
        {
            var ca = Cos(radians);
            var sa = Sin(radians);
            return new Vector2(ca * v.X - sa * v.Y, sa * v.X + ca * v.Y);
        }

        public static float RotationFromDirection(this Vector2 v)
        {
            return (v.X > 0 ? 1 : -1) * Acos(Vector2.Dot(Vector2.Normalize(v), Vector2.UnitY)) - (0.5f * (float)Math.PI);
        }
    }
}
