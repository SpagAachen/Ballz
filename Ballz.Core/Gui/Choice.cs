//
//  Choice.cs
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
using System.Collections.Generic;

namespace Ballz.Gui
{
    public class Choice<T> : Leaf, IChooseable 
    {
        private Settings.Setting<T> SelectedChoice;
        private List<T> Choices;
        private string previousDecorator = "", nextDecorator = "";

        public Choice(string name, Settings.Setting<T> selectedValue, List<T> chooseableValues, bool selectable = true) : base(name, selectable)
        {
            SelectedChoice = selectedValue;
            Choices = chooseableValues;

            OnSelect += () =>
            {
                if(!Active)
                {
                    ActiveChanged = true;
                    Active = true;
                }
                else
                    ActiveChanged = false;
            };
            OnUnSelect += () =>
            {
                if(Active)
                {
                    ActiveChanged = true;
                    Active = false;
                }
                else
                    ActiveChanged = false;
            };

            OnSelect += SelectionChanged;
            OnUnSelect += SelectionChanged;
        }

        private void SelectionChanged()
        {
            if(Active)
            {
                previousDecorator = "<< ";
                nextDecorator = " >>";
            }
            else
            {
                previousDecorator = "";
                nextDecorator = "";
            }
        }

        public override string DisplayName => (Name + previousDecorator+SelectedChoice.Value.ToString()+nextDecorator);

        public void SelectNext()
        {
            if(Active)
            {
                int index = Choices.IndexOf(SelectedChoice.Value);
                SelectedChoice.Value = Choices[(index+1)%Choices.Count];
            }
        }

        public void SelectPrevious()
        {
            if (Active)
            {
                int index = Choices.IndexOf(SelectedChoice.Value);
                index = ((index - 1) + Choices.Count) % Choices.Count;
                SelectedChoice.Value = Choices[index];
            }
        }
    }
}