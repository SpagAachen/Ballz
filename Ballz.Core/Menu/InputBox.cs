using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ballz.Menu
{
    public class InputBox : Leaf, IRawInputConsumer
    {
        private string code;

        public InputBox(string name, bool selectable = false) : base(name, selectable)
        {
        }
        
        public override void HandleRawKey(char key)
        {
            code += key;
        }

        public override void HandleBackspace()
        {
            if (code.Length > 0)
                code = code.Substring(0, code.Length - 1);
        }

        public override string DisplayName => Name + code;
    }
}
