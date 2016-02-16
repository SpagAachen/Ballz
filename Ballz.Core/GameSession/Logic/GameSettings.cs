using System.Collections.Generic;
using Ballz.SessionFactory;

namespace Ballz.GameSession.Logic
{
    public class Team
    {
        public string Name { get; set; } = "Unnamed";

        public int NumberOfBallz { get; set; } = 0;

        public Player player { get; set; }

        public bool ControlledByAI { get; set; } = false;
    }

    public class GameSettings
    {
        public string MapName { get; set; } = "TestWorld2"; //TODO: Unused field.Reserved for after refactoring.

        public SessionFactory.SessionFactory GameMode { get; set; }

        public List<Team> Teams { get; set; } = new List<Team>();
    }
}
