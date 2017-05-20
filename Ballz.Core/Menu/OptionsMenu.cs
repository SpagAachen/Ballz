using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ballz.Gui;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using System.IO;

namespace Ballz.Menu
{
    public class OptionsMenu : MenuPanel
    {
        TextInput PlayerName = new TextInput(false);
        CheckBox EnableFullscreen = new CheckBox("Fullscreen");
        DropDown Resolution = new DropDown(new Vector2(0, 280));
        Slider MasterVolume = new Slider(0, 100);
        Slider MusicVolume = new Slider(0, 100);
        public OptionsMenu() : base("Options")
        {
            Skin = PanelSkin.Default;

            PlayerName.PlaceholderText = "Enter your Name";

            Resolution = new DropDown(new Vector2(0, 280));
            var resolutions = Ballz.The().GetResolutions();
            foreach (var resolution in resolutions)
            {
                Resolution.AddItem($"{resolution.Width}x{resolution.Height}");
            }

            LoadSettings();

            AddItem(new Label("Player Name:"));
            AddItem(PlayerName);

            AddItem(EnableFullscreen);

            AddItem(new Label("Screen Resolution:"));
            AddItem(Resolution);

            AddItem(new Label("Sound Volume:"));
            AddItem(MasterVolume);

            AddItem(new Label("Music Volume:"));
            AddItem(MusicVolume);
            
            var backButton = new MenuButton(
                "Back",
                size: new Vector2(0.5f, -1.0f),
                skin: ButtonSkin.Alternative,
                anchor: Anchor.BottomLeft,
                click: () => Ballz.The().Logic.MenuGoBack()
                );
            AddItem(backButton);
            var saveButton = new MenuButton(
                "OK",
                size: new Vector2(0.5f, -1.0f),
                skin: ButtonSkin.Alternative,
                anchor: Anchor.BottomRight,
                click: SaveSettings
                );
            AddItem(saveButton);
        }

        public void LoadSettings()
        {
            var GameSettings = Ballz.The().GameSettings;

            EnableFullscreen.Checked = GameSettings.Fullscreen.Value;
            PlayerName.Value = GameSettings.PlayerName.Value ?? "";
            if (Ballz.The().GetResolutions().Contains(Ballz.The().GameSettings.ScreenResolution?.Value))
            {
                Resolution.SelectedIndex = Ballz.The().GetResolutions().IndexOf(Ballz.The().GameSettings.ScreenResolution.Value);
            }

            MasterVolume.Value = GameSettings.MasterVolume.Value;
            MusicVolume.Value = GameSettings.MusicVolume.Value;
        }

        public void SaveSettings()
        {
            var Graphics = Ballz.The().Graphics;
            var GameSettings = Ballz.The().GameSettings;

            GameSettings.Fullscreen.Value = EnableFullscreen.Checked;
            GameSettings.PlayerName.Value = PlayerName.Value;
            if (Resolution.SelectedIndex >= 0)
            {
                GameSettings.ScreenResolution.Value = Ballz.The().GetResolutions()[Resolution.SelectedIndex];
            }
            GameSettings.MasterVolume.Value = MasterVolume.Value;
            GameSettings.MusicVolume.Value = MusicVolume.Value;

            // Store Settings in File
            FileStream stream = new FileStream("Settings.tmp.xml", FileMode.OpenOrCreate);
            Ballz.The().StoreSettings(stream);
            stream.Close();

            // Atomic move operation prevents corrupted settings file
            File.Replace("Settings.tmp.xml", "Settings.xml", "Settings.bak.xml");

            // Apply Graphics Settings
            Graphics.IsFullScreen = GameSettings.Fullscreen.Value;
            Graphics.PreferredBackBufferWidth = GameSettings.ScreenResolution.Value.Width;
            Graphics.PreferredBackBufferHeight = GameSettings.ScreenResolution.Value.Height;

            Graphics.ApplyChanges();
        }
    }
}
