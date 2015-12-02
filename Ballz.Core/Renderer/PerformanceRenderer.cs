//
//  PerformanceRenderer.cs
//
//  Author:
//       Martin <Martin.Schultz@RWTH-Aachen.de>
//
//  Copyright (c) 2015 SPAG
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Diagnostics;
using Ballz.Messages;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ballz
{
    public class PerformanceRenderer : DrawableGameComponent
    {
        private SpriteFont font;   
        private SpriteBatch renderer;
        private int fpsCounter = 0;
        private int fps = 0;
        private double seconds = 0;

        private float allocatedMemory;

        private Vector2 fpsPosition = new Vector2(20,20);
        private Vector2 frametimePosition = new Vector2(20,40);
        private Vector2 memoryPosition = new Vector2(20,60);

        public PerformanceRenderer(Ballz game) : base(game)
        {
            Enabled = false;
            Visible = false;
            //draw the overlay last, assuming we have game and menu for the very first draw calls
            DrawOrder = 3;
        }

        public override void Initialize()
        {
            Game.Services.GetService<Logic.LogicControl>().Message += handleMessage;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            //for now use the Menu font for our performance overlay
            font = Game.Content.Load<SpriteFont>("Fonts/Menufont");
            renderer = new SpriteBatch(Game.GraphicsDevice);
            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            if (gameTime.TotalGameTime.TotalSeconds > (seconds+1.0))
            {
                fps = fpsCounter;
                fpsCounter = 0;
                seconds = gameTime.TotalGameTime.TotalSeconds;
                allocatedMemory = Process.GetCurrentProcess().PrivateMemorySize64 / 1024f / 1024f;
            }
            else
                ++fpsCounter;
            
            renderer.Begin();
            renderer.DrawString(font, "fps: " + fps, fpsPosition, Color.DarkSlateGray, 0, -2*Vector2.One,0.4f,SpriteEffects.None,0);
            renderer.DrawString(font, "fps: " + fps, fpsPosition, Color.Wheat, 0, Vector2.Zero,0.4f,SpriteEffects.None,0);

            renderer.DrawString(font, "frametime: " + gameTime.ElapsedGameTime.TotalSeconds*1000 + " ms", frametimePosition, Color.DarkSlateGray, 0, -2*Vector2.One,0.4f,SpriteEffects.None,0);
            renderer.DrawString(font, "frametime: " + gameTime.ElapsedGameTime.TotalSeconds*1000 + " ms", frametimePosition, Color.Wheat, 0, Vector2.Zero,0.4f,SpriteEffects.None,0);

            renderer.DrawString(font, "Memory: " + allocatedMemory + " MB", memoryPosition, Color.DarkSlateGray, 0, -2*Vector2.One,0.4f,SpriteEffects.None,0);
            renderer.DrawString(font, "Memory: " + allocatedMemory + " MB", memoryPosition, Color.Wheat, 0, Vector2.Zero,0.4f,SpriteEffects.None,0);

            renderer.End();

            base.Draw(gameTime);
        }

        protected override void OnDrawOrderChanged(object sender, EventArgs args)
        {
            base.OnDrawOrderChanged(sender, args);
        }

        public void handleMessage(object sender, Message msg)
        {
            if (msg.Kind == Message.MessageType.LogicMessage)
            {
                if (((LogicMessage)msg).Kind == LogicMessage.MessageType.PerformanceMessage)
                {
                    Visible = !Visible;
                }
            }
                
        }
    }
}

