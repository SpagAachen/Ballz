using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Physics2DDotNet;

namespace Ballz.Utils
{
	[Serializable]
	public class BalllzVec2
	{
		public BalllzVec2(float x, float y)
		{
			X = x;
			Y = y;
		}
		public float X = 0;
		public float Y = 0;
	}

    public static class VectorExtensions
    {
		public static BalllzVec2 ToBallz(Vector2 v)
		{
			return new BalllzVec2(v.X, v.Y);
		}

		public static Vector2 ToXna(this BalllzVec2 v)
		{
			return new Vector2(v.X, v.Y);
		}

        public static Vector2 ToXna(this AdvanceMath.Vector2D v)
        {
            return new Vector2(v.X, v.Y);
        }

        public static Vector2 ToXna(this ALVector2D v)
        {
            return new Vector2(v.X, v.Y);
        }

        public static AdvanceMath.Vector2D ToPhysics(this Vector2 v)
        {
            return new AdvanceMath.Vector2D(v.X, v.Y);
        }
    }
}
