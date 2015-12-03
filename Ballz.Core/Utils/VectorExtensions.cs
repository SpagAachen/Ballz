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
    }
}
