using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ballz
{
	public class MenuRenderer : DrawableGameComponent
	{
		Texture2D TextureSplashScreen;
		SpriteBatch spriteBatch;
		public MenuRenderer (Game _game) : base(_game)
		{
		}

		protected override void LoadContent ()
		{
			spriteBatch = new SpriteBatch (Game.GraphicsDevice);
			TextureSplashScreen = Game.Content.Load<Texture2D>("Balls");
			base.LoadContent ();
		}

		protected override void UnloadContent ()
		{
			base.UnloadContent ();
		}

		public override void Draw (GameTime gameTime)
		{
			spriteBatch.Begin();
			spriteBatch.Draw(TextureSplashScreen, Game.Window.ClientBounds, Color.White);
			spriteBatch.End();
			base.Draw (gameTime);
		}
	}
}

