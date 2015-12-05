//
//  Profilers.cs
//
//  Author:
//       Martin <>
//
//  Copyright (c) 2015 Martin
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
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace Ballz.Utils
{    
    public sealed class PerformanceReporter : IDisposable
    {
        private Stopwatch Sw;
        Ballz Game;
        private string Name;
        public PerformanceReporter(Ballz game)
        {     
            StackFrame frame = new StackFrame(1);
            var method = frame.GetMethod();
            var type = method.DeclaringType;
            Name = type.Name+"."+method.Name;
            Game=game;
            Sw = new Stopwatch();
            Sw.Start();
        }

        public void Dispose()
        {
            Sw.Stop();
            foreach (GameComponent c in Game.Components )
            {
                (c as PerformanceRenderer)?.reportTime(Name, Sw.Elapsed);
            }
        }
    }
}

