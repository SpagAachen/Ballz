using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GeonBit.UI;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;

namespace Ballz.GameSession.Gui
{
    public class WeaponSelect : Panel
    {
        public WeaponSelect() : base(new Vector2(250, 50), anchor: Anchor.Center)
        {
            AddChild(new Button("Gun"));
        }
    }
}
