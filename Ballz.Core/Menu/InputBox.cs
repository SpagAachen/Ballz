using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ballz.Menu
{
    public class InputBox : Leaf, IRawInputConsumer
    {
        public string Value;

        public InputBox(string name, bool selectable = false) : base(name, selectable)
        {
        }
        
        public void HandleRawKey(char key)
        {
            if(!Char.IsControl(key))
                Value += key;
        }

        public void HandleBackspace()
        {
            if (Value.Length > 0)
                Value = Value.Substring(0, Value.Length - 1);
        }

        public override string DisplayName => Name + Value;
    }
}
