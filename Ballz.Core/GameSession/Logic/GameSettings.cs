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

    [Serializable]
    public class Team
    {
        static int IdCounter = 0;
        public int Id { get; set; } = IdCounter++;

        public string Name { get; set; } = "Unnamed";

        public string Country = "Germoney";

        public int NumberOfBallz { get; set; } = 1;
        
        public bool ControlledByAI { get; set; } = false;
    }

    [Serializable]
    [JsonConverter(typeof(MatchSettingsSerializer))]
    public class MatchSettings
    {
        public string MapName { get; set; } = "Invalid";

        public Texture2D MapTexture { get; set; }

        public SessionFactory.SessionFactory GameMode { get; set; }

        public List<Team> Teams { get; set; } = new List<Team>();

        public bool UsePlayerTurns { get; set; } = false;
    }

    public class MatchSettingsSerializer : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            var gameSettings = value as MatchSettings;
            Debug.Assert(gameSettings != null);
            // MapName
            {
                writer.WritePropertyName("MapName");
                serializer.Serialize(writer, gameSettings.MapName);
            }
            // UsePlayerTurns
            {
                writer.WritePropertyName("UsePlayerTurns");
                serializer.Serialize(writer, gameSettings.UsePlayerTurns);
            }
            // MapTexture
            {
                writer.WritePropertyName("MapTexture");
                if (gameSettings.MapTexture != null)
                {
                    var mapData = Utils.TextureHelper.SaveTextureData(gameSettings.MapTexture);
                    serializer.Serialize(writer, mapData);
                }
                else
                {
                    serializer.Serialize(writer, null);
                }
            }
            // GameMode
            {
                writer.WritePropertyName("GameMode");
                var gmIdx = Utils.EnumberableHelper.IndexOf(SessionFactory.SessionFactory.AvailableFactories, gameSettings.GameMode);
                serializer.Serialize(writer, gmIdx);
            }
            // Teams
            {
                writer.WritePropertyName("Teams");
                serializer.Serialize(writer, gameSettings.Teams);
            }
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var properties = jsonObject.Properties().ToList();
            var mapTex = (string)properties[2].Value;
            return new MatchSettings
            {
                MapName = (string)properties[0].Value,
                UsePlayerTurns = (bool)properties[1].Value,
                MapTexture = mapTex == null?null:Utils.TextureHelper.LoadTextureData((string)properties[2].Value),
                GameMode = SessionFactory.SessionFactory.AvailableFactories.ElementAt((int)properties[3].Value),
                Teams = JsonConvert.DeserializeObject<List<Team>>(properties[4].Value.ToString())
            };
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(MatchSettings).IsAssignableFrom(objectType);
        }
    }
}
