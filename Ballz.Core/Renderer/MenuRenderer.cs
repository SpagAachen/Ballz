using Ballz.Menu;
using Ballz.Messages;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ballz.Renderer
{
    public class MenuRenderer : DrawableGameComponent
    {
        private GameMenu menu;
        private SpriteFont menuFont;
        private SpriteBatch spriteBatch;
        private Texture2D textureSplashScreen;

        public MenuRenderer(Game game) : base(game)
        {
            menu = GameMenu.Default;
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(Game.GraphicsDevice);
            
            // Load a texture for the background.
            textureSplashScreen = Game.Content.Load<Texture2D>("Textures/Balls");
            
            // Load fonts for the menu.
            menuFont = Game.Content.Load<SpriteFont>("Fonts/Menufont");

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
                menu = msg.Value;
            }
            if (message.Kind == Message.MessageType.LogicMessage)
            {
                //TODO: Check content of logicmessage as soon as it is implemented.
                Enabled = !Enabled;
                Visible = !Visible;
            }

            //TODO: handle Messages
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            
            // Draw a background screen.
            spriteBatch.Draw(textureSplashScreen, Game.Window.ClientBounds, Color.White);

            // Draw the MenuTitle.
            spriteBatch.DrawString(
                menuFont,
                menu.Name,
                new Vector2(Game.Window.ClientBounds.Width/2f - menuFont.MeasureString(menu.Name).X/2, 0),
                Color.Black);

            // Draw other Menu Items.
            var itemOffset = menuFont.MeasureString(menu.Name).Y + 30;
            foreach (var item in menu.Items)
            {
                spriteBatch.DrawString(
                    menuFont,
                    item.Name,
                    new Vector2(
                        Game.Window.ClientBounds.Width/8f,
                        itemOffset),
                    (menu.SelectedItem != null && menu.SelectedItem.Value == item) ? Color.Red : Color.Black);

                itemOffset += menuFont.MeasureString(item.Name).Y + 30;
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}