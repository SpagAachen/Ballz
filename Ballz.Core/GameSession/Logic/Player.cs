using Ballz.GameSession.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ballz.GameSession.Logic
{
    [Serializable]
    public class Player
    {
        private static int IdCounter = 1;

        public int Id { get; set; } = IdCounter++;

        [Newtonsoft.Json.JsonIgnore]
        public bool IsLocal { get; set; } = false;

        public string Name { get; set; }

        //TODO(MS): This is somehow redundant to GameSession.Logic.GameSettings.Teams
        //TODO(MS): TeamName currently specifies how Ballz are rendered. The name is currently somehow misleading.
        public string TeamName{ get; set; }

        public List<Ball> OwnedBalls { get; set; } = new List<Ball>();

        public Ball ActiveBall = null;
    }
}
