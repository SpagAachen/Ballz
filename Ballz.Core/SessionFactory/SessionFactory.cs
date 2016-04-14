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
        // Must be called before StartSession
        // Can modify GameSettings! E.g., sets the map texture and mapName
        public void InitializeSession(Ballz game, GameSession.Logic.GameSettings settings)
        {
            ImplInitializeSession(game, settings);
            IsInitialized = true;
        }

        // StartSession must _not_ modify GameSettings
        public Session StartSession(Ballz game, GameSession.Logic.GameSettings settings, bool remoteControlled, int localPlayerId)
        {
            if (!IsInitialized) InitializeSession(game, settings);
            return ImplStartSession(game, settings, remoteControlled, localPlayerId);
        }

        protected abstract void ImplInitializeSession(Ballz game, GameSession.Logic.GameSettings settings);

        protected abstract Session ImplStartSession(Ballz game, GameSession.Logic.GameSettings settings, bool remoteControlled, int localPlayerId);

        public abstract string Name { get; }

        protected bool IsInitialized { get; set; } = false;

        public static IEnumerable<SessionFactory> AvailableFactories = new SessionFactory[]
        {
            new Worms("TestWorld2"),
            new Worms("TestWorld2", true),
            new Worms("TestWorld2", true),
            new Worms("RopeWorld"),
            new Worms("Mining"),
            new Worms("Mining", true)
        };
    }
}
