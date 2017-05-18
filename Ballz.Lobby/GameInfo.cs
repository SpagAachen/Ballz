using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Ballz.Lobby
{
    public class PublicGameInfo
    {
        public string PublicId;
        public string Name;
        public string HostAddress;
        public int HostPort;
        public bool IsPrivate;
    }

    public class FullGameInfo: PublicGameInfo
    {
        public string Secret;
        public DateTime LastKeepAlive;
        
        public PublicGameInfo PublicInfo()
        {
            return new PublicGameInfo
            {
                PublicId = PublicId,
                Name = Name,
                HostAddress = HostAddress,
                HostPort = HostPort,
                IsPrivate = IsPrivate
            };
        }
    }
}
