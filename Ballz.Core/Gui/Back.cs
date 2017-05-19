using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GeonBit.UI.Entities;

namespace Ballz.Gui
{
    public class BackButton : Button
    {
        public BackButton() : base("Back", ButtonSkin.Alternative)
        {
            OnClick += (e) => Ballz.The().Logic.MenuGoBack();
        }
    }
}
