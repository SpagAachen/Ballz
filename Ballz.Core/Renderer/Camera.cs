//
//  Camera.cs
//
//  Author:
//       Martin <Martin.Schultz@RWTH-Aachen.de>
//
//  Copyright (c) 2015 SPAG
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using Microsoft.Xna.Framework;

namespace Ballz
{
    //TODO: implement the camera properly
    public class Camera
    {
        public Matrix View{ get; private set;}

        public Matrix Projection{ get; private set;}

		private Vector2 CurrentPosition; //

        private Vector2 TargetPosition{ get; set;}

		private CameraTrajectory CurrentCameraTrajectory;
        private double lastMillis;
        private float AspectRatio;
		private float Zoom;
        public Camera()
        {
            View = new Matrix();
            Projection = new Matrix();
            CurrentPosition = new Vector2 (20, 0);
            TargetPosition = CurrentPosition;
            AspectRatio = 1;
            lastMillis = 0.0;
			Zoom = 1.0f;
        }

		public void SetZoom(float zoom)
		{
			Zoom = zoom;
		}

        public Camera(float aspectratio) :this()
        {
            AspectRatio = aspectratio;
        }

        public void SetView( Matrix view)
        {
            View = view;
        }

        public void SetAspectRatio( float r)
        {
            AspectRatio = r;
        }

        public void SetProjection(Matrix projection)
        {
            Projection = projection;
        }

        public void SetTargetPosition(Vector2 Position, GameTime t)
        {
            TargetPosition = Position;
            UpdateCurrentCameraPosition (t);
        }

		public void SwitchTarget(Vector2 targetPosition, GameTime t)
		{
			if (CurrentCameraTrajectory == null)
			{
				CurrentCameraTrajectory = new CameraTrajectory(CurrentPosition, targetPosition, t);
			}
		}

        private void UpdateCurrentCameraPosition(GameTime t)
        {
			if (CurrentCameraTrajectory == null) {
				Vector2 DiffPos = TargetPosition - CurrentPosition;

				double DiffTime = (t.TotalGameTime.TotalMilliseconds - lastMillis) / 1000.0;
				lastMillis = t.TotalGameTime.TotalMilliseconds;

				if (DiffPos.Length () == 0.0f) {
					return;
				}

				float speed = 3.0f + DiffPos.Length () * 1.0f;
				Vector2 delta = Vector2.Normalize (DiffPos) * speed * (float)DiffTime;

				if (delta.Length () > DiffPos.Length ()) {
					CurrentPosition = TargetPosition;
				} else {
					CurrentPosition += delta;
				}

				float dynamicZoom = Zoom - DiffPos.Length () * 0.01f;
				float x_size = 40.0f / dynamicZoom;
				float y_size = 40.0f / dynamicZoom / AspectRatio;
				SetView(Matrix.CreateOrthographicOffCenter(
					CurrentPosition.X-x_size/2.0f, CurrentPosition.X+x_size/2.0f, 
					CurrentPosition.Y-y_size/2.0f, CurrentPosition.Y+y_size/2.0f, 
					-20, 20));
			} else {
				if (CurrentCameraTrajectory.IsValid () == false) {
					CurrentCameraTrajectory = null;
					return;
				}
				Vector3 p = CurrentCameraTrajectory.GetCurrentPoint (t);
				CurrentPosition.X = p.X;
				CurrentPosition.Y = p.Y;
				float dynamicZoom = p.Z;
				float x_size = 40.0f / dynamicZoom;
				float y_size = 40.0f / dynamicZoom / AspectRatio;
				SetView(Matrix.CreateOrthographicOffCenter(
					p.X-x_size/2.0f, p.X+x_size/2.0f, 
					p.Y-y_size/2.0f, p.Y+y_size/2.0f, 
					-20, 20));
			}
        }
    }
}
