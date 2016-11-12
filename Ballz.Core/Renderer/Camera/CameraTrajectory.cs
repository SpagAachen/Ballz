using System;
using Microsoft.Xna.Framework;

namespace Ballz
{
	public class CameraTrajectory
	{
		private bool valid;
        private Vector2 StartPoint { get; set; }
        private Vector2 EndPoint { get; set; }
		private TimeSpan StartTime;
		private float _speed;

		public CameraTrajectory (Vector2 startpoint, Vector2 endpoint, GameTime startTime)
		{
			StartPoint = startpoint;
			EndPoint = endpoint;
			StartTime = startTime.TotalGameTime;
			_speed = (EndPoint - StartPoint).Length () / (float)Math.PI; // 2 seconds fly time
			valid = true;
		}

		public bool IsValid()
		{
			return valid;
		}

		public Vector3 GetCurrentPoint(GameTime g)
		{
			Vector2 speed = Vector2.Normalize (EndPoint - StartPoint)*_speed;
			float distance = (EndPoint - StartPoint).Length ();

			float timeTraveled = (float)(g.TotalGameTime - StartTime).TotalSeconds + (float)(g.TotalGameTime - StartTime).TotalMilliseconds / 1000.0f;
			float traveledDistance = speed.Length() * timeTraveled;

			if (traveledDistance > distance) {
				valid = false;
				return new Vector3(EndPoint, 1.0f);
			}

			// zoom( x ) = a* (x - b)^2 + c;
			// zoom ( 0 ) = 1;
			// zoom ( distance ) = 1;
			// zoom ( distance/2 ) = 0.5;
			//
			float b = distance/2.0f;
			float c = 0.7f;
			float a = -0.3f / (distance*distance/4.0f);
			float zoom = -a * (traveledDistance - b)*(traveledDistance - b) + c;

            // I have no idea what is happening above, so let me just reduce the zoom by 80% and leave it alone.
            zoom = 0.8f + zoom * 0.2f;
			return new Vector3 ((StartPoint + (speed * timeTraveled)), zoom);
		}
	}
}

