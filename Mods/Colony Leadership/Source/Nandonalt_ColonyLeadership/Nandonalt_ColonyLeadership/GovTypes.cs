using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Nandonalt_ColonyLeadership
{
    public class GovType
    {
        public string name;
        public string desc;
        public string nameMale;
        public string nameFemale;
    

        public GovType(string name, string desc, string nameMale, string nameFemale = "")
        {
            this.name = name;
            this.desc = desc;
            this.nameMale = nameMale;
            if (nameFemale == "") this.nameFemale = nameMale;
        }

    }
}
