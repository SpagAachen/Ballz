using Ballz.Gui;
using Ballz.Messages;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GeonBit.UI;
using GeonBit.UI.Entities;
using System;

using static MathFloat.MathF;

namespace Ballz.Renderer
{
    public class GuiRenderer : BaseRenderer
    {
        
        float FadeTimer = 0f;
        float FadeAnimationLength = 0.75f;

        Texture2D Underline;

        public GuiRenderer(Ballz game) : base(game)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            UserInterface.Initialize(Ballz.The().Content, BuiltinThemes.hd);
            UserInterface.UseRenderTarget = true;
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
        
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            UserInterface.Update(gameTime);
            UserInterface.Draw(SpriteBatch);
            Game.GraphicsDevice.Clear(Color.CornflowerBlue);

            DrawSky();
            
            UserInterface.DrawMainRenderTarget(SpriteBatch);

            if (Ballz.The().MessageOverlay != null)
            {
                DrawMessageOverlay(Ballz.The().MessageOverlay);
            }
        }

        const float TitleFontSize = 1f;

        float EaseOut(float t)
        {
            t = 1 - t;
            return 1 - (t * t * (3 * t - 2));
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