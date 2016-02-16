using Ballz.GameSession;
using Ballz.GameSession.Logic;
using Ballz.GameSession.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ballz.SessionFactory
{
    public class Worms : SessionFactory
    {
        public Worms(string mapName = "TestWorld2", bool usePlayerTurns = false)
        {
            MapName = mapName;
            UsePlayerTurns = usePlayerTurns;
        }

        public string MapName;
        public bool UsePlayerTurns;

        public override string Name { get { return "Worms (" + MapName + (UsePlayerTurns ? ", turn mode" : "") + ")"; } }

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

        public override Session StartSession(Ballz game, GameSession.Logic.GameSettings settings)
        {
            var session = new Session(game, settings);

            session.UsePlayerTurns = UsePlayerTurns;

            var mapTexture = game.Content.Load<Texture2D>("Worlds/" + MapName);
            session.Terrain = new Terrain(mapTexture);

            FindSpawnPoints(mapTexture, session.Terrain.Scale);
            var spawnPoints = SelectSpawnpoints(settings.Teams.Count);

            // Create players and Ballz
            var currBallCreating = 0;
            foreach (var team in settings.Teams)
            {
                session.Players.Add(team.player);
                // Create ballz
                for (var i = 0; i < team.NumberOfBallz; ++i)
                {
                    var playerBall = new Ball
                        {
                            Position = spawnPoints[currBallCreating],
                            Velocity = new Vector2(0, 0),
                            IsAiming = true,
                            Player = team.player,
                            HoldingWeapon = "Bazooka",
                            IsStatic = false
                        };
                    ++currBallCreating;
                    session.Entities.Add(playerBall);
                    if (team.ControlledByAI)
                        session.SessionLogic.BallControllers[team.player] = new AIControl(game, session, playerBall);
                    else
                        session.SessionLogic.BallControllers[team.player] = new UserControl(game, session, playerBall);
                }
            }

            var snpsht = new World(session.Entities, session.Terrain);
            session.Game.World = snpsht;

            return session;
        }
    }
}
