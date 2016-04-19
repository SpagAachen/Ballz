using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ballz.GameSession.World
{
    public abstract class GraphicsEvent
    {
        public float Start { get; protected set; }
        public float End { get { return Start + Duration; } }
        public abstract float Duration { get; set; }

        public float GetProgress(float gameTime)
        {
            return (gameTime - Start) / (End - Start);
        }
    }

    public class GenericGraphicsEffect : GraphicsEvent
    {
        public Vector2 PositionStart { get; set; }
        public Vector2 PositionMid { get; set; }
        public Vector2 PositionEnd { get; set; }

        public float RotationStart { get; set; } = 0;
        public float RotationMid { get; set; } = 0;
        public float RotationEnd { get; set; } = 0;

        public float OpacityStart { get; set; } = 1;
        public float OpacityMid { get; set; } = 1;
        public float OpacityEnd { get; set; } = 1;

        public float ScaleStart { get; set; } = 1;
        public float ScaleMid { get; set; } = 1;
        public float ScaleEnd { get; set; } = 1;

        public Vector2 Position(float gameTime)
        {
            return PositionStart;
        }

        public float Rotation(float gameTime)
        {
            return RotationStart;
        }

        public float Opacity(float gameTime)
        {
            return OpacityStart;
        }

        public float Scale(float gameTime)
        {
            return ScaleStart;
        }

        public override float Duration
        {
            get; set;
        } = 1f;

        public string SpriteName { get; set; }

        public static GenericGraphicsEffect CreateExplosion(float gameTime, Vector2 position, float rotation, float scale = 1)
        {
            return new GenericGraphicsEffect
            {
                Start = gameTime,
                Duration = 0.2f,
                PositionStart = position,
                RotationStart = rotation,
                ScaleStart = scale,
                SpriteName = "Explosion"
            };
        }

        public static GenericGraphicsEffect CreateMuzzle(float gameTime, Vector2 position, float rotation)
        {
            return new GenericGraphicsEffect
            {
                Start = gameTime,
                Duration = 0.1f,
                PositionStart = position,
                RotationStart = rotation,
                SpriteName = "Muzzle"
            };
        }
    }

}
