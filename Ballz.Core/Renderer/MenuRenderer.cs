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
            switch(message.Kind)
            {
                case Message.MessageType.MenuMessage:
                    {
                        var msg = (MenuMessage)message;
                        parentMenu = Menu;
                        Menu = msg.Value;
                    }
                    break;
                case Message.MessageType.LogicMessage:
                    {
                        LogicMessage msg = (LogicMessage)message;
                        if (msg.Kind == LogicMessage.MessageType.GameMessage)
                        {
                            Enabled = !Enabled;
                            Visible = !Visible;
                        }
                    }
                    break;
            }

            //TODO: handle Messages
        }

        private float ComputeFadeProgress(GameTime gameTime)
        {
            FadeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            return Min(1.5f, FadeTimer / (FadeAnimationLength / 1.5f));
        }

        private void RenderInterpolatedMenu(float fadeProgress)
        {
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
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            DrawSky();

            var fadeProgress = ComputeFadeProgress(gameTime);

            SpriteBatch.Begin();

            RenderInterpolatedMenu(fadeProgress);

            SpriteBatch.End();

            if (Ballz.The().MessageOverlay != null)
            {
                DrawMessageOverlay(Ballz.The().MessageOverlay);
            }
        }

        const float TitleFontSize = 1f;
        const float ItemFontSize = 0.75f;

        float EaseOut(float t)
        {
            t = 1 - t;
            return 1 - (t * t * (3 * t - 2));
        }

        private void RenderMenuBackground(Item menu, float fadeProgress)
        {
            var background = (menu as Composite)?.BackgroundTexture;

            if (background != null)
                SpriteBatch.Draw(
                    background,
                    new Rectangle((GraphicsDevice.Viewport.Width - background.Width) / 2, (GraphicsDevice.Viewport.Height - background.Height) / 2, background.Width, background.Height),
                    new Color(Color.White, fadeProgress)
                    );
        }

        private void RenderMenuTitle(Item menu, float fadeProgress, float leftOffset, float topOffset)
        {
            float menuTopOffset = topOffset - (1 - EaseOut(fadeProgress)) * 150f;
            // Draw the MenuTitle.
            DrawText(
                menu.DisplayName,
                new Vector2(leftOffset, menuTopOffset),
                TitleFontSize,
                new Color(Color.Black, Sqrt(fadeProgress))
                );

            menuTopOffset += StringHeight(menu.DisplayName) * TitleFontSize;
            SpriteBatch.Draw(
                    Underline,
                    new Vector2(leftOffset - 20, menuTopOffset),
                    new Color(Color.White, fadeProgress)
                    );
        }

        private string DecorateInputBox(Item menu, Item item, bool showUnderscore)
        {
            string renderString;
            if (showUnderscore && item == menu.SelectedItem && item is InputBox)
                renderString = item.DisplayName + "_";
            else
                renderString = item.DisplayName;
            renderString = CheckLetters(renderString);
            return renderString;
        }

        private void RenderMenuSubItems(Item menu,float leftOffset, float topOffset, bool showUnderscore, float fadeProgress)
        {
            string renderString;
            foreach (var item in menu.Items)
            {
                leftOffset -= (1 - EaseOut(fadeProgress)) * 150f;
                if (item.Visible)
                {
                    renderString = DecorateInputBox(menu, item, showUnderscore);

                    DrawText(
                        renderString,
                        new Vector2(leftOffset, topOffset),
                        ItemFontSize,
                        new Color((menu.SelectedItem != null && menu.SelectedItem == item) ? Color.Red : Color.Black, Sqrt(fadeProgress)),
                        1
                        );

                    topOffset += StringHeight(renderString)* ItemFontSize * resolutionFactor + 30 * resolutionFactor;
                }
            }
        }

        private void RenderMenu(Item menu, bool showUnderscore, float fadeProgress)
        {
            RenderMenuBackground(menu, fadeProgress);

            // Make y margin the same as the x offset
            var topOffset = 40f * resolutionFactor;
            var leftOffset = 40f;

            // Draw the MenuTitle.
            RenderMenuTitle(menu, fadeProgress, leftOffset, topOffset);
            topOffset += StringHeight( menu.DisplayName) * TitleFontSize + 50;

            // Draw subMenu Items.
            RenderMenuSubItems(menu, leftOffset, topOffset, showUnderscore, fadeProgress);
        }
        
        private float StringHeight(string text)
        {
            return Font.MeasureString(CheckLetters(text)).Y;
        }

        private float StringWidth( string text)
        {
            return Font.MeasureString(CheckLetters(text)).X;
        }
    }
}