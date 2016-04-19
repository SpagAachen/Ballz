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

        public float RotationStart { get; set; }
        public float RotationMid { get; set; }
        public float RotationEnd { get; set; }

        public float OpacityStart { get; set; }
        public float OpacityMid { get; set; }
        public float OpacityEnd { get; set; }

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

        public override float Duration
        {
            get; set;
        } = 1f;

        public string SpriteName { get; set; }

        public static GenericGraphicsEffect CreateExplosion(float gameTime, Vector2 position, float rotation)
        {
            return new GenericGraphicsEffect
            {
                Start = gameTime,
                Duration = 0.2f,
                PositionStart = position,
                RotationStart = rotation,
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
