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
using Ballz.GameSession.World;

namespace Ballz
{
    //TODO: implement the camera properly
    public class Camera
    {
        public Matrix View{ get; set;}

        public Matrix Projection{ get; set;}

        public Vector2 BottomLeftBoundary { get; set; }
        public Vector2 TopRightBoundary { get; set; }
        public bool UseBoundary { get; set; } = false;

		private Vector2 CurrentPosition;
        
        private Vector2 TargetPosition{ get; set;}

		private CameraTrajectory CurrentCameraTrajectory;
        private Entity FocussedEntity;

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

        void ClampToBoundary(ref float top, ref float right, ref float bottom, ref float left)
        {
            if (UseBoundary)
            {
                if (left < BottomLeftBoundary.X)
                {
                    var clampOffset = BottomLeftBoundary.X - left;
                    left += clampOffset;
                    right += clampOffset;
                }
                if (bottom < BottomLeftBoundary.Y)
                {
                    var clampOffset = BottomLeftBoundary.Y - bottom;
                    bottom += clampOffset;
                    top += clampOffset;
                }
                if (right > TopRightBoundary.X)
                {
                    var clampOffset = right - BottomLeftBoundary.X;
                    right += clampOffset;
                    left += clampOffset;
                }
                if (top > TopRightBoundary.Y)
                {
                    var clampOffset = top - BottomLeftBoundary.Y;
                    top += clampOffset;
                    bottom += clampOffset;
                }
            }
        }

        private void UpdateCurrentCameraPosition(GameTime t)
        {
            if (CurrentCameraTrajectory != null)
            {
                if (CurrentCameraTrajectory.IsValid() == false)
                {
                    CurrentCameraTrajectory = null;
                    return;
                }
                Vector3 p = CurrentCameraTrajectory.GetCurrentPoint(t);
                CurrentPosition.X = p.X;
                CurrentPosition.Y = p.Y;
                float dynamicZoom = p.Z;
                float x_size = 40.0f / dynamicZoom;
                float y_size = 40.0f / dynamicZoom / AspectRatio;

                var left = p.X - x_size / 2.0f;
                var right = p.X + x_size / 2.0f;
                var bottom = p.Y - y_size / 2.0f;
                var top = p.Y + y_size / 2.0f;

                ClampToBoundary(ref top, ref right, ref bottom, ref left);

                // Clamp view frustum to boundary
                SetView(Matrix.CreateOrthographicOffCenter(
                    left, right, 
                    bottom, top, 
                    -20, 20));
                
            }
            else
            {
                Vector2 DiffPos = TargetPosition - CurrentPosition;

                double DiffTime = (t.TotalGameTime.TotalMilliseconds - lastMillis) / 1000.0;
                lastMillis = t.TotalGameTime.TotalMilliseconds;

                if (DiffPos.Length() == 0.0f)
                {
                    return;
                }

                float speed = 3.0f + DiffPos.Length() * 1.0f;
                Vector2 delta = Vector2.Normalize(DiffPos) * speed * (float)DiffTime;

                if (delta.Length() > DiffPos.Length())
                {
                    CurrentPosition = TargetPosition;
                }
                else
                {
                    CurrentPosition += delta;
                }

                float dynamicZoom = Zoom - DiffPos.Length() * 0.01f;
                float x_size = 40.0f / dynamicZoom;
                float y_size = 40.0f / dynamicZoom / AspectRatio;

                var left = CurrentPosition.X - x_size / 2.0f;
                var right = CurrentPosition.X + x_size / 2.0f;
                var bottom = CurrentPosition.Y - y_size / 2.0f;
                var top = CurrentPosition.Y + y_size / 2.0f;

                ClampToBoundary(ref top, ref right, ref bottom, ref left);

                SetView(Matrix.CreateOrthographicOffCenter(
                    left, right, 
                    bottom, top, 
                    -20, 20));
            }
        }
    }
}
