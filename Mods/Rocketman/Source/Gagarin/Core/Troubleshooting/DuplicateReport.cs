using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Build.Utilities;
using Microsoft.XmlDiffPatch;
using RocketMan;
using Verse;
using Logger = RocketMan.Logger;
using RecordPair = Verse.Pair<Gagarin.DuplicateReport.DuplicationRecord, Gagarin.DuplicateReport.DuplicationRecord>;

namespace Gagarin
{
    public enum DuplicateReportType
    {
        DefName = 0,
        Base = 1,
    }
    public class DuplicateReport
    {
        private readonly string name;

        private readonly List<DuplicationRecord> records = new List<DuplicationRecord>();

        private bool coreModInvolved = false;

        private bool problematic = false;

        private class NullableStruct<T>
        {
            public T value;
        }

        public struct DuplicationRecord
        {
            public string xmlFilePath;

            public XmlNode node;

            public ModContentPack mod;

            public bool Parentless
            {
                get => xmlFilePath == null;
            }

            public static DuplicationRecord Invalid
            {
                get => new DuplicationRecord() { node = null, mod = null };
            }

            public static DuplicationRecord Create(XmlNode node, ModContentPack mod, string xmlFilePath)
            {
                return new DuplicationRecord() { mod = mod, node = node, xmlFilePath = xmlFilePath };
            }

            public override bool Equals(object obj)
            {
                return obj is DuplicationRecord other && other.mod == mod && other.node == node && other.xmlFilePath == xmlFilePath;
            }

            public override int GetHashCode()
            {
                return node.GetHashCode();
            }
        }

        public readonly DuplicateReportType ReportType;

        public string Name
        {
            get => name;
        }

        public bool CoreModInvolved
        {
            get => coreModInvolved && HasDuplicates;
        }

        public bool Problematic
        {
            get => problematic;
        }

        public bool HasDuplicates
        {
            get => records.Count > 1;
        }

        public int Length
        {
            get => records.Count;
        }

        public List<DuplicationRecord> Records
        {
            get => records;
        }

        public IEnumerable<ModContentPack> CulpritMods
        {
            get => records.Select(r => r.mod);
        }

        public DuplicateReport(string name, DuplicateReportType reportType)
        {
            this.name = name;
            this.ReportType = reportType;
        }

        public void AddXmlNode(XmlNode node, ModContentPack mod, string xmlFilePath)
        {
            if (mod?.IsCoreMod ?? true)
            {
                coreModInvolved = true;
            }
            XmlDocument document = new XmlDocument();
            records.Add(DuplicationRecord.Create(document.ImportNode(node, true), mod, xmlFilePath));
            if (records.Count > 1)
            {
                problematic = true;
            }
        }

        public ModContentPack GetCulprit(XmlNode node)
        {
            foreach (DuplicationRecord record in records)
            {
                if (record.node == node)
                    return record.mod;
            }
            return null;
        }

        public void CalculateDiff()
        {
            try
            {
                List<RecordPair> xmldiffPairs = new List<RecordPair>();
                XmlDiff xmldiff = new XmlDiff(XmlDiffOptions.IgnoreChildOrder
                        | XmlDiffOptions.IgnoreNamespaces
                        | XmlDiffOptions.IgnorePrefixes
                        | XmlDiffOptions.IgnoreWhitespace
                        | XmlDiffOptions.IgnoreComments);
                NullableStruct<DuplicationRecord>[] nodes = Records.Select(r => new NullableStruct<DuplicationRecord>() { value = r }).ToArray();
                DuplicationRecord a;
                DuplicationRecord b;
                for (int i = 0; i < nodes.Length; i++)
                {
                    if (nodes[i] == null)
                        continue;
                    a = nodes[i].value;
                    for (int j = i + 1; j < nodes.Length; j++)
                    {
                        if (nodes[j] == null)
                            continue;
                        b = nodes[j].value;
                        if (xmldiff.Compare(a.node, b.node))
                        {
                            nodes[j] = null;
                            continue;
                        }
                        xmldiffPairs.Add(new RecordPair(a, b));
                    }
                }
                if (nodes.Count(n => n != null) == 1)
                {
                    problematic = false;
                    return;
                }
            }
            catch (Exception er)
            {
                problematic = true;
                string reportType = ReportType == DuplicateReportType.Base ? "DuplicateReportType.Base" : "DuplicateReportType.DefName";
                Logger.Debug($"GAGARIN: Parsing report {name}:{reportType}", er);
            }
        }

        public void Write(string path)
        {
            XmlDocument document = new XmlDocument();
            XmlElement root;
            document.AppendChild(root = document.CreateElement("DuplicateReport"));
            root.SetAttribute("Name", Name);
            root.SetAttribute("Type", ReportType == DuplicateReportType.Base ? "DuplicateReportType.Base" : "DuplicateReportType.DefName");
            root.SetAttribute("Length", Length.ToString());
            root.SetAttribute("IsCoreModInvolved", coreModInvolved ? "True" : "False");
            root.SetAttribute("IsCoreModInvolved", coreModInvolved ? "True" : "False");
            foreach (DuplicationRecord record in Records)
            {
                XmlElement node = document.CreateElement("DuplicationRecord");
                node.SetAttribute("PackageId", record.mod?.PackageIdPlayerFacing);
                node.SetAttribute("ModName", record.mod?.Name);
                node.SetAttribute("XmlFilePath", record.xmlFilePath);
                node.AppendChild(document.ImportNode(record.node, true));
                root.AppendChild(node);
            }
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

        public override int GetHashCode()
        {
            return name.GetHashCode() ^ records.GetHashCode();
        }
    }
}
