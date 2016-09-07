using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathFloat;

namespace Ballz.GameSession.World
{
    public abstract class GraphicsEvent
    {
        public float Start { get; set; }
        public float End { get { return Start + Duration; } }
        public virtual float Duration { get; set; }

        public float GetProgress(float gameTime)
        {
            return (gameTime - Start) / (End - Start);
        }
    }

    public class GenericGraphicsEffect : GraphicsEvent
    {
        // Where is the only additional keyframe? \in 0..1
        private float _midFactor = 0.5f;
        public float   MidFactor     
        { 
            get
            {
                return _midFactor;
            }
            set
            {
                if(value <= 0 || value >= 1)
                    throw new ArgumentOutOfRangeException();
                
                _midFactor = value;
            } 
        }

        public Vector2 PositionStart { get; set; }
        public Vector2 PositionMid   { get; set; }
        public Vector2 PositionEnd   { get; set; }

        public float RotationStart   { get; set; } = 0;
        public float RotationMid     { get; set; } = 0;
        public float RotationEnd     { get; set; } = 0;

        public float OpacityStart    { get; set; } = 1;
        public float OpacityMid      { get; set; } = 1;
        public float OpacityEnd      { get; set; } = 1;

        public float ScaleStart      { get; set; } = 1;
        public float ScaleMid        { get; set; } = 1;
        public float ScaleEnd        { get; set; } = 1;

        public Vector2 Position(float gameTime)
        {
            var progress = GetProgress(gameTime);
            
            if (progress < MidFactor)
                return Vector2.Lerp(PositionStart, PositionMid, progress / MidFactor);

            // progress >= MidFactor
            return Vector2.Lerp(PositionMid, PositionEnd, (progress-MidFactor) / (1 - MidFactor));
        }

        // Why the hell is there no float.lerp(...) ?
        public static float Mix(float val0, float val1, float a)
        {
            return val0 + (val1 - val0) * a;
        }

        // Helper function
        private float interpolateFloat(float valStart, float valMid, float valEnd, float gameTime)
        {
            var progress = GetProgress(gameTime);

            if (progress < MidFactor)
                return Mix(valStart, valMid, progress / MidFactor);

            // progress >= MidFactor
            return Mix(valMid, valEnd, (progress-MidFactor) / (1 - MidFactor));
        }

        public float Rotation(float gameTime)
        {
            return interpolateFloat(RotationStart, RotationMid, RotationEnd, gameTime);
        }

        public float Opacity(float gameTime)
        {
            return interpolateFloat(OpacityStart, OpacityMid, OpacityEnd, gameTime);
        }

        public float Scale(float gameTime)
        {
            return interpolateFloat(ScaleStart, ScaleMid, ScaleEnd, gameTime);
        }
        
        public string SpriteName { get; set; }

        public static GenericGraphicsEffect CreateExplosion(float gameTime, Vector2 position, float rotation, float scaleStart = 1, float scaleEnd = 1.5f)
        {
            return new GenericGraphicsEffect
            {
                Start = gameTime,
                Duration = 0.2f,
                PositionStart = position,
                PositionMid = position,
                PositionEnd = position,
                RotationStart = rotation,
                RotationMid = rotation,
                RotationEnd = rotation,
                ScaleStart = scaleStart,
                ScaleEnd = scaleEnd,
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
                PositionMid = position,
                PositionEnd = position,
                RotationStart = rotation,
                RotationMid = rotation,
                RotationEnd = rotation,
                SpriteName = "Muzzle"
            };
        }
    }

    public class CameraShakeEffect: GraphicsEvent
    {
        public float Intensity { get; set; }
    }
}
