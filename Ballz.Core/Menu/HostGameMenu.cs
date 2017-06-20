using GeonBit.UI.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ballz.Gui;
using Microsoft.Xna.Framework;
using GeonBit.UI.DataTypes;
using Ballz.GameSession.Logic;

namespace Ballz
{
    class HostGameMenu : Gui.MenuPanel
    {
        Label ErrorLabel = new Label("");
        TextInput GameName = new TextInput(false);
        TextInput Port = new TextInput(false);
        DropDown ModeSelect = new DropDown(new Microsoft.Xna.Framework.Vector2(0, -1));
        CheckBox TurnBased = new CheckBox("Turn-based");

        public HostGameMenu(bool isMultiplayer) : base(isMultiplayer ? "Host Game" : "Singleplayer Game")
        {
            Skin = PanelSkin.Default;

            if (isMultiplayer)
            {
                //AddItem(new Gui.InputBox("Public Name", true));
            }


            AddItem(new Label("Game Name:"));

            GameName.Value = Ballz.The().Settings.HostGameName;
            AddItem(GameName);
            
            AddItem(new Label("Map:"));
            // TODO: Map Select
            ModeSelect.AddItem("TestWorld2");
            ModeSelect.AddItem("Procedural");
            ModeSelect.AddItem("Desert");
            ModeSelect.SelectedIndex = 0;
            AddItem(ModeSelect);
            AddItem(TurnBased);

            ErrorLabel.SetStyleProperty("FillColor", new StyleProperty(Color.Red));
            AddItem(ErrorLabel);

            if (isMultiplayer)
            {
                var startGameButton = new MenuButton("Open Game", () =>
                {
                    var name = GameName.Value;
                    Ballz.The().Settings.HostGameName = name;
                    if (String.IsNullOrWhiteSpace(name))
                    {
                        ErrorLabel.Text = "Invalid Game Name!";
                        return;
                    }

                    var mapName = ModeSelect.SelectedValue;
                    var usePlayerTurns = TurnBased.Checked;

                    var settings = new MatchSettings
                    {
                        GameName = name,
                        IsPrivate = false,
                        GameMode = SessionFactory.SessionFactory.AvailableFactories.First(), // Todo: Select game modes
                        UsePlayerTurns = usePlayerTurns,
                        MapName = mapName,
                        Teams = new List<Team>()
                    };

                    Ballz.The().Logic.OpenMenu(new LobbyMenu(true, settings));
                });

                AddItem(startGameButton);
            }
            else
            {
                AddItem(new Label("Start Game"));
            }
            AddItem(new Gui.BackButton());
        }
    }
}
