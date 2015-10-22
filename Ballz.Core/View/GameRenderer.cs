using System;
using Microsoft.Xna.Framework;

namespace Ballz
{
	/// <summary>
	/// Render system performs all rendering of the Game and is inteded as a module.
	/// </summary>
	public class GameRenderer : DrawableGameComponent
	{
		public GameRenderer (Game _game) : base(_game)
		{

		}

		/// <summary>
		/// Draw the game for the specified _time.
		/// </summary>
		/// <param name="_time">time since start of game (cf BallzGame draw).</param>
		public void draw(GameTime _time)
		{
		}

		public void handleMessage(object _sender, Message _message)
		{
			throw new NotImplementedException();
		}
	}
}

