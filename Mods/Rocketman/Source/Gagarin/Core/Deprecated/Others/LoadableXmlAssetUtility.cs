using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using Gagarin.Core;
using RocketMan;
using Verse;

namespace Gagarin
{
    public static class LoadableXmlAssetUtility
    {

        public static void Push(XmlNode node, LoadableXmlAsset asset, XmlDocument document)
        {
            XmlElement current = document.CreateElement("DefXmlNode");
            XmlElement nLoadableId = document.CreateElement("LoadableXmlInfo");
            nLoadableId.InnerText = asset?.GetLoadableId() ?? string.Empty;
            current.SetAttribute("name", asset?.name ?? string.Empty);
            current.SetAttribute("packageId", asset?.mod?.PackageId ?? string.Empty);
            current.SetAttribute("filePath", asset?.FullFilePath ?? string.Empty);
            current.SetAttribute("isCore", (asset?.mod?.IsCoreMod ?? false) ? "true" : "false");
            current.AppendChild(nLoadableId);
            current.AppendChild(document.ImportNode(node, true));
            document.DocumentElement.AppendChild(current);
        }

        public static void Dump(Dictionary<XmlNode, LoadableXmlAsset> assetlookup, XmlDocument document, string outputPath)
        {
            XmlDocument dump = new XmlDocument();
            dump.RemoveAll();
            dump.AppendChild(dump.CreateElement("DefXmlStorage"));
            DefXmlHelper.Clear();
            HashSet<XmlElement> skipset = DefXmlHelper.FindInvalidNodes(document.DocumentElement.ChildNodes);
            LoadableXmlAsset asset;
            foreach (XmlNode node in document.DocumentElement.ChildNodes)
            {
                if (true
                    && node is XmlElement element
                    && skipset.Contains(element))
                    continue;
                asset = null;
                assetlookup.TryGetValue(node, out asset);
                Push(node, asset, dump);
            }
            DefXmlHelper.Clear();
            XmlWriterSettings settings = new XmlWriterSettings
            {
                CheckCharacters = false,
                Indent = true,
                NewLineChars = "\n"
            };
            using (XmlWriter writer = XmlWriter.Create(outputPath, settings))
            {
                dump.Save(writer);
            }
        }

        public static void Load(Dictionary<string, LoadableXmlAsset> loadablelookup, Dictionary<XmlNode, LoadableXmlAsset> assetlookup, XmlDocument document, string dumpPath)
        {
            XmlReaderSettings settings = new XmlReaderSettings
            {
                IgnoreComments = true,
                IgnoreWhitespace = true,
                CheckCharacters = false
            };
            using StringReader input = new StringReader(File.ReadAllText(dumpPath));
            using XmlReader xmlReader = XmlReader.Create(input, settings);
            LoadableXmlAsset defaultLoadable = new LoadableXmlAsset(Context.core.Name, dumpPath, "<Empty />")
            {
                mod = Context.core,
            };
            XmlDocument dump = new XmlDocument();
            dump.Load(xmlReader);
            document.RemoveAll();
            document.AppendChild(document.CreateElement("Defs"));
            assetlookup.Clear();
            foreach (XmlNode child in dump.DocumentElement.ChildNodes)
            {
                if (child.NodeType != XmlNodeType.Element)
                {
                    continue;
                }
                XmlElement node = child as XmlElement;
                string id = node.FirstChild?.InnerText ?? string.Empty;
                if (!(node.LastChild is XmlElement) || node == null)
                {
                    continue;
                }
                XmlElement defContent = document.ImportNode(node.LastChild, true) as XmlElement;
                if (!id.NullOrEmpty() && !node.GetAttribute("name").NullOrEmpty() && node.GetAttribute("isCore") == "false")
                {
                    if (loadablelookup.TryGetValue(id, out LoadableXmlAsset loadable))
                    {
                        assetlookup[node] = loadable;
                    }
                    else
                    {
                        assetlookup[node] = FindLoadable(node.GetAttribute("name"), node.GetAttribute("packageId"), node.GetAttribute("filePath"), node.GetAttribute("isCore") == "true" ? true : false);
                    }
                }
                else
                {
                    assetlookup[node] = defaultLoadable;
                }
                document.DocumentElement.AppendChild(defContent);
            }
        }

        public static LoadableXmlAsset FindLoadable(string name, string packageId, string filePath, bool isCore)
        {
            //
            // Log.Message($"GAGARIN: Fallback for finding loadable: {name}${filePath}");
            List<LoadableXmlAsset> assets = Context.assetPackageIdlookup[packageId];
            foreach (LoadableXmlAsset loadable in assets)
            {
                if (false
                    || loadable.name == name
                    || loadable.FullFilePath == filePath)
                    return loadable;
            }
            string report = "";
            foreach (LoadableXmlAsset loadable in assets)
                report += $"{loadable.name}:{loadable.FullFilePath}\n";
            throw new Exception($"GAGARIN: didn't find a loadable asset with name:{name} packageId:{packageId} filePath:{filePath}\n" +
                $"report:\n {report}");
        }

        public static string GetLoadableId(this LoadableXmlAsset asset)
        {
            string result = "$" + asset.name + "$" + asset.FullFilePath + "$" + "$" + (asset.mod?.PackageId ?? "[unkown]").ToLower();
            return result.Replace('/', '$').Base64Encode();
        }
    }
}
