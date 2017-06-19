using System;
using Ballz.Messages;

using GeonBit.UI;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;

namespace Ballz
{
    public class MessageOverlay: Panel
    {
        Panel InnerPanel;
        public Header HeaderText;
        public Label MessageText;
        public Button CloseButton;

        public InputMessage.MessageType ButtonKey;

        public MessageOverlay(string header, string message = null, string buttonText = "OK", Action onButton = null, InputMessage.MessageType buttonKey = InputMessage.MessageType.ControlsAction) : base(new Vector2(1, 1), PanelSkin.Simple)
        {
            InnerPanel = new Panel(new Vector2(600, 400), PanelSkin.Default, Anchor.Center);
            FillColor = new Color(Color.Black, 0.5f);

            AddChild(InnerPanel);

            if (header != null)
            {
                HeaderText = new Header(header);
                InnerPanel.AddChild(HeaderText);
            }

            if(message != null)
            {
                MessageText = new Label(message, Anchor.Center);
                InnerPanel.AddChild(MessageText);
            }

            if (buttonText != null)
            {
                CloseButton = new Button(buttonText, anchor: Anchor.BottomCenter);
                CloseButton.OnClick += (e) =>
                {
                    Hide();
                    onButton?.Invoke();
                };
                InnerPanel.AddChild(CloseButton);
            }
        }

        public virtual void HandleInput(Messages.InputMessage input)
        {
            if (input.Pressed && input.Kind == ButtonKey)
                CloseButton.OnClick?.Invoke(CloseButton);
        }

        bool Hidden = false;
        public void Hide()
        {
            if (Hidden)
                return;
            Hidden = true;

            Ballz.The().MessageOverlay = null;
            UserInterface.RemoveEntity(this);
            this.Visible = false;
        }

        public static MessageOverlay Show(string header, string message = null, string buttonText = "OK", Action onButton = null, InputMessage.MessageType buttonKey = InputMessage.MessageType.ControlsAction)
        {
            if (Ballz.The().MessageOverlay != null)
                return null;

            MessageOverlay overlay = new MessageOverlay(header, message, buttonText, onButton, buttonKey);
            
            UserInterface.AddEntity(overlay);

            return overlay;
        }

        public static MessageOverlay ShowWaitMessage(string header = "Working...", string message = "Please wait.", bool canCancel = true, Action onCancel = null)
        {
            if (canCancel)
            {
                return Show(header, message, "Cancel", onCancel, InputMessage.MessageType.ControlsBack);
            }
            else
            {
                return Show(header, message, null, null, InputMessage.MessageType.None);
            }
        }

        public static MessageOverlay ShowError(string message)
        {
            var overlay = Show("Error", message);
            overlay.InnerPanel.FillColor = new Color(1.0f, 0.5f, 0.5f);
            return overlay;
        }
    }
}

