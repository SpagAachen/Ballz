using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ballz.Gui
{
    public class InputBox : Leaf, IRawInputConsumer
    {
        public string Value;
        public Settings.Setting<string> Setting;
        public bool internalValue = true;

        public InputBox(string name, bool selectable = false) : base(name, selectable)
        {
            BackGroundColor = new Microsoft.Xna.Framework.Color(Microsoft.Xna.Framework.Color.Black, 127);
            BorderColor = new Microsoft.Xna.Framework.Color(Microsoft.Xna.Framework.Color.Gray,191);
            BorderWidth = 3;
        }
        
        public void HandleRawKey(char key)
        {
            if(!char.IsControl(key))
            {
                if(internalValue)
                    Value += key;
                else
                    Setting.Value += key;    
            }
        }

        public void HandleBackspace()
        {
            if(internalValue)
            {
            if (Value.Length > 0)            
                Value = Value.Substring(0, Value.Length - 1);
            }
            else
            {
                if (Setting.Value.Length > 0)            
                    Setting.Value = Setting.Value.Substring(0, Setting.Value.Length - 1);
            }
        }

        public override string DisplayName => internalValue ? Name + Value : Name + Setting.Value;
    }
}
