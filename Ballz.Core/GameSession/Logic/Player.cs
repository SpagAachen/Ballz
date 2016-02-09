using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ballz.GameSession.Logic
{
    public class Player
    {
        private static int IdCounter = 1;

        public int Id { get; set; } = IdCounter++;

        public string Name { get; set; }
    }
}
