using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ballz.Gui;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;

namespace Ballz.Menu
{
    public class OptionsMenu : MenuPanel
    {
        TextInput PlayerName = new TextInput(false);
        CheckBox EnableFullscreen = new CheckBox("Fullscreen");
        DropDown Resolution = new DropDown(new Vector2(0, 280));
        Slider SoundVolume = new Slider(0, 100);
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

            AddItem(new Label("Player Name:"));
            AddItem(PlayerName);

            AddItem(EnableFullscreen);

            AddItem(new Label("Screen Resolution:"));
            AddItem(Resolution);

            AddItem(new Label("Sound Volume:"));
            AddItem(SoundVolume);

            AddItem(new Label("Music Volume:"));
            AddItem(MusicVolume);
            
            AddItem(new BackButton());
        }
    }
}
