//
//  CheckBox.cs
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

namespace Ballz.Menu
{
    public class CheckBox : Leaf
    {
        readonly Settings.Setting<bool> value;
        public CheckBox(string name, Settings.Setting<bool> value = null, bool selectable = true ) : base(name, selectable)
        {
            this.value = value;
            if (this.value != null)
                OnSelect += () => this.value.Value = !this.value.Value;
        }

        public override string DisplayName => Name + (value?.Value.ToString() ?? "");
    }
}

