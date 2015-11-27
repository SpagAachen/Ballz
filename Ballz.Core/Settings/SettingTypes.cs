//
//  BooleanSetting.cs
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
using Microsoft.Xna.Framework.Graphics;

namespace Ballz.Settings
{   
    [Serializable]
    public class Setting<T> 
    {
        public T Value{ get; set;}
    }

    [Serializable]
    public class Resolution : IComparable
    {
        public int Width{ get; set;}
        public int Height{ get; set;}
        public Resolution()
        {
        }

        public Resolution(int width, int height)
        {
            Width = width;
            Height = height;
        }
            
        public override string ToString()
        {
            return string.Format("{0}x{1}",Width,Height);
        }

        public override bool Equals(object obj)
        {
            return (obj as Resolution)?.Width == Width && (obj as Resolution)?.Height == Height;
        }

        public int CompareTo(object obj)
        {
            int? result;
            Resolution toCompare = obj as Resolution;
            if (toCompare != null)
            {
                if (Width.CompareTo(toCompare.Width) == 0)
                {
                    result = Height.CompareTo(toCompare.Height);
                }
                else
                {
                    result = Width.CompareTo(toCompare.Width);
                }
                if (result.HasValue)
                    return result.Value;
                else
                    return 0;
            }
            else
                return 0;
        }
    }
}

