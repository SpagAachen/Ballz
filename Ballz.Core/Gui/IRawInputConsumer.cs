using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ballz.Gui
{
    interface IRawInputConsumer
    {
        void HandleRawKey(char key);

        void HandleBackspace();
    }
}
