//
//  Programmsettings.cs
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
using System.Runtime.Serialization;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace Ballz.Settings
{
    [Serializable]
    public class GameSettings
    {
        public bool Fullscreen = false;
        public Resolution ScreenResolution = new Resolution(1280, 720);
        public int MasterVolume = 50;
        public int MusicVolume = 0;
        public int MSAASamples = 4;
        public string PlayerName = Environment.UserName;
        public string HostGameName = Environment.UserName + "'s Game";
        public bool FriendlyFire = false;

        public const string SettingsFilename = "Settings.json";
        public static GameSettings Load()
        {
            GameSettings settings = new GameSettings();
            if (File.Exists(SettingsFilename))
            {
                try
                {
                    var serialized = File.ReadAllText(SettingsFilename, Encoding.UTF8);
                    settings = JsonConvert.DeserializeObject<GameSettings>(serialized);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error while reading settings, replacing with default settings. Message: {e.Message}");
                }
            }

            return settings;
        }

        public void Save()
        {
            var serialized = JsonConvert.SerializeObject(this);

            // First write to temporary file, then atomically replace old file to prevent corrupted contents
            var tmpFilename = SettingsFilename + ".tmp";
            File.WriteAllText(tmpFilename, serialized, Encoding.UTF8);
            if (File.Exists(SettingsFilename))
            {
                File.Replace(tmpFilename, SettingsFilename, SettingsFilename + ".bak");
            }
            else
            {
                File.Move(tmpFilename, SettingsFilename);
            }
        }
    }
}