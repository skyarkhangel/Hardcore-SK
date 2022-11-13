using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace aRandomKiwi.RimThemes
{
    struct FOI
    {
        public string field;
        public BindingFlags bf;

        public FOI(string field, BindingFlags bf = (BindingFlags.Public | BindingFlags.Static))
        {
            this.field = field;
            this.bf = bf;
        }
    };
}
