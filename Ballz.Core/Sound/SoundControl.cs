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
        private static float currentVolume;
        public static Dictionary<string, string> WinnerSounds = new Dictionary<string, string>();
        public static string ShotSound    = "Sounds/pew";
        public static string PistolSound  = "Sounds/peng05";
        public static string BazookaSound = "Sounds/rocket";
        public static string SelectSound  = "Sounds/drop01";
        public static string AcceptSound  = "Sounds/accept";
        public static string DeclineSound = "Sounds/decline";
        public static string MenuMusic    = "Sounds/badkitty";
        public static string DrownSound   = "Sounds/gurgel";
        private Dictionary<string,SoundEffect> loadedSounds;

        // Currently, we can only play one music at a time.
        private SoundEffectInstance music;

		bool SoundsEnabled = true;

        public SoundControl(Ballz game)
        {
           
            Game = game;
            loadedSounds = new Dictionary<string, SoundEffect>();
            WinnerSounds.Add("Germoney","Sounds/germoney");
            WinnerSounds.Add("Murica", "Sounds/freedom_fuckyeah");
        }

        private SoundEffect LoadSound(string name)
        {
            SoundEffect sound;
            try
            {
                sound = Game.Content.Load<SoundEffect>(name);
            }
            catch (Microsoft.Xna.Framework.Audio.NoAudioHardwareException e)
            {
                // If no audio hardware is installed, print a warning and don't try to load sounds again
                MessageOverlay.ShowAlert("Audio error", e.Message);
                SoundsEnabled = false;
                return null;
            }
            loadedSounds.Add(name, sound);
            return sound;
        }

		public SoundEffect GetSound(string name)
		{
			if (!SoundsEnabled)
				return null;
			
			SoundEffect sound;
			if(!loadedSounds.TryGetValue(name, out sound))
                sound = LoadSound(name);            

			return sound;
		}

        public void PlaySound(string name)
        {
			var sndEffect = GetSound(name);
			if(sndEffect != null)
            {
                SoundEffectInstance soundInstance = sndEffect.CreateInstance();
                soundInstance.Volume = (float)Game.GameSettings.MasterVolume.Value / (float)100;
                soundInstance.Play();
            }
        }

        private void UpdateMusicVolume()
        {
            currentVolume = (float)Game.GameSettings.MasterVolume.Value / (float)100;
            if (music.Volume != currentVolume)
                music.Volume = currentVolume;
        }

        private void InitMusic(SoundEffect sndEffect)
        {
            music = sndEffect.CreateInstance();
            music.IsLooped = true;
            music.Volume = (float)Game.GameSettings.MasterVolume.Value / (float)100;
            music.Play();
        }

        public void StartMusic(string name)
        {
			var sndEffect = GetSound(name);
			if(sndEffect != null)
            {
                // My work here is done
                if (music != null && music.State == SoundState.Playing)
                {
                    UpdateMusicVolume();
                    return;
                }

                InitMusic(sndEffect);
            }
        }

        public void StopMusic(string name)
        {
            if(music != null)
            {
                music.Stop();
            }
        }
    }
}