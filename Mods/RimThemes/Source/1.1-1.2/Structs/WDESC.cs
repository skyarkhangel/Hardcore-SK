using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace aRandomKiwi.RimThemes
{
    struct WDESC
    {
        public int type;
        public int wid;

        public WDESC(int type, int wid)
        {
            this.type = type;
            this.wid = wid;
        }
    };
}
