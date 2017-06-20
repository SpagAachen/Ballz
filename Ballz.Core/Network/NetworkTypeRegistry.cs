using Ballz.GameSession.Logic;
using Ballz.GameSession.World;
using Ballz.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ballz.Network
{
    public partial class Network
    {
        public void RegisterTypes()
        {
            SynchronizingInfo.RegisterClass<LobbyPlayerList>();
            SynchronizingInfo.RegisterClass<LobbyPlayerGreeting>();
            SynchronizingInfo.RegisterClass<SerializedMatchSettings>();
            SynchronizingInfo.RegisterClass<Team>();
            SynchronizingInfo.RegisterClass<Ball>();
            SynchronizingInfo.RegisterClass<Shot>();
            SynchronizingInfo.RegisterClass<Message>();
            SynchronizingInfo.RegisterClass<NetworkMessage>();
            SynchronizingInfo.RegisterClass<InputMessage>();
            SynchronizingInfo.RegisterClass<Terrain.TerrainModification>();

            SynchronizingInfo.RegisterIdentifiable<Entity>((id) => Ballz.The().Match.World.EntityById((int)id), (obj) => ((Entity)obj).ID);
        }
    }
}
