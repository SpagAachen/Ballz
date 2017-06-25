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
    using System.Diagnostics;

    public class Worms : SessionFactory
    {

        public Worms(string mapName = "Beach", bool usePlayerTurns = false)
        {
            MapName = mapName;
            UsePlayerTurns = usePlayerTurns;
        }

        private int width;
        private int height;
        public Worms(int width, int height, bool usePlayerTurns = false)
        {
            this.width = width;
            this.height = height;
            MapName = "Generated";
            UsePlayerTurns = usePlayerTurns;
        }

        public string MapName;
        public bool UsePlayerTurns;

        public override string Name { get { return "Worms (" + MapName + (UsePlayerTurns ? ", turn mode" : "") + ")"; } }

        public List<Vector2> SpawnPoints = new List<Vector2>();

        bool HasGravityPoint = false;
        Vector2 GravityPoint = Vector2.Zero;

        public void FindSpecialMarkers(Texture2D map, float terrainScale)
        {
            var w = map.Width;
            var h = map.Height;
            Color[] pixels = new Color[w * h];
            map.GetData<Color>(pixels);

            // Spawn points are identified by green pixels in the map
            var spawnPointColor = new Color(1f, 0f, 0f);
            var gravityPointColor = new Color(0f, 1f, 1f);

            for (int x = 0; x < w; x++)
            {
                for(int y = 0; y < h; y++)
                {
                    Color c = pixels[y * w + x];
                    if(c == spawnPointColor)
                        SpawnPoints.Add(new Vector2(x * terrainScale, (h - y) * terrainScale));
                    else if(c == gravityPointColor)
                    {
                        HasGravityPoint = true;
                        GravityPoint = new Vector2(x * terrainScale, (h - y) * terrainScale);
                    }
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

        protected override void ImplInitializeSession(Ballz game, MatchSettings settings)
        {
            var mapTexture = MapName == "Generated" ? TerrainGenerator.GenerateTerrain(width,height): game.Content.Load<Texture2D>("Worlds/" + MapName);
            settings.MapName = MapName;
            settings.MapTexture = mapTexture;
        }

        protected override Session ImplStartSession(Ballz game, MatchSettings settings, bool isMultiplayer, int localPlayerId)
        {
            var session = new Session(game, new World(new Terrain(settings.MapTexture)), settings)
                              {
                                  UsePlayerTurns = this.UsePlayerTurns
                              };

            FindSpecialMarkers(settings.MapTexture, session.Terrain.Scale);

            var spawnPoints = SelectSpawnpoints(settings.Teams.Select(t=>t.NumberOfBallz).Sum());

            session.Terrain.GravityPoint = GravityPoint;
            session.Terrain.HasGravityPoint = HasGravityPoint;

            // Create players and Ballz
            var currBallCreating = 0;
            foreach (var team in settings.Teams)
            {
                var player = new Player
                {
                    Id = team.Id,
                    Name = team.Name,
                    TeamName = team.Country,
                    IsLocal = !isMultiplayer || localPlayerId == team.Id
                };
                
                session.Players.Add(player);

                var ballCount = UsePlayerTurns ? team.NumberOfBallz : 1;
                var ballNames = TeamNames.GetBallNames(team.Country, ballCount);

                for (var i = 0; i < ballCount; ++i)
                {
                    var playerBall = new Ball
                    {
                        Name = ballNames[i],
                        Position = spawnPoints[currBallCreating],
                        Velocity = new Vector2(0, 0),
                        IsAiming = true,
                        Player = player,
                        HoldingWeapon = "Bazooka",
                        IsStatic = false
                    };
                    player.OwnedBalls.Add(playerBall);
                    ++currBallCreating;
                    session.World.AddEntity(playerBall);

                    BallControl controller;

                    if (team.ControlledByAI)
                        controller = new AIControl(game, session, playerBall);
                    else
                        controller = new UserControl(game, session, playerBall);

                    session.SessionLogic.BallControllers[playerBall] = controller;

                }

                player.ActiveBall = player.OwnedBalls.FirstOrDefault();
                session.SessionLogic.ActiveControllers[player] = session.SessionLogic.BallControllers[player.ActiveBall];
            }

            return session;
        }
    }
}
