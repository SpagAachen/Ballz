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

        private Vector2 CurrentPosition{ get; set;}

        private Vector2 TargetPosition{ get; set;}

        private double lastMillis;
        private float AspectRatio;

        public Camera()
        {
            View = new Matrix();
            Projection = new Matrix();
            CurrentPosition = new Vector2 (20, 0);
            TargetPosition = CurrentPosition;
            AspectRatio = 1;
            lastMillis = 0.0;
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

        private void UpdateCurrentCameraPosition(GameTime t)
        {
            Vector2 DiffPos = TargetPosition - CurrentPosition;

            float speed = 5.0f + DiffPos.Length() * 2.0f;

            double DiffTime = (t.TotalGameTime.TotalMilliseconds - lastMillis) / 1000.0;
            lastMillis = t.TotalGameTime.TotalMilliseconds;

            Vector2 delta = Vector2.Normalize(DiffPos) * speed * (float)DiffTime;

            if (delta.Length() > DiffPos.Length()) {
                CurrentPosition = TargetPosition;
            } else {
                CurrentPosition += delta;
            }

            if (float.IsNaN (CurrentPosition.X) || float.IsNaN (CurrentPosition.Y)) {
                CurrentPosition = TargetPosition;
            }

            SetView(Matrix.CreateOrthographicOffCenter(CurrentPosition.X-20, 20+CurrentPosition.X, CurrentPosition.Y-20/AspectRatio, CurrentPosition.Y+20/AspectRatio, -20, 20));
        }
    }
}
