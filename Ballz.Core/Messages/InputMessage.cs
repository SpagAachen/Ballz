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
            RawBack,
            RawInput
        }

        [JsonProperty(PropertyName = "Key")]
        public char Key{ get; private set;}

        [JsonProperty(PropertyName = "Pressed")]
        public bool Pressed{ get; private set;}

        [JsonIgnore]
        public Player Player { get; private set; }

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