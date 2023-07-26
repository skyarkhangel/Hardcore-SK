using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
using Verse;
using static UnityEngine.Scripting.GarbageCollector;

namespace ModIndicator
{
    public class ModListerSettingsDef : Def
    {
        public List<ModIndicator> modIndicators;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            //mod = ParseHelper.FromString<string>(xmlRoot.FirstChild.Value);
            //DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "modTypeDef", xmlRoot.Name);
            //Log.Message("passed DirectXmlCrossRefLoader");

            modIndicators = new List<ModIndicator>();
            this.defName = xmlRoot.SelectSingleNode("/Defs/ModIndicator.ModListerSettingsDef/defName").FirstChild.Value;

            foreach (XmlNode indicators in xmlRoot.SelectNodes("/Defs/ModIndicator.ModListerSettingsDef/modIndicators"))
            {
                //Log.Message("Checking1: " + item);
                foreach (XmlNode modType in indicators)
                {
                    //Log.Message("Checking1: " + item2.Name);
                    foreach (XmlNode listItem in modType.ChildNodes)
                    {
                        ModIndicator tmp = new ModIndicator();
                        //Log.Message("Checking2: " + item3.Name);
                        foreach (XmlNode info in listItem)
                        {
                            //Log.Message("Checking3: " + info.Name);
                            if (info.Name == "id")
                            {
                                //Log.Message("mod Value is: " + info.FirstChild.Value);
                                tmp.mod = ParseHelper.FromString<string>(info.FirstChild.Value);
                            }

                            if (info.Name == "link")
                            {
                                //Log.Message("link Value is: " + info.FirstChild.Value);
                                tmp.link = ParseHelper.FromString<string>(info.FirstChild.Value);
                            }

                            if (info.Name == "requiredMods")
                            {
                                tmp.requiredMods = new List<string>();
                                foreach (XmlNode requiredMod in info)
                                {
                                    tmp.requiredMods.Add(ParseHelper.FromString<string>(requiredMod.FirstChild.Value));
                                }
                                //Log.Message("link Value is: " + info.FirstChild.Value);
                            }
                        }
                        modIndicators.Add(tmp);
                        //Log.Message("modIndicator Added");
                        DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(tmp, "modTypeDef", modType.Name);
                        //Log.Message("Checking4: DirectXmlCrossRefLoader: modTypeDef - " + item2.Name + " assigned");
                    }
                }
            }
            //DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "modIndicators", this.defName);
            //Log.Message("Checking5: Exit");
            /*
            foreach (XmlNode item in xmlRoot.ChildNodes)
            {
                Log.Message("Checking: " + item.ToString());
                foreach (XmlNode node in item.ChildNodes)
                {
                    Log.Message("Checking: " + node.ToString());
                    if (node.Name == "id")
                        mod = node.Name;
                    else if (node.Name == "link")
                        link = node.Name;
                }
            }*/
        }
    }
}
