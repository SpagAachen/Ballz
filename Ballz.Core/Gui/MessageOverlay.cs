using System;
using Ballz.Messages;

namespace Ballz
{
    public class MessageOverlay
    {
        public string HeaderText;
        public string MessageText;
        public string FooterText;

        public Action OnEnter = () => { Ballz.The().MessageOverlay = null; };

        public virtual void HandleInput(Messages.InputMessage input)
        {
            if (input.Pressed && input.Kind == InputMessage.MessageType.ControlsAction)
                OnEnter?.Invoke();
        }

        public static void ShowAlert(string header, string message = null, string footer = "Enter to continue", Action onEnter = null)
        {
            if (Ballz.The().MessageOverlay != null)
                return;

            MessageOverlay overlay = new MessageOverlay
            {
                HeaderText = header,
                MessageText = message,
                FooterText = footer
            };

            if (onEnter != null)
                overlay.OnEnter = onEnter;
            
            Ballz.The().MessageOverlay = overlay;
        }
    }
}

