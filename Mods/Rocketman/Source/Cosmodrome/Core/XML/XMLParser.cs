using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using HarmonyLib;
using Verse;

namespace RocketMan
{
    public static class XMLParser
    {
        public static string rocketRulesFolder = "Extras";

        public static void ParseXML()
        {
            Log.Message("ROCKETMAN: XMLParser started");
            foreach (ModContentPack mod in LoadedModManager.RunningModsListForReading)
            {
                LoadableXmlAsset[] assets = DirectXmlLoader.XmlAssetsInModFolder(mod, rocketRulesFolder);
                foreach (var ass in assets)
                {
                    if (!ass.name.ToLower().StartsWith("rocket") || !ass.name.ToLower().EndsWith(".xml")) continue;
                    foreach (var element in ass.xmlDoc["RocketRules"].OfType<XmlElement>())
                        ProcessRocketRuleData(element);
                }
            }
        }

        private static void ProcessRocketRuleData(XmlElement node)
        {
            if (node.Name == "IgnoreMe")
            {
                if (node.HasAttribute("defname"))
                {
                    IgnoreMeDatabase.Add(node.GetAttribute("defname"));
                    return;
                }
                if (node.HasAttribute("packageId"))
                {
                    IgnoreMeDatabase.AddPackageId(node.GetAttribute("packageId"));
                    return;
                }
            }            
            else if (node.Name == "Incompatibility")
            {
                if (!node.HasAttribute("packageId"))
                    return;
                if (!node.HasAttribute("name"))
                    return;
                string name = node.GetAttribute("name").ToLower();
                string packageId = node.GetAttribute("packageId").ToLower();
                IncompatibilityHelper.Register(name, packageId);
            }
        }
    }
}
