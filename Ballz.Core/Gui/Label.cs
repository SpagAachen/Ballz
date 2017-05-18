using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ballz.Gui
{
    public class Label : Leaf
    {
        public Label(string name, bool selectable = false, float fontSize = 0.75f) : base(name, selectable)
        {
            FontSize = fontSize;
        }
    }
}
