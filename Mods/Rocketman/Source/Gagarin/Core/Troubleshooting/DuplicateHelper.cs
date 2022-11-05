using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using RocketMan;
using Verse;

namespace Gagarin
{
    public static class DuplicateHelper
    {
        private static DuplicateReport[] duplicates;

        private static Dictionary<string, DuplicateReport> nameToReport = new Dictionary<string, DuplicateReport>();

        private static Dictionary<string, DuplicateReport> defNameToReport = new Dictionary<string, DuplicateReport>();

        private static Stopwatch stopwatch = new Stopwatch();

        public static Thread Thread_ReportParser;

        public static IEnumerable<DuplicateReport> Duplicates
        {
            get => duplicates;
        }

        public static void QueueReportProcessing()
        {
            LongEventHandler.QueueLongEvent(() => JoinReportParser(), textKey: "Gagarin.ParsingReports", false, null);
        }

        public static void ParseCreateReports(XmlDocument document, Dictionary<XmlNode, LoadableXmlAsset> assetlookup)
        {
            Log.Message("GAGARIN:[DUPLICATE]: Started!");
            if (Context.IsRecovering)
            {
                return;
            }
            List<DuplicateReport> duplicates_Base;
            List<DuplicateReport> duplicates_defName;
            bool failed = true;
            stopwatch.Restart();
            try
            {
                duplicates_Base = FindAllDuplicateBases(document, assetlookup)?.ToList() ?? new List<DuplicateReport>();
                duplicates_defName = FindAllDuplicateDefs(document, assetlookup)?.ToList() ?? new List<DuplicateReport>();
                duplicates_Base.AddRange(duplicates_defName);
                duplicates = duplicates_Base.ToArray();
                failed = false;
            }
            catch (Exception er)
            {
                Logger.Debug("GAGARIN duplicate reports creation failed", er);
                Log.Error($"GAGARIN: duplicate reports creation failed {er}");
            }
            finally
            {
                PrepareReportFolder();
            }
            stopwatch.Stop();
            Log.Message($"GAGARIN:[DUPLICATE] Creating duplication reports took {stopwatch.ElapsedMilliseconds} MS");
            if (failed)
            {
                return;
            }
            Thread_ReportParser = new Thread(new ThreadStart(() =>
            {
                try
                {
                    ParseReportsOffMainThread();
                }
                catch (Exception er)
                {
                    Logger.Debug("GAGARIN: Parsing report failed", er);
                }
            }));
            Thread_ReportParser.Start();
        }

        private static void ParseReportsOffMainThread()
        {
            if (Context.IsRecovering)
            {
                return;
            }
            StringBuilder builder = new StringBuilder();
            GenThreading.ParallelForEach(duplicates.ToList(),
            (d) =>
            {
                d.CalculateDiff();
            });
            for (int i = 0; i < duplicates.Length; i++)
            {
                DuplicateReport report = duplicates[i];
                if (!report.Problematic)
                {
                    continue;
                }
                if (!report.HasDuplicates)
                {
                    throw new Exception($"GAGARIN:[DUPLICATE] Processor return a DuplicateReport with length={report.Length} for name={report.Name}");
                }
                int j = 1;
                report.Write(Path.Combine(GagarinEnvironmentInfo.ReportsFolderPath, $"Report_{GenText.SanitizeFilename(report.Name.CapitalizeFirst())}.xml"));
                string tag = report.CoreModInvolved || report.Problematic ? "CRITICAL" : "IGNOREME";
                string primaryColor = report.CoreModInvolved ? "orange" : "yellow";
                string secondaryColor = report.CoreModInvolved ? "red" : "white";
                string type = report.ReportType == DuplicateReportType.Base ? "Name" : "defName";
                builder.Clear();
                builder.Append($"GAGARIN:[<color={primaryColor}>DUPLICATE:{tag}</color>]<color={secondaryColor}> duplicate found for {type}={report.Name}</color>");
                foreach (DuplicateReport.DuplicationRecord record in report.Records)
                {
                    builder.AppendInNewLine($"\t{j++}. PackageId={record.mod?.PackageId}\t| ModName={record.mod?.Name}\t| XmlFilePath={record.xmlFilePath}");
                }
                Log.Message(builder.ToString());
            }
            Log.Message($"GAGARIN:[DUPLICATE] Finished creating reports at <color=red>{GagarinEnvironmentInfo.ReportsFolderPath}</color>");
            nameToReport.Clear();
        }

        private static void JoinReportParser()
        {
            if (Thread_ReportParser == null || Context.IsRecovering)
            {
                return;
            }
            try
            {
                Thread_ReportParser.Join();
            }
            catch (Exception er)
            {
                Logger.Debug($"GAGARIN: Report parsing failed with error", er);
            }
            finally
            {
                Log.Message("GAGARIN: Lock removed!");
            }
        }

        private static IEnumerable<DuplicateReport> FindAllDuplicateBases(XmlDocument document, Dictionary<XmlNode, LoadableXmlAsset> assetlookup)
        {
            foreach (XmlNode node in document.DocumentElement.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                {
                    continue;
                }
                XmlElement element = node as XmlElement;
                if (!element.HasAttribute("Name"))
                {
                    continue;
                }
                ModContentPack mod = Context.Core;
                if (assetlookup.TryGetValue(node, out LoadableXmlAsset asset))
                {
                    mod = asset.mod;
                }
                string name = element.GetAttribute("Name");
                if (!nameToReport.TryGetValue(name, out DuplicateReport report))
                {
                    report = nameToReport[name] = new DuplicateReport(name, DuplicateReportType.Base);
                }
                report.AddXmlNode(node, mod, asset?.FullFilePath ?? null);
                if (report.HasDuplicates && report.Length == 2)
                {
                    yield return report;
                }
            }
        }

        private static IEnumerable<DuplicateReport> FindAllDuplicateDefs(XmlDocument document, Dictionary<XmlNode, LoadableXmlAsset> assetlookup)
        {
            foreach (XmlNode node in document.DocumentElement.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                {
                    continue;
                }
                XmlElement element = node as XmlElement;
                if (element.HasAttribute("Abstract") && element.GetAttribute("Abstract").ToLower() == "true")
                {
                    continue;
                }
                if (!TryGetDefName(element, out string defName))
                {
                    continue;
                }
                ModContentPack mod = Context.Core;
                if (assetlookup.TryGetValue(node, out LoadableXmlAsset asset))
                {
                    mod = asset.mod;
                }
                if (!defNameToReport.TryGetValue(defName, out DuplicateReport report))
                {
                    report = nameToReport[defName] = new DuplicateReport(defName, DuplicateReportType.DefName);
                }
                report.AddXmlNode(node, mod, asset?.FullFilePath ?? null);
                if (report.HasDuplicates && report.Length == 2)
                {
                    yield return report;
                }
            }
        }

        private static bool TryGetDefName(XmlElement root, out string defName)
        {
            defName = string.Empty;
            foreach (XmlNode child in root.ChildNodes)
            {
                if (child.NodeType != XmlNodeType.Element)
                {
                    continue;
                }
                XmlElement element = child as XmlElement;
                if (element.Name == "defName")
                {
                    defName = element.InnerText.Trim();
                    return true;
                }
            }
            return false;
        }

        private static bool _preparedOnce = false;

        private static void PrepareReportFolder()
        {
            if (_preparedOnce)
            {
                return;
            }
            if (Directory.Exists(GagarinEnvironmentInfo.ReportsFolderPath))
            {
                Directory.Delete(GagarinEnvironmentInfo.ReportsFolderPath, recursive: true);
            }
            _preparedOnce = true;
            Directory.CreateDirectory(GagarinEnvironmentInfo.ReportsFolderPath);
        }

    }
}
