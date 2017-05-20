using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ballz.Gui;
using GeonBit.UI.Entities;
using Ballz.GameSession.Logic;

namespace Ballz.Menu
{
    public class SinglePlayerMenu : MenuPanel
    {
        public SinglePlayerMenu() : base("Singleplayer")
        {
            var currGameSettings = new GameSettings();
            // hard-coded game settings
            {
                // Player 1
                {
                    var team1 = new Team
                    {
                        ControlledByAI = false,
                        Name = "Germoney",
                        Country = "Germoney",
                        NumberOfBallz = 2
                    };
                    currGameSettings.Teams.Add(team1);
                }
                // Player 2
                {
                    var team2 = new Team
                    {
                        ControlledByAI = false,
                        Name = "Murica",
                        Country = "Murica",
                        NumberOfBallz = 2
                    };
                    currGameSettings.Teams.Add(team2);
                }
            }

            // Select GameMode
            foreach (var factory in SessionFactory.SessionFactory.AvailableFactories)
            {
                var factoryLabel = new MenuButton(factory.Name);
                factoryLabel.OnClick += (e) =>
                {
                    currGameSettings.GameMode = factory;
                    Ballz.The().Logic.StartGame(currGameSettings);
                };
                AddItem(factoryLabel);
            }
            

            AddItem(new BackButton());
        }
    }
}
