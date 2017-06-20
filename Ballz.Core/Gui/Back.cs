using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;

namespace Ballz.Gui
{
    public class BackButton : Button
    {
        public BackButton(Anchor anchor = Anchor.Auto, Vector2? size = null, string text = "Back") : base(text, ButtonSkin.Alternative, anchor, size)
        {
            OnClick += (e) => Ballz.The().Logic.MenuGoBack();
        }
    }
}
