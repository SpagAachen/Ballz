using Ballz.GameSession.World;
using Ballz.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ballz.Sound;

namespace Ballz.GameSession.Logic.Weapons
{
    public class Bazooka: ChargedProjectileWeapon
    {
        public Bazooka(Ball ball, Ballz game) : base(ball, game) { }

        public override string Icon { get; } = "Bazooka";

        public override string Name { get; } = "Bazooka";
        
    }
}
