namespace Ballz.Renderer
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Menu;
    using Messages;

    public class MenuRenderer : DrawableGameComponent
	{
		Texture2D TextureSplashScreen;
		SpriteBatch spriteBatch;
		SpriteFont menuFont;
		
      GameMenu Menu;

		public MenuRenderer(Game _game) : base (_game)
		{
         Menu = GameMenu.Default;
		}

		protected override void LoadContent ()
		{
			spriteBatch = new SpriteBatch (Game.GraphicsDevice);
         //load a texture for the background
			TextureSplashScreen = Game.Content.Load<Texture2D> ("Textures/Balls");
         //load fonts for the menu
			menuFont = Game.Content.Load<SpriteFont> ("Fonts/Menufont");

			base.LoadContent ();
		}

		protected override void UnloadContent ()
		{
			base.UnloadContent ();
		}

		public void handleMessage (object _sender, Message _message)
		{
			if (_message.Kind == Message.MessageType.MenuMessage) 
			{
            MenuMessage msg = (MenuMessage)_message;
            Menu = msg.Value;
			}
         if (_message.Kind == Message.MessageType.LogicMessage) 
         {
            //todo check content of logicmessage as soon as it is implemented
            Enabled = !Enabled;
            Visible = !Visible;
         }
			//TODO: handle Messages
		}

		public override void Draw (GameTime gameTime)
		{
			spriteBatch.Begin ();
         //draw a backgroundscreen
			spriteBatch.Draw (TextureSplashScreen, Game.Window.ClientBounds, Color.White);

         //draw the MenuTitle
         spriteBatch.DrawString (menuFont, Menu.Name, new Vector2 (Game.Window.ClientBounds.Width/2 - menuFont.MeasureString(Menu.Name).X / 2 ,0), Color.Black);

         //draw other Menu Items
         float itemOffset = menuFont.MeasureString(Menu.Name).Y + 30 ;
         foreach (var item in Menu.Items) 
         {
            spriteBatch.DrawString (menuFont, item.Name, new Vector2 ( Game.Window.ClientBounds.Width/8 , itemOffset ),(Menu.SelectedItem != null && Menu.SelectedItem.Value == item) ? Color.Red : Color.Black);
            itemOffset += menuFont.MeasureString (item.Name).Y + 30;
         }

			spriteBatch.End ();
			base.Draw (gameTime);
		}
	}
}

