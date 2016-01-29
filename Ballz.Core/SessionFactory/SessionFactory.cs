using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ballz.GameSession;

namespace Ballz.SessionFactory
{
    public abstract class SessionFactory
    {
        public abstract Session StartSession(Ballz game);

        public abstract string Name {
            get;
        }

        public static IEnumerable<SessionFactory> AvailableFactories = new SessionFactory[]
        {
            new Worms("TestWorld"),
            new Worms("TestWorld2"),
            new Worms("RopeWorld"),
            new Ballerburg()
        };
    }
}
