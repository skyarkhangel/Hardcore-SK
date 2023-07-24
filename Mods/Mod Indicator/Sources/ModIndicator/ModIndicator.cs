using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
using Verse;

namespace ModIndicator
{
    public class ModIndicator
    {
        public string mod;

        public ModTypeDef modTypeDef;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            mod = ParseHelper.FromString<string>(xmlRoot.Name);
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "modTypeDef", xmlRoot.FirstChild.Value);
        }
    }
}
