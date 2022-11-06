using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Verse;

namespace Gagarin
{
    public static class RunningModsSetUtility
    {
        public static HashSet<string> Load(string path)
        {
            HashSet<string> result = new HashSet<string>();
            XmlDocument document = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings
            {
                IgnoreComments = true,
                IgnoreWhitespace = true,
                CheckCharacters = false
            };
            using StringReader input = new StringReader(File.ReadAllText(path));
            using XmlReader xmlReader = XmlReader.Create(input, settings);
            document.Load(xmlReader);
            foreach (XmlElement modXml in document.DocumentElement.ChildNodes)
            {
                if (modXml.Name != "Mod")
                    continue;
                result.Add(modXml.GetAttribute("packageId"));
            }
            return result;
        }

        public static void Dump(IEnumerable<ModContentPack> mods, string path)
        {
            if (File.Exists(path))
                File.Delete(path);
            XmlDocument document = new XmlDocument();
            XmlElement root = document.CreateElement("RunningMods");
            foreach (ModContentPack mod in mods)
            {
                XmlElement modXml = document.CreateElement("Mod");
                modXml.SetAttribute("packageId", mod.PackageId);
                root.AppendChild(modXml);
            }
            document.AppendChild(root);
            XmlWriterSettings settings = new XmlWriterSettings
            {
                CheckCharacters = false,
                Indent = true,
                NewLineChars = "\n"
            };
            using (XmlWriter writer = XmlWriter.Create(path, settings))
            {
                document.Save(writer);
            }
        }

        public static bool Changed(HashSet<string> current, string path)
        {
            if (!File.Exists(path))
                return true;
            HashSet<string> old = Load(path);
            if (current.Count != old.Count)
                return true;
            return current.Intersect(current).Count() != current.Count;
        }
    }
}
