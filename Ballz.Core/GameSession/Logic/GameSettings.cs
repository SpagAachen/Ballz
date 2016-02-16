using System.Collections.Generic;

namespace Ballz.GameSession.Logic
{
    class Team
    {
        public string Name { get; set; } = "Unnamed";

        public int NumberOfBallz { get; set; } = 0;

        public bool ControlledByAI { get; set; } = false;
    }

    class GameSettings
    {
        public string MapName { get; set; } = "TestWorld2";

        public List<Team> Teams { get; set; }
    }
}
