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
        public override string Name { get; } = "Worms";

        public override Session StartSession(Ballz game)
        {
            var session = new Session(game);

            session.Terrain = new Terrain(game.Content.Load<Texture2D>("Worlds/TestWorld2"));

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

            session.SessionLogic.AddPlayer(player1, player1Ball);

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

            session.SessionLogic.AddPlayer(player2, player2Ball);

            var npc = new Ball
            {
                Position = new Vector2(8, 10),
                Velocity = new Vector2(0, 0)
            };
            session.Entities.Add(npc);

            //System.Console.WriteLine("");

            var snpsht = new World(session.Entities, session.Terrain);
            session.Game.World = snpsht;

            return session;
        }
    }
}
