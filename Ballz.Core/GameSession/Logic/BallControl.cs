using Ballz.GameSession.World;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ballz.GameSession.Logic
{
    public class BallControl
    {
        public Session Match;
        public Ballz Game;

        public BallControl(Ballz game, Session match)
        {
            Game = game;
            Match = match;
        }
        
        public void UpdateBall(Ball ball)
        {

        }
    }
}
