using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace SimpleSidearms.rimworld
{
    class SidearmWeaponTagMapDef : Def
    {
        #pragma warning disable 0649
        public string sourceTag;

        public List<string> resultTags = new List<string>();
        #pragma warning restore 0649
    }
}
