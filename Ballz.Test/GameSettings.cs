using NUnit.Framework;

namespace Ballz.Test
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using global::Ballz.GameSession.Logic;

    using Microsoft.Xna.Framework.Graphics;

    using Newtonsoft.Json;

    [TestFixture]
    public class GameSettingsTests
    {
        // Arrange
        public GameSettings CreateTestObject()
        {
            // create players
            var player1 = new Player { Id = 0, Name = "Player1", TeamName = "Team1"};
            var player2 = new Player { Id = 1, Name = "Player2", TeamName = "Team2" };
            // add some teams
            var teams = new List<Team>
                            {
                                new Team
                                    {
                                        Name = "Test1",
                                        ControlledByAI = false,
                                        NumberOfBallz = 1,
                                    },
                                new Team
                                    {
                                        Name = "Test2",
                                        ControlledByAI = true,
                                        NumberOfBallz = 2,
                                    }
                            };
            // create settings object
            var settings = new GameSettings
                               {
                                   GameMode = SessionFactory.SessionFactory.AvailableFactories.ElementAt(0),
                                   MapName = "TestWorld2",
                                   MapTexture = null,
                                   Teams = teams
                               };
            return settings;
        }

        // Assert
        public void ValidateTestObject(GameSettings settings)
        {
            Assert.NotNull(settings);
            Assert.Null(settings.MapTexture);
            Assert.NotNull(settings.GameMode);
            Assert.NotNull(settings.Teams);

            var gameMode = SessionFactory.SessionFactory.AvailableFactories.ElementAt(0);

            Assert.AreEqual(settings.GameMode, gameMode);
            Assert.AreEqual(settings.MapName, "TestWorld2");
            Assert.AreEqual(settings.Teams.Count, 2);

            {
                var team1 = settings.Teams.ElementAt(0);
                Assert.AreEqual(team1.ControlledByAI, false);
                Assert.AreEqual(team1.Name, "Test1");
                Assert.AreEqual(team1.NumberOfBallz, 1);
            }
            { 
                var team2 = settings.Teams.ElementAt(1);
                Assert.AreEqual(team2.ControlledByAI, true);
                Assert.AreEqual(team2.Name, "Test2");
                Assert.AreEqual(team2.NumberOfBallz, 2);
            }
        }

        [Test]
        public void CreateDestroy()
        {
            var obj = CreateTestObject();
            ValidateTestObject(obj);
        }

        // Act
        [Test]
        public void SerializeDeserialize()
        {
            //Arrange
            var obj = CreateTestObject();

            // Act
            var json = JsonConvert.SerializeObject(obj);
            var obj2 = JsonConvert.DeserializeObject<GameSettings>(json);

            // Assert
            ValidateTestObject(obj2);
        }
    }

}
