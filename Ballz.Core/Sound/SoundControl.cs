//
//  SoundControl.cs
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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;

namespace Ballz.Sound
{
    public class SoundControl
    {
        private Ballz Game;
        public static string ShotSound = "Sounds/pew";
        private Dictionary<string,SoundEffect> loadedSounds;

        public SoundControl(Ballz game)
        {
            Game = game;
            loadedSounds = new Dictionary<string, SoundEffect>();
        }

        public void PlaySound(string name)
        {
            //load sound if it is not already loaded
            if(!loadedSounds.ContainsKey(name))
                loadedSounds.Add(name,Game.Content.Load<SoundEffect>(name));
            SoundEffect sndEffect;
            if(loadedSounds.TryGetValue(name, out sndEffect))
            {
                SoundEffectInstance soundInstance = sndEffect.CreateInstance();
                soundInstance.Play();
            }
        }
    }
}