using System.Collections.Generic;
using System.Linq;

namespace Ballz.Menu
{
    public abstract class Item
    {
        protected Item(string name, bool selectable = false)
        {
            Name = name;
            Selectable = selectable;
        }

        //The name
        public string Name { get; }

        //TODO: this is not very nice :(
        public static Item Default
        {
            get
            {
                // main menu
                var mainMenu = new Composite("Main Menu");

                var play = new Label("Play",true);

                mainMenu.AddItem(play);
                mainMenu.AddItem(new Label("Options",true));
                mainMenu.AddItem(new Label("Multiplayer",true));

                var quit = new Label("Quit", true);               
                mainMenu.AddItem(quit);

                return mainMenu;
            }
        }

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

        public void BindCompositeHandler(ItemHandler<Composite> handler)
        {
            foreach (var c in Descendants.OfType<Composite>())
                c.OnSelect += () => handler.Invoke(c);
        }
        public void BindBackHandler(ItemHandler<Back> handler)
        {
            foreach (var c in Descendants.OfType<Back>())
                c.OnSelect += () => handler.Invoke(c);
        }
        public void BindInputBoxHandler(ItemHandler<InputBox> handler)
        {
            foreach (var c in Descendants.OfType<InputBox>())
                c.OnSelect += () => handler.Invoke(c);
        }

        public abstract Item SelectedItem { get; }

        public bool Selectable { get; }

        public virtual string DisplayName => Name;

        public abstract void HandleRawKey(char key);

        public abstract void HandleBackspace();

        public delegate void SelectHandler();

        public event SelectHandler OnSelect;

        public void Activate()
        {
            OnSelect?.Invoke();
        }
    }
}