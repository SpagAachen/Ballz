using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ballz.Menu
{
    public abstract class Leaf : Item
    {
        protected Leaf(string name, bool selectable = false) : base(name, selectable)
        {
            

        }

        public override IReadOnlyList<Item> Items => new List<Item>();

        public override Item SelectedItem => this;
    }
}
