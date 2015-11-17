using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ballz.Menu
{
    public class Composite : Item
    {
        private readonly List<Item> members = new List<Item>();
        private int index; 

        public Composite(string name, bool selectable = false) : base(name, selectable)
        {
        }

        public void AddItem(Item item)
        {
            members.Add(item);
        }

        public override IReadOnlyList<Item> Items => members;
        public override Item SelectedItem => members[index].Selectable ? members[index] : null;

        public override void HandleRawKey(char key)
        {
        }

        public override void HandleBackspace()
        {
        }

        public void SelectNext()
        {
            if (members.All(m => !m.Selectable))
                return;
            do
            {
                index = (index + 1) % members.Count;
            }
            while (!SelectedItem.Selectable);
        }

        public void SelectPrevious()
        {
            if (members.All(m => !m.Selectable))
                return;
            do
            {
                index = (index + members.Count - 1) % members.Count;
            }
            while (!SelectedItem.Selectable);
        }
    }
}
