using System;
using System.Collections.Generic;

namespace Ballz.GameSession.Logic
{
    using System.Diagnostics;
    using System.Linq;

    using Microsoft.Xna.Framework.Graphics;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    //TODO: Use UsePlayerTurns variable
    
    public class Team
    {
        static int IdCounter = 0;
        public int Id { get; set; } = IdCounter++;

        public string Name { get; set; } = "Unnamed";

        public string Country = "Germoney";

        public int NumberOfBallz { get; set; } = 1;
        
        public bool ControlledByAI { get; set; } = false;
    }
    
    public class MatchSettings
    {
        public string GameName { get; set; } = "Unnamed Game";

        public bool IsPrivate { get; set; } = true;

        public string MapName { get; set; } = "Invalid";

        public Texture2D MapTexture { get; set; }

        public SessionFactory.SessionFactory GameMode { get; set; }

        public List<Team> Teams { get; set; } = new List<Team>();

        public bool UsePlayerTurns { get; set; } = false;

        public static MatchSettings Deserialize(SerializedMatchSettings serialized)
        {
            return new MatchSettings
            {
                GameName = serialized.GameName,
                GameMode = SessionFactory.SessionFactory.AvailableFactories.ToList()[serialized.GameMode],
                IsPrivate = serialized.IsPrivate,
                MapName = serialized.MapName,
                Teams = serialized.Teams,
                MapTexture = Utils.TextureHelper.LoadTextureData(serialized.MapTexture),
                UsePlayerTurns=serialized.UsePlayerTurns
            };
        }

        public SerializedMatchSettings Serialize()
        {
            return new SerializedMatchSettings
            {
                GameName = GameName,
                GameMode = SessionFactory.SessionFactory.AvailableFactories.ToList().IndexOf(GameMode),
                IsPrivate = IsPrivate,
                MapName = MapName,
                MapTexture = Utils.TextureHelper.SaveTextureData(MapTexture),
                Teams = Teams,
                UsePlayerTurns = UsePlayerTurns
            };
        }
    }

    public class SerializedMatchSettings
    {
        public string GameName;
        public bool IsPrivate;
        public string MapName;
        public string MapTexture;
        public int GameMode;
        public List<Team> Teams;
        public bool UsePlayerTurns;
    }
    
}
