using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Xml;
//using Mono.Security.X509.Extensions;
using RocketMan;
using Verse;
using static Verse.XmlInheritance;

namespace Gagarin
{
    public static class CachedDefHelper
    {
        private static XmlDocument document;

        private static List<DefXmlUnit> defs = new List<DefXmlUnit>();

        private static HashSet<string> registeredNames = new HashSet<string>();

        private class DefXmlUnit
        {
            public Def def;
            public XmlNode node;
            public LoadableXmlAsset asset;
            public XmlInheritanceNode inheritanceNode;
        }

        public static void Prepare()
        {
            if (Context.IsUsingCache)
                return;

            document = new XmlDocument();
            document.AppendChild(document.CreateElement("DefXmlStorage"));
        }

        public static void Clean()
        {
            defs?.Clear();
            registeredNames?.Clear();
            document?.RemoveAll();
            document = null;
        }

        public static void Register(Def def, XmlNode node, LoadableXmlAsset asset)
        {
            defs.Add(new DefXmlUnit()
            {
                def = def,
                node = node,
                asset = asset,
                inheritanceNode = XmlInheritance.resolvedNodes.TryGetValue(node, out XmlInheritanceNode inheritanceNode)
                ? inheritanceNode : null
            });
        }

        public static void Save()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            XmlElement root = document.DocumentElement;
            XmlElement wrapper;
            XmlElement resolvedNode;

            foreach (DefXmlUnit unit in defs)
            {
                XmlElement node = unit.node as XmlElement;
                if (unit.inheritanceNode == null)
                {
                    wrapper = WrapXmlNode(node, unit.asset?.FullFilePath);
                    root.AppendChild(wrapper);
                    continue;
                }
                if (unit.inheritanceNode.resolvedXmlNode == null)
                {
                    Log.Error($"GAGARIN: {unit.def.defName} has <color=yellow>resolvedXmlNode == null!</color>");
                    continue;
                }

                resolvedNode = unit.inheritanceNode.resolvedXmlNode as XmlElement;
                resolvedNode.RemoveAttribute("ParentName");

                if (resolvedNode.Name != node.Name)
                {
                    XmlElement temp = document.CreateElement(node.Name);
                    foreach (XmlNode n in resolvedNode.ChildNodes)
                    {
                        if (n.NodeType != XmlNodeType.Element)
                            continue;
                        temp.AppendChild(document.ImportNode(n, true));
                    }
                    resolvedNode = temp;
                }
                else if (node.HasAttribute("Class") && !resolvedNode.HasAttribute("Class"))
                    resolvedNode.SetAttribute("Class", node.GetAttribute("Class"));

                wrapper = WrapXmlNode(resolvedNode, unit.asset?.FullFilePath);
                wrapper.SetAttribute("resolved", "true");

                root.AppendChild(wrapper);
            }

            XmlWriterSettings settings = new XmlWriterSettings
            {
                CheckCharacters = false,
                Indent = true,
                NewLineChars = "\n"
            };
            using (XmlWriter writer = XmlWriter.Create(GagarinEnvironmentInfo.UnifiedXmlFilePath, settings))
            {
                document.Save(writer);
            }

            stopwatch.Stop();
            //
            //CachedDefHelper.Dump();
            Log.Warning($"GAGARIN: <color=white>Cache created!</color> creating cache took <color=green>{stopwatch.ElapsedMilliseconds / 1000} seconds</color>");
        }

        public static void Load(XmlDocument document, Dictionary<XmlNode, LoadableXmlAsset> assets)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            XmlReaderSettings settings = new XmlReaderSettings
            {
                IgnoreComments = true,
                IgnoreWhitespace = true,
                CheckCharacters = false
            };
            using StringReader input = new StringReader(File.ReadAllText(GagarinEnvironmentInfo.UnifiedXmlFilePath));
            using XmlReader xmlReader = XmlReader.Create(input, settings);
            LoadableXmlAsset defaultLoadable = new LoadableXmlAsset(Context.Core.Name, GagarinEnvironmentInfo.UnifiedXmlFilePath, "<Empty />")
            {
                mod = Context.Core
            };
            string path;
            XmlNode defXml;
            assets.Clear();
            document.RemoveAll();
            document.AppendChild(document.CreateElement("Defs"));
            XmlDocument unifiedDocument = new XmlDocument();
            unifiedDocument.RemoveAll();
            Stopwatch documentStopwatch = new Stopwatch();
            documentStopwatch.Start();
            unifiedDocument.Load(xmlReader);
            documentStopwatch.Stop();
            Log.Warning($"GAGARIN: <color=green>Loadeding XmlDocument</color> took <color=red>{(float)documentStopwatch.ElapsedTicks / Stopwatch.Frequency} seconds</color>");

            foreach (XmlElement element in unifiedDocument.DocumentElement.ChildNodes)
            {
                defXml = document.ImportNode(element.FirstChild, true);
                path = element.GetAttribute("path");

                if (Context.XmlAssets.TryGetValue(path, out LoadableXmlAsset asset))
                    assets[defXml] = asset;

                document.DocumentElement.AppendChild(defXml);
            }

            stopwatch.Stop();
            Log.Warning($"GAGARIN: <color=green>Loaded from cache!</color> Loading cache took <color=red>{stopwatch.ElapsedMilliseconds / 1000} seconds</color>");
        }

        private static void Dump()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            XmlDocument document = new XmlDocument();
            document.AppendChild(document.CreateElement("Defs"));

            XmlElement root = document.DocumentElement;
            XmlElement wrapper;
            XmlElement resolvedNode;

            foreach (DefXmlUnit unit in defs)
            {
                XmlElement node = unit.node as XmlElement;
                if (unit.inheritanceNode == null)
                {
                    wrapper = WrapXmlNode(node, unit.asset?.FullFilePath);
                    root.AppendChild(wrapper);
                    continue;
                }
                if (unit.inheritanceNode.resolvedXmlNode == null)
                {
                    Log.Error($"GAGARIN: {unit.def.defName} has <color=yellow>resolvedXmlNode == null!</color>");
                    continue;
                }

                resolvedNode = unit.inheritanceNode.resolvedXmlNode as XmlElement;
                resolvedNode.RemoveAttribute("ParentName");

                if (resolvedNode.Name != node.Name)
                {
                    XmlElement temp = document.CreateElement(node.Name);
                    foreach (XmlNode n in resolvedNode.ChildNodes)
                    {
                        if (n.NodeType != XmlNodeType.Element)
                            continue;
                        temp.AppendChild(document.ImportNode(n, true));
                    }
                    resolvedNode = temp;
                }
                else if (node.HasAttribute("Class") && !resolvedNode.HasAttribute("Class"))
                    resolvedNode.SetAttribute("Class", node.GetAttribute("Class"));

                root.AppendChild(resolvedNode);
            }

            XmlWriterSettings settings = new XmlWriterSettings
            {
                CheckCharacters = false,
                Indent = true,
                NewLineChars = "\n"
            };
            using (XmlWriter writer = XmlWriter.Create(GagarinEnvironmentInfo.UnifiedPatchedOriginalXmlPath, settings))
            {
                document.Save(writer);
            }

            stopwatch.Stop();
            Log.Warning($"GAGARIN: <color=white>Cache created!</color> creating cache took <color=green>{stopwatch.ElapsedMilliseconds / 1000} seconds</color>");
        }

        private static XmlElement WrapXmlNode(XmlNode node, string path = null)
        {
            XmlElement xml = document.CreateElement("Item");
            xml.SetAttribute("path", path ?? string.Empty);
            xml.AppendChild(document.ImportNode(node, true));
            return xml;
        }
    }
}
