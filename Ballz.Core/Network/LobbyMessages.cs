using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ObjectSync;

namespace Ballz
{
    public class LobbyPlayerList
    {
        [Synced]
        public string[] PlayerNames;
    }
    public class LobbyPlayerGreeting
    {
        [Synced]
        public string PlayerName;
    }
}
