using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ballz.GameSession;
using Ballz.GameSession.World;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Ballz.GameSession.Logic;

namespace Ballz.SessionFactory
{
    public class Worms : SessionFactory
    {
        public Worms(string mapName = "TestWorld2", bool includeAI = false, bool usePlayerTurns = false)
        {
            MapName = mapName;
            IncludeAI = includeAI;
            UsePlayerTurns = usePlayerTurns;
        }

        public string MapName;
        public bool IncludeAI;
        public bool UsePlayerTurns;

        public override string Name { get { return "Worms (" + MapName + (IncludeAI ? ", with NPC" : "") + (UsePlayerTurns ? ", turn mode" : "") + ")"; } }

        public List<Vector2> SpawnPoints = new List<Vector2>();

        public void FindSpawnPoints(Texture2D map, float terrainScale)
        {
            var w = map.Width;
            var h = map.Height;
            Color[] pixels = new Color[w * h];
            map.GetData<Color>(pixels);

            // Spawn points are identified by green pixels in the map
            var spawnPointColor = new Color(0f, 1f, 0f);
            
            for(int x = 0; x < w; x++)
            {
                for(int y = 0; y < h; y++)
                {
                    if (pixels[y * w + x] == spawnPointColor)
                        SpawnPoints.Add(new Vector2(x * terrainScale, (h - y) * terrainScale));
                }
            }
        }

        public List<Vector2> SelectSpawnpoints(int count)
        {
            var spawns = new List<int>();
            var rand = new Random();
            
            for(int i = 0; i < count; i++)
            {
                bool foundSpawn = false;

                // Make a limited number of tries to find a good spawn point
                for(int j = 0; j < 20; j++)
                {
                    int spawnIndex = rand.Next(SpawnPoints.Count);
                    if (!spawns.Contains(spawnIndex))
                    {
                        spawns.Add(spawnIndex);
                        foundSpawn = true;
                        break;
                    }
                }

                // If that didn't work, just pick some spawn point
                if(!foundSpawn)
                {
                    spawns.Add(rand.Next(SpawnPoints.Count));
                }
            }
            
            return spawns.Select((i)=>SpawnPoints[i]).ToList();
        }

        public override Session StartSession(Ballz game)
        {
            var session = new Session(game);

            session.UsePlayerTurns = UsePlayerTurns;

            var mapTexture = game.Content.Load<Texture2D>("Worlds/" + MapName);
            session.Terrain = new Terrain(mapTexture);

            FindSpawnPoints(mapTexture, session.Terrain.Scale);
            var spawnPoints = SelectSpawnpoints(IncludeAI ? 3 : 2);

            var player1 = new Player
            {
                Name = "Player1"
            };

            session.Players.Add(player1);

            var player1Ball = new Ball
            {
                Position = spawnPoints[0],
                IsAiming = true,
                Player = player1,
                HoldingWeapon = "Bazooka",
            };
            session.Entities.Add(player1Ball);
            session.SessionLogic.BallControllers[player1] = new UserControl(game, session, player1Ball);

            var player2 = new Player
            {
                Name = "Player2"
            };
            session.Players.Add(player2);

            var player2Ball = new Ball
            {
                Position = spawnPoints[1],
                Velocity = Vector2.Zero,
                IsAiming = true,
                Player = player2,
                HoldingWeapon = "HandGun",
            };
            session.Entities.Add(player2Ball);

            session.SessionLogic.BallControllers[player2] = new UserControl(game, session, player2Ball);

            if (IncludeAI)
            {
                var playerAi = new Player
                {
                    Name = "NPC"
                };
                session.Players.Add(playerAi);

                var aiBall = new Ball
                {
                    Position = spawnPoints[2],
                    Player = playerAi
                };
                session.Entities.Add(aiBall);
                session.SessionLogic.BallControllers[playerAi] = new AIControl(game, session, aiBall);
            }

            var snpsht = new World(session.Entities, session.Terrain);
            session.Game.World = snpsht;

            return session;
        }
    }
}
