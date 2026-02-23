using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitDoorWindowCounter
{
    [Transaction(TransactionMode.Manual)]
    public class CmdCountDWHostAndLinksExportCsv : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var doc = commandData.Application.ActiveUIDocument.Document;

            try
            {
                var rows = new List<Row>();

                // MAIN
                rows.AddRange(CollectDoorWindowRows(doc, "MAIN"));

                // LINKS (loaded only)
                var linkInstances = new FilteredElementCollector(doc)
                    .OfClass(typeof(RevitLinkInstance))
                    .Cast<RevitLinkInstance>()
                    .ToList();

                foreach (var link in linkInstances)
                {
                    var linkDoc = link.GetLinkDocument();
                    if (linkDoc == null) continue; // unloaded

                    var linkName = GetLinkLabel(doc, link);
                    rows.AddRange(CollectDoorWindowRows(linkDoc, linkName));
                }

                // Group & count (Level + TypeMark + Type)
                var grouped = rows
                    .GroupBy(r => new
                    {
                        r.Source,
                        r.Category,
                        r.Level,
                        r.TypeMark,
                        r.FamilyAndType
                    })
                    .Select(g => new ResultRow
                    {
                        Source = g.Key.Source,
                        Category = g.Key.Category,
                        Level = g.Key.Level,
                        TypeMark = g.Key.TypeMark,
                        FamilyAndType = g.Key.FamilyAndType,
                        Count = g.Count()
                    });

                // Sort: Level, TypeMark, Type, Count(desc)
                var sorted = grouped
                    .OrderBy(x => x.Level, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(x => x.TypeMark, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(x => x.FamilyAndType, StringComparer.OrdinalIgnoreCase)
                    .ThenByDescending(x => x.Count)
                    .ThenBy(x => x.Source, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(x => x.Category, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                // Export CSV
                var outPath = WriteCsv(sorted);

                // Quick summary
                int doorTotal = rows.Count(r => r.Category == "Door");
                int windowTotal = rows.Count(r => r.Category == "Window");
                int loadedLinkCount = linkInstances.Count(li => li.GetLinkDocument() != null);

                TaskDialog.Show("DW Counter (Host + Links) → CSV",
                    "Done!\n\n" +
                    "Loaded links: " + loadedLinkCount + "\n" +
                    "Doors: " + doorTotal + "\n" +
                    "Windows: " + windowTotal + "\n\n" +
                    "CSV saved to:\n" + outPath);

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.ToString();
                return Result.Failed;
            }
        }

        private static string GetLinkLabel(Document hostDoc, RevitLinkInstance link)
        {
            // Prefer link type name (often shows RVT link name), fallback to instance name
            try
            {
                var type = hostDoc.GetElement(link.GetTypeId());
                if (type != null && !string.IsNullOrWhiteSpace(type.Name))
                    return type.Name.Trim();
            }
            catch { }

            return string.IsNullOrWhiteSpace(link.Name) ? "LINK" : link.Name.Trim();
        }

        private static IEnumerable<Row> CollectDoorWindowRows(Document anyDoc, string sourceLabel)
        {
            // Doors
            foreach (var e in new FilteredElementCollector(anyDoc)
                         .OfCategory(BuiltInCategory.OST_Doors)
                         .WhereElementIsNotElementType())
            {
                yield return MakeRow(anyDoc, e, sourceLabel, "Door");
            }

            // Windows
            foreach (var e in new FilteredElementCollector(anyDoc)
                         .OfCategory(BuiltInCategory.OST_Windows)
                         .WhereElementIsNotElementType())
            {
                yield return MakeRow(anyDoc, e, sourceLabel, "Window");
            }
        }

        private static Row MakeRow(Document doc, Element e, string source, string category)
        {
            return new Row
            {
                Source = source,
                Category = category,
                Level = ResolveLevelName(doc, e),
                TypeMark = ResolveTypeMark(doc, e),
                FamilyAndType = ResolveFamilyAndType(doc, e)
            };
        }

        private static string ResolveTypeMark(Document doc, Element e)
        {
            try
            {
                var type = doc.GetElement(e.GetTypeId()) as ElementType;
                if (type == null) return "";

                // Built-in: Type Mark
                var p = type.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_MARK);
                var v = p != null ? p.AsString() : null;
                if (!string.IsNullOrWhiteSpace(v)) return v.Trim();

                // Fallback (common label)
                var p2 = type.LookupParameter("Type Mark");
                v = p2 != null ? p2.AsString() : null;
                if (!string.IsNullOrWhiteSpace(v)) return v.Trim();
            }
            catch { }

            return "";
        }

        private static string ResolveFamilyAndType(Document doc, Element e)
        {
            try
            {
                var type = doc.GetElement(e.GetTypeId()) as ElementType;
                if (type == null) return "(Unknown Type)";

                var fam = string.IsNullOrWhiteSpace(type.FamilyName) ? "" : type.FamilyName.Trim();
                var typ = string.IsNullOrWhiteSpace(type.Name) ? "" : type.Name.Trim();

                if (string.IsNullOrWhiteSpace(fam) && string.IsNullOrWhiteSpace(typ)) return "(Unknown Type)";
                if (string.IsNullOrWhiteSpace(fam)) return typ;
                if (string.IsNullOrWhiteSpace(typ)) return fam;

                return fam + " : " + typ;
            }
            catch
            {
                return "(Unknown Type)";
            }
        }

        private static string ResolveLevelName(Document doc, Element e)
        {
            // Primary: FamilyInstance.LevelId
            var fi = e as FamilyInstance;
            if (fi != null)
            {
                try
                {
                    var lid = fi.LevelId;
                    if (lid != ElementId.InvalidElementId)
                    {
                        var lvl = doc.GetElement(lid) as Level;
                        if (lvl != null && !string.IsNullOrWhiteSpace(lvl.Name))
                            return lvl.Name.Trim();
                    }
                }
                catch { }
            }

            // Fallback parameters
            try
            {
                var p1 = e.get_Parameter(BuiltInParameter.FAMILY_LEVEL_PARAM);
                if (p1 != null && p1.StorageType == StorageType.ElementId)
                {
                    var lvl = doc.GetElement(p1.AsElementId()) as Level;
                    if (lvl != null && !string.IsNullOrWhiteSpace(lvl.Name))
                        return lvl.Name.Trim();
                }
            }
            catch { }

            try
            {
                var p2 = e.get_Parameter(BuiltInParameter.INSTANCE_REFERENCE_LEVEL_PARAM);
                if (p2 != null && p2.StorageType == StorageType.ElementId)
                {
                    var lvl = doc.GetElement(p2.AsElementId()) as Level;
                    if (lvl != null && !string.IsNullOrWhiteSpace(lvl.Name))
                        return lvl.Name.Trim();
                }
            }
            catch { }

            return "(No Level)";
        }

        private static string WriteCsv(IList<ResultRow> rows)
        {
            var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var outDir = Path.Combine(docs, "RevitDoorWindowCounter");
            Directory.CreateDirectory(outDir);

            var fileName = "DW_HostAndLinks_TypeMark_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";
            var path = Path.Combine(outDir, fileName);

            var sb = new StringBuilder();
            sb.AppendLine("Source,Category,Level,TypeMark,FamilyAndType,Count");

            foreach (var r in rows)
            {
                sb.AppendLine(string.Join(",",
                    Csv(r.Source),
                    Csv(r.Category),
                    Csv(r.Level),
                    Csv(r.TypeMark),
                    Csv(r.FamilyAndType),
                    r.Count.ToString()
                ));
            }

            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            return path;
        }

        private static string Csv(string s)
        {
            if (s == null) s = "";
            s = s.Replace("\"", "\"\"");
            return "\"" + s + "\"";
        }

        private class Row
        {
            public string Source { get; set; }         // MAIN or link label
            public string Category { get; set; }       // Door / Window
            public string Level { get; set; }          // Level name (in that document)
            public string TypeMark { get; set; }       // Type Mark (from ElementType)
            public string FamilyAndType { get; set; }  // Family : Type
        }

        private class ResultRow
        {
            public string Source { get; set; }
            public string Category { get; set; }
            public string Level { get; set; }
            public string TypeMark { get; set; }
            public string FamilyAndType { get; set; }
            public int Count { get; set; }
        }
    }
}