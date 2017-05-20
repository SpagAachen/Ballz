using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;


using GeonBit.UI;
using GeonBit.UI.Entities;

namespace Ballz.Gui
{
    public class MenuPanel : Panel
    {
        private int SelectedIndex;

        /// <summary>
        /// Gets or sets the background texture which is used as screen Background.
        /// </summary>
        /// <value>The background texture.</value>
        public Texture2D BackgroundTexture { get; set; }

        public MenuPanel(string name) : base(new Vector2(600, 0), PanelSkin.None, Anchor.Center)
        {
            //UserInterface.Root.PanelOverflowBehavior = PanelOverflowBehavior.VerticalScroll;
            //PanelOverflowBehavior = PanelOverflowBehavior.VerticalScroll;
            
            var header = new Header(name, Anchor.TopCenter);
            header.SetStyleProperty("Scale", new GeonBit.UI.DataTypes.StyleProperty(2.0f));
            AddChild(header);
        }

        public void AddItem(Entity item, int index = -1)
        {
            if (index >= 0)
                Members.Insert(index, item);
            else
                Members.Add(item);

            AddChild(item, index: index);
        }

        protected List<Entity> Members = new List<Entity>();

        public IReadOnlyList<Entity> Items => Members;

        public Entity SelectedItem => Members.Skip(SelectedIndex).FirstOrDefault();

        public void OnClose()
        {
            Close?.Invoke(this, new EventArgs());
            UserInterface.RemoveEntity(this);
        }

        public void OnOpen()
        {
            Open?.Invoke(this, new EventArgs());
            UserInterface.AddEntity(this);
        }

        public event EventHandler Close;
        public event EventHandler Open;

        public void SelectNext()
        {
            //if (Members.All(m => !m.Selectable))
            //    return;
            do
            {
                SelectedIndex = (SelectedIndex + 1) % Members.Count;
            }
            while (SelectedItem == null /*|| !SelectedItem.Selectable*/);
        }

        public void SelectPrevious()
        {
            //if (Members.All(m => !m.Selectable))
            //    return;
            do
            {
                SelectedIndex = (SelectedIndex + Members.Count - 1) % Members.Count;
            }
            while (SelectedItem == null /*|| !SelectedItem.Selectable*/);
        }

        public void SelectIndex(int i)
        {
            SelectedIndex = i;
        }

        public virtual void Update()
        {

        }

        protected static void OpenMenu(MenuPanel menu)
        {
            Ballz.The().Logic.OpenMenu(menu);
        }
    }
}
