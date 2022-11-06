using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Verse;

namespace Gagarin
{
    public static class AssetHashingUtility
    {
        public static Dictionary<string, UInt64> Load(string path)
        {
            Dictionary<string, UInt64> result = new Dictionary<string, UInt64>();
            XmlDocument document = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings
            {
                IgnoreComments = true,
                IgnoreWhitespace = true,
                CheckCharacters = false
            };
            try
            {
                using StringReader input = new StringReader(File.ReadAllText(path));
                using XmlReader xmlReader = XmlReader.Create(input, settings);
                document.Load(xmlReader);

                foreach (XmlElement node in document.DocumentElement.ChildNodes)
                {
                    if (node.NodeType != XmlNodeType.Element)
                        continue;
                    result[node.GetAttribute("id")] = UInt64.Parse(node.GetAttribute("hash"));
                }
            }
            catch (Exception er)
            {
                Log.Error($"GAGARIN: Error while loading the old hashes dump! DELETING THE OLD FILE! {er}");
                if (File.Exists(path))
                    File.Delete(path);
            }
            return result;
        }

        public static void Dump(Dictionary<string, UInt64> hashes, string path)
        {
            if (File.Exists(path))
                File.Delete(path);

            XmlDocument document = new XmlDocument();
            XmlElement root = document.CreateElement("AssetsHash");
            foreach (KeyValuePair<string, UInt64> assetHashPair in hashes)
            {
                XmlElement modXml = document.CreateElement("Asset");
                modXml.SetAttribute("id", $"{assetHashPair.Key}");
                modXml.SetAttribute("hash", $"{assetHashPair.Value}");
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
    }
}
