using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;

namespace Ballz.Gui
{
    class MenuButton: Button
    {
        public MenuButton(string text, Action click = null, ButtonSkin skin = ButtonSkin.Default, Anchor anchor = Anchor.Auto, Vector2? size = default(Vector2?), Vector2? offset = default(Vector2?)) : base(text, skin, anchor, size, offset)
        {
            OnClick += (e) => click?.Invoke();
        }
        
        public void DoClick()
        {
            this.DoOnClick(null);
        }
    }
}
