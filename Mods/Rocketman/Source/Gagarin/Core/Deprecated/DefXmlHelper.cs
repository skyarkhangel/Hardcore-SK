using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Xml;
using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;
using RocketMan;
using Verse;

namespace Gagarin.Core
{
    public static class DefXmlHelper
    {
        private static HashSet<XmlElement> skipSet = new HashSet<XmlElement>();

        private static Dictionary<string, XmlElement> namelookup = new Dictionary<string, XmlElement>();

        private static Dictionary<string, string[]> fieldslookup = new Dictionary<string, string[]>();

        private static Dictionary<string, Node> graph = new Dictionary<string, Node>();

        private class Node
        {
            public XmlElement parent;

            public List<XmlElement> elements = new List<XmlElement>();

            public HashSet<string> fields = new HashSet<string>();

            public bool processed = false;

            public bool ParentHasParent => parent.HasAttribute("ParentName") && parent.GetAttribute("ParentName") != string.Empty;

            public bool ParentIsAbstract => parent.HasAttribute("Abstract") && parent.GetAttribute("Abstract") == "true";
        }

        public static void Clear()
        {
            graph.Clear();
            fieldslookup.Clear();
            namelookup.Clear();
        }

        public static HashSet<XmlElement> FindInvalidNodes(XmlNodeList nodes)
        {
            BuildFieldsDatabase();
            BuildGraph(nodes);
            XmlElement node;
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].NodeType == XmlNodeType.Element)
                {
                    node = nodes[i] as XmlElement;
                    if (!IsValid(node))
                    {
                        skipSet.Add(node);
                        continue;
                    }
                    if (node?.IsEmpty ?? true)
                    {
                        continue;
                    }
                    if (node.HasAttribute("Name"))
                    {
                        TryRegisterNamed(node, node.GetAttribute("Name"));
                    }
                }
            }
            DefXmlHelper.Clear();
            return skipSet;
        }

        private static void BuildGraph(XmlNodeList nodes)
        {
            //XmlElement node;
            //for (int i = 0; i < nodes.Count; i++)
            //{
            //    if (nodes[i].NodeType == XmlNodeType.Element)
            //    {
            //        node = nodes[i] as XmlElement;
            //        if (node.HasAttribute("Abstract") && node.GetAttribute("Abstract").ToLower() == "true")
            //        {
            //            AddParent(node);
            //        }
            //        if (node.HasAttribute("ParentName"))
            //        {
            //            AddChild(node);
            //        }
            //    }
            //}
        }

        static void AddChild(XmlElement child)
        {
            string parentName = child.GetAttribute("ParentName");
            if (!graph.TryGetValue(parentName, out Node node))
            {
                node = graph[parentName] = new Node();
            }
            node.elements.Add(child);
            string[] fields = ProcessXmlFields(AccessTools.TypeByName(GetDefClass(child, includeNamespace: true)));
            node.fields.AddRange(fields);
        }

        static void AddParent(XmlElement parent)
        {
            string name = parent.GetAttribute("Name");
            if (!graph.TryGetValue(name, out Node node))
            {
                node = graph[name] = new Node();
            }
            node.parent = parent;
        }

        private static void BuildFieldsDatabase()
        {
            fieldslookup.Clear();
            foreach (Type type in typeof(Def).AllSubclassesNonAbstract())
            {
                ProcessXmlFields(type);
            }
        }

        private static BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;
        private static string[] emptyArray = new string[0];

        private static string[] ProcessXmlFields(Type type)
        {
            if (fieldslookup.TryGetValue(type.Name, out string[] output))
            {
                return output;
            }
            if ((!type.IsSubclassOf(typeof(Def)) && type != typeof(Def)) || type == typeof(Def).BaseType)
            {
                return emptyArray;
            }
            List<string> fields = type
                .GetFields(flags)
                .Select(f => f.Name.Trim())
                .ToList();
            fields.AddRange(ProcessXmlFields(type.BaseType));
            return fieldslookup[type.Name] = fields.ToArray();
        }

        private static bool IsValid(XmlElement defXmlNode)
        {
            string typeName = GetDefClass(defXmlNode);
            if (!fieldslookup.TryGetValue(typeName, out string[] fields))
            {
                return true;
            }
            //if (defXmlNode.HasAttribute("Abstract"))
            //{
            //    fields = graph[defXmlNode.GetAttribute("Name")].fields.ToArray();
            //}
            foreach (XmlNode node in defXmlNode.ChildNodes)
            {
                string name = node.Name.Trim();
                if (!fields.Any(n => n == name))
                {
                    if (RocketEnvironmentInfo.IsDevEnv)
                    {
                        string report = $"GAGARIN: Invalid node " +
                            $"<color=red>{node.Name}:{defXmlNode.Name}</color> " +
                            $"the class fields are:\n";
                        foreach (string f in fields)
                            report += f + "\n";
                        Log.Warning(report);
                    }
                    return false;
                }
            }
            return true;
        }

        private static string GetDefClass(XmlElement defXmlNode, bool includeNamespace = false)
        {
            string typeName = defXmlNode.Name;
            if (defXmlNode.HasAttribute("Class"))
            {
                typeName = defXmlNode.GetAttribute("Class");
                if (!fieldslookup.ContainsKey(typeName))
                {
                    typeName = !includeNamespace ? AccessTools.TypeByName(typeName).Name
                        : AccessTools.TypeByName(typeName).FullName;
                }
            }
            return typeName;
        }

        private static void TryRegisterNamed(XmlElement node, string name)
        {
            if (skipSet.Contains(node))
            {
                return;
            }
            if (namelookup.TryGetValue(name, out XmlElement other))
            {
                if (RocketEnvironmentInfo.IsDevEnv)
                {
                    Log.Warning($"GAGARIN: Duplicates detected for <{other.Name} Name=\"{name}\">");
                }
                skipSet.Add(other);
            }
            namelookup[name] = node;
        }
    }
}
