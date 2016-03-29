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

    public class Ballerburg : SessionFactory
    {
        public override string Name { get; } = "Ballerburg";

        protected override void ImplInitializeSession(Ballz game, GameSession.Logic.GameSettings settings)
        {
            if (settings.MapTexture == null)
            { // Multiplayer clients will already have a map
                var texture = GenerateMountain();
                settings.MapName = "RandomGeneratedMap";
                settings.MapTexture = texture;
            }
        }

        protected override Session ImplStartSession(Ballz game, GameSession.Logic.GameSettings settings)
        {
            var session = new Session(game, new World(new Terrain(settings.MapTexture)), settings);

            // Create players and Ballz
            var ballzPositions = new List<Vector2> { new Vector2(1,5), new Vector2(35,5) };
            var currBallCreating = 0;
            foreach (var team in settings.Teams)
            {
                session.Players.Add(team.player);
                // Create ballz
                for (var i = 0; i < team.NumberOfBallz; ++i)
                {
                    var playerBall = new Ball
                    {
                        Position = ballzPositions[currBallCreating],
                        Velocity = new Vector2(0, 0),
                        IsAiming = true,
                            Player = team.player,
                        HoldingWeapon = "Bazooka",
                        IsStatic = true
                    };
                    ++currBallCreating;
                    session.World.AddEntity(playerBall);
                    if (team.ControlledByAI)
                        session.SessionLogic.ActiveControllers[team.player] = new AIControl(game, session, playerBall);
                    else
                        session.SessionLogic.ActiveControllers[team.player] = new UserControl(game, session, playerBall);
                }
            }

            var snpsht = new World(session.Terrain);
            session.Game.Match.World = snpsht;

            return session;
        }
        
        public static Texture2D GenerateMountain()
        {
            var width = 1920 / 4;
            var height = 1080 / 4;
            
            var rand = new Random();

            var heightmap = new float[width];

            var castleWidth = 400 / 4;
            var mountainHeight = 800 / 4;
            var leftHeight = 200 / 4;
            var rightHeight = 200 / 4;
            
            var i = 0; var start_i = 0; var end_i = 0;
            for (end_i = castleWidth, start_i = i; i < end_i; i++)
            {
                heightmap[i] = leftHeight;
            }

            for (end_i = width / 2, start_i = i; i < end_i; i++)
            {
                heightmap[i] = LinMap(i, start_i, end_i, leftHeight, mountainHeight);
            }

            for (end_i = width - castleWidth, start_i = i; i < end_i; i++)
            {
                heightmap[i] = LinMap(i, start_i, end_i, mountainHeight, rightHeight);
            }

            for (end_i = width, start_i = i; i < end_i; i++)
            {
                heightmap[i] = rightHeight;
            }

            Randomize(rand, heightmap, castleWidth, width / 2);
            Randomize(rand, heightmap, width / 2, width - castleWidth);
            
            Color[] pixels = new Color[width * height];

            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    pixels[x + (height - y - 1) * width] = heightmap[x] < y ? Color.Black : Color.White;
                }
            }

            var texture = new Texture2D(Ballz.The().GraphicsDevice, width, height, false, SurfaceFormat.Color);
            texture.SetData<Color>(pixels);
            return texture;
        }

        private static void Randomize(Random rand, float[] data, int left, int right)
        {
            if (right - 1 <= left)
            {
                return;
            }

            var mid = (right + left) / 2;
            var midHeight = (data[right] + data[left]) * 0.5f;

            var u1 = rand.NextDouble(); //these are uniform(0,1) random doubles
            var u2 = rand.NextDouble();
            var randNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)

            data[mid] = (float)(midHeight + (right - left) * 0.2 * randNormal);

            Randomize(rand, data, left, mid);
            Randomize(rand, data, mid, right);
        }

        static float LinMap(float a, float min1, float max1, float min2, float max2)
        {
            return ((a - min1) / (max1 - min1)) * (max2 - min2) + min2;
        }
    }
}
