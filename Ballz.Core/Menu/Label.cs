using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ballz.Menu
{
    public class Label : Leaf
    {
        public Label(string name, bool selectable = false) : base(name, selectable)
        {
        }

        public override void HandleRawKey(char key)
        {
        }

        public override void HandleBackspace()
        {
        }
    }
}
