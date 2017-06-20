using Ballz.GameSession.Logic;
using Ballz.GameSession.World;
using Ballz.Messages;
using Microsoft.Xna.Framework;
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
            SynchronizingInfo.RegisterClass<GameStartInfo>();
            SynchronizingInfo.RegisterClass<Team>();
            SynchronizingInfo.RegisterClass<Message>();
            SynchronizingInfo.RegisterClass<NetworkMessage>();
            SynchronizingInfo.RegisterClass<InputMessage>();
            SynchronizingInfo.RegisterClass<Terrain.TerrainModification>();

            SynchronizingInfo.RegisterIdentifiable<Entity>(
                (id) => Ballz.The().Match.World.EntityById((int)id),
                (obj) => ((Entity)obj).ID,
                (msg, obj) => ((Entity)obj).Serialize(msg),
                (msg, obj) => ((Entity)obj).Deserialize(msg)
                );

            SynchronizingInfo.RegisterIdentifiable<Ball>(
                (id) => Ballz.The().Match.World.EntityById((int)id),
                (obj) => ((Entity)obj).ID,
                (msg, obj) => ((Ball)obj).Serialize(msg),
                (msg, obj) => ((Ball)obj).Deserialize(msg)
                );

            SynchronizingInfo.RegisterIdentifiable<Shot>(
                (id) => Ballz.The().Match.World.EntityById((int)id),
                (obj) => ((Entity)obj).ID,
                (msg, obj) => ((Shot)obj).Serialize(msg),
                (msg, obj) => ((Shot)obj).Deserialize(msg)
                );

        }
    }
}
