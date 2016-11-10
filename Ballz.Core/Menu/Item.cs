using System.Collections.Generic;
using System.Linq;

namespace Ballz.Menu
{
    public abstract class Item
    {
        public bool Visible { get; set; } = true;

        public bool Active{ get; protected set;}

        //TODO: if necessary use an event for this.
        public bool ActiveChanged{ get; protected set;}

        protected Item(string name, bool selectable = false)
        {
            Name = name;
            Selectable = selectable;
            Active = false;
        }

        //The name
        public string Name { get; set; }

        public abstract IReadOnlyList<Item> Items { get; }

        public IEnumerable<Item> Descendants
        {
            get
            {
                foreach (var i in Items)
                {
                    yield return i;

                    foreach (var d in i.Descendants)
                        yield return d;
                }
            }
        }

        public delegate void ItemHandler<in T>(T item) 
            where T : Item;

        public void BindSelectHandler<T>(ItemHandler<T> handler) where T : Item
        {
            foreach (var c in Descendants.OfType<T>())
                c.OnSelect += () => handler.Invoke(c);
        }

        public void BindUnSelectHandler<T>(ItemHandler<T> handler) where T : Item
        {
            foreach (var c in Descendants.OfType<T>())
                c.OnUnSelect += () => handler.Invoke(c);
        }

        public abstract Item SelectedItem { get; }

        public bool Selectable { get; set; }

        public virtual string DisplayName => Name;

        public delegate void SelectHandler();

        public delegate void UnSelectHandler();

        public event SelectHandler OnSelect;

        public event UnSelectHandler OnUnSelect;

        public void Activate()
        {
            OnSelect?.Invoke();
        }

        public void DeActivate()
        {
            OnUnSelect?.Invoke();
        }

        /// <summary>
        /// Gets or sets the width (only used when backgroundColor or border is set).
        /// </summary>
        /// <value>The width.</value>
        public int Width{ get; set;}

        /// <summary>
        /// Gets or sets the height (only used when backgroundColor or border is set).
        /// </summary>
        /// <value>The height.</value>
        public int Height{ get; set;}

        public int BorderWidth{ get; set;}      

        public Microsoft.Xna.Framework.Color BackGroundColor{ get; set;}

        public Microsoft.Xna.Framework.Color BorderColor{ get; set;}
    }
}