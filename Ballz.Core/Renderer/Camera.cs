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

        private long lastTicks;
        private float AspectRatio;
        private float lastDiffTime;

        public Camera()
        {
            View = new Matrix();
            Projection = new Matrix();
            CurrentPosition = new Vector2 (20, 0);
            TargetPosition = CurrentPosition;
            AspectRatio = 1;
            lastDiffTime = 0;
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
            float speed = 20.0f;
            Vector2 DiffPos = TargetPosition - CurrentPosition;

            float DiffTime = ((float)(t.ElapsedGameTime.Ticks - lastTicks))/10000000.0f;
            lastTicks = t.ElapsedGameTime.Ticks;

            if (DiffTime <= 0) {
                DiffTime = lastDiffTime;
            } else {
                lastDiffTime = DiffTime;
            }

            Vector2 norm = DiffPos;
            norm.Normalize ();

            Vector2 delta = norm;
            delta.X = delta.X * speed * DiffTime;
            delta.Y = delta.Y * speed * DiffTime;

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
