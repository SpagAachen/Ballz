using Ballz.Menu;
using Ballz.Messages;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Ballz.Renderer
{
    public class MenuRenderer : BaseRenderer
    {
        public Item Menu { get; set; }
        private Item parentMenu;

        public MenuRenderer(Ballz game, Item defaultMenu = null) : base(game)
        {
            Menu = defaultMenu;
        }
        
        protected override void UnloadContent()
        {
            base.UnloadContent();
        }

        public void HandleMessage(object sender, Message message)
        {
            if (message.Kind == Message.MessageType.MenuMessage)
            {
                var msg = (MenuMessage)message;
                parentMenu = Menu;
                Menu = msg.Value;
            }

            if (message.Kind == Message.MessageType.LogicMessage)
            {
                LogicMessage msg = (LogicMessage)message;
                if (msg.Kind == LogicMessage.MessageType.GameMessage)
                {
                    Enabled = !Enabled;
                    Visible = !Visible;
                }
            }

            //TODO: handle Messages
        }

        public override void Draw(GameTime gameTime)
        {
            DrawSky();

            SpriteBatch.Begin();
            
            if (Menu != null)
            {
                if (Menu.Items.Count > 0)
                {
                    RenderMenu(Menu, false);
                }
                else
                {
                    RenderMenu(parentMenu, true);
                }
            }

            SpriteBatch.End();
            base.Draw(gameTime);
        }

        const float TitleFontSize = 1f;
        const float ItemFontSize = 0.5f;

        private void RenderMenu(Item menu, bool showUnderscore)
        {
            var background = (menu as Composite)?.BackgroundTexture;

            if (background != null)
                SpriteBatch.Draw(background, new Rectangle((GraphicsDevice.Viewport.Width - background.Width) / 2, (GraphicsDevice.Viewport.Height - background.Height) / 2, background.Width, background.Height), Color.White);

            // Make y margin the same as the x offset
            var topOffset = 40f;
            var leftOffset = 40f;
            // Draw the MenuTitle.
            DrawText(
                menu.DisplayName,
                new Vector2(leftOffset, topOffset),
                TitleFontSize,
                Color.Black                
                );

            // Draw subMenu Items.
            topOffset += Font.MeasureString(menu.DisplayName).Y * TitleFontSize + 30;
            string renderString;
            foreach (var item in menu.Items)
            {
                if (item.Visible)
                {
                    if (showUnderscore && item == menu.SelectedItem && item is InputBox)
                        renderString = item.DisplayName + "_";
                    else
                        renderString = item.DisplayName;
                    renderString = CheckLetters(renderString);

                    DrawText(
                        renderString,
                        new Vector2(leftOffset, topOffset),
                        ItemFontSize,
                        (menu.SelectedItem != null && menu.SelectedItem == item) ? Color.Red : Color.Black
                        );

                    topOffset += Font.MeasureString(renderString).Y * ItemFontSize + 30;
                }
            }
        }

        private string CheckLetters(string toCheck)
        {
            char[] letters = toCheck.ToCharArray();
            string checkedString = toCheck;
            foreach (char letter in letters)
            {
                if (!Font.Characters.Contains(letter))
                    checkedString = checkedString.Replace(letter, '?');
            }

            return checkedString;
        }
    }
}