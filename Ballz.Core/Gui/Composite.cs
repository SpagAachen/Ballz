using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ballz.Gui
{
    public class Composite : Item
    {
        protected readonly List<Item> Members = new List<Item>();
        private int SelectedIndex; 

        /// <summary>
        /// Gets or sets the background texture which is used as screen Background.
        /// </summary>
        /// <value>The background texture.</value>
        public Texture2D BackgroundTexture { get; set; }

        public Composite(string name, bool selectable = false) : base(name, selectable)
        {
        }

        public void AddItem(Item item, int index = -1)
        {
            if (index >= 0)
                Members.Insert(index, item);
            else
                Members.Add(item);
        }

        public override IReadOnlyList<Item> Items => Members;

        public override Item SelectedItem => Members.Where(i => i.Selectable).Skip(SelectedIndex).FirstOrDefault();

        public void SelectNext()
        {
            if (Members.All(m => !m.Selectable))
                return;
            do
            {
                SelectedIndex = (SelectedIndex + 1) % Members.Count;
            }
            while (SelectedItem == null || !SelectedItem.Selectable);
        }

        public void SelectPrevious()
        {
            if (Members.All(m => !m.Selectable))
                return;
            do
            {
                SelectedIndex = (SelectedIndex + Members.Count - 1) % Members.Count;
            }
            while (SelectedItem == null || !SelectedItem.Selectable);
        }

        public void SelectIndex(int i)
        {
            SelectedIndex = i;
        }
    }
}
