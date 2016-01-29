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
        public Worms(string mapName = "TestWorld2")
        {
            MapName = mapName;
        }

        public string MapName;

        public override string Name { get { return "Worms (" + MapName + ")"; } }

        public override Session StartSession(Ballz game)
        {
            var session = new Session(game);

            session.Terrain = new Terrain(game.Content.Load<Texture2D>("Worlds/" + MapName));

            var player1 = new Player
            {
                Name = "Player1"
            };

            session.Players.Add(player1);

            var player1Ball = new Ball
            {
                Position = new Vector2(4, 10),
                Velocity = new Vector2(2, 0),
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
                Position = new Vector2(27, 7),
                Velocity = new Vector2(2, 0),
                IsAiming = true,
                Player = player2,
                HoldingWeapon = "HandGun",
            };
            session.Entities.Add(player2Ball);

            session.SessionLogic.BallControllers[player2] = new UserControl(game, session, player2Ball);

            //var playerAi = new Player
            //{
            //    Name = "NPC"
            //};
            //session.Players.Add(playerAi);

            //var aiBall = new Ball
            //{
            //    Position = new Vector2(30, 20),
            //    Velocity = new Vector2(0, 0),
            //    Player = playerAi
            //};
            //session.Entities.Add(aiBall);
            //session.SessionLogic.BallControllers[playerAi] = new AIControl(game, session, aiBall);

            var snpsht = new World(session.Entities, session.Terrain);
            session.Game.World = snpsht;

            return session;
        }
    }
}
