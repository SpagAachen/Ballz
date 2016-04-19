using Ballz.Menu;
using Ballz.Messages;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

using static MathFloat.MathF;

namespace Ballz.Renderer
{
    public class MenuRenderer : BaseRenderer
    {
        Item _Menu;
        Item OldMenu;
        public Item Menu
        {
            get
            {
                return _Menu;
            }
            set
            {
                if (_Menu != value)
                {
                    OldMenu = _Menu;
                    FadeTimer = 0f;
                    _Menu = value;
                }
            }
        }

        private Item parentMenu;

        float FadeTimer = 0f;
        float FadeAnimationLength = 0.75f;

        Texture2D Underline;

        public MenuRenderer(Ballz game, Item defaultMenu = null) : base(game)
        {
            Menu = defaultMenu;
        }

        protected override void LoadContent()
        {
            Underline = Game.Content.Load<Texture2D>("Textures/Underline");
            base.LoadContent();
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

            FadeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            var fadeProgress = Min(1.5f, FadeTimer / (FadeAnimationLength / 1.5f));

            SpriteBatch.Begin();

            if (fadeProgress <= 1f && OldMenu != null && OldMenu.Items.Count > 0)
            {
                RenderMenu(OldMenu, false, 1f - fadeProgress);
            }
            else
                OldMenu = null;

            if (fadeProgress > 0.5f && Menu != null)
            {
                if (Menu.Items.Count > 0)
                {
                    RenderMenu(Menu, false, fadeProgress - 0.5f);
                }
                else
                {
                    RenderMenu(parentMenu, true, fadeProgress - 0.5f);
                }
            }

            SpriteBatch.End();
            base.Draw(gameTime);
        }

        const float TitleFontSize = 1f;
        const float ItemFontSize = 0.75f;

        float EaseOut(float t)
        {
            t = 1 - t;
            return 1 - (t * t * (3 * t - 2));
        }

        private void RenderMenu(Item menu, bool showUnderscore, float fadeProgress)
        {
            var background = (menu as Composite)?.BackgroundTexture;

            if (background != null)
                SpriteBatch.Draw(
                    background,
                    new Rectangle((GraphicsDevice.Viewport.Width - background.Width) / 2, (GraphicsDevice.Viewport.Height - background.Height) / 2, background.Width, background.Height),
                    new Color(Color.White, fadeProgress)
                    );

            // Make y margin the same as the x offset
            var topOffset = 40f * resolutionFactor;
            var leftOffset = 40f;

            var menuTopOffset = topOffset - (1 - EaseOut(fadeProgress)) * 150f;

            // Draw the MenuTitle.
            DrawText(
                menu.DisplayName,
                new Vector2(leftOffset, menuTopOffset),
                TitleFontSize,
                new Color(Color.Black, Sqrt(fadeProgress))
                );

            menuTopOffset += Font.MeasureString(menu.DisplayName).Y * TitleFontSize;
            SpriteBatch.Draw(
                    Underline,
                    new Vector2(leftOffset - 20, menuTopOffset),
                    new Color(Color.White, fadeProgress)
                    );

            // Draw subMenu Items.

            topOffset += Font.MeasureString(menu.DisplayName).Y * TitleFontSize + 50;
            string renderString;
            foreach (var item in menu.Items)
            {
                leftOffset -= (1 - EaseOut(fadeProgress)) * 150f;
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
                        new Color((menu.SelectedItem != null && menu.SelectedItem == item) ? Color.Red : Color.Black, Sqrt(fadeProgress)),
                        1
                        );

                    topOffset += Font.MeasureString(renderString).Y * ItemFontSize*resolutionFactor + 30 * resolutionFactor;
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