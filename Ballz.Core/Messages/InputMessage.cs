using Ballz.GameSession.Logic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Ballz.Messages
{
    [Serializable]
    public class InputMessage : Message
    {
        public new enum MessageType
        {
            ControlsConsole,
            ControlsUp,
            ControlsDown,
            ControlsLeft,
            ControlsRight,
            ControlsAction,
            ControlsJump,
            ControlsBack,
            ControlsNextWeapon,
            ControlsPreviousWeapon,
			ControlsCameraZoomIn,
			ControlsCameraZoomOut,
			ControlsCameraModeToggle,
            RawBack,
            RawInput,
            None
        }

        [JsonProperty(PropertyName = "Key")]
        public char Key{ get; set;}

        [JsonProperty(PropertyName = "Pressed")]
        public bool Pressed{ get; set;}

        [JsonIgnore]
        public Player Player { get; set; }

        public InputMessage() { }

        public InputMessage(MessageType type, bool pressed, char value, Player player) : base(Message.MessageType.InputMessage)
        {
            Kind = type;
            Key = value;
            Pressed = pressed;
            Player = player;
        }

        [JsonProperty("InputMessageKind")]
        [JsonConverter(typeof(StringEnumConverter))]
        public new MessageType Kind { get; private set; }
    }
}