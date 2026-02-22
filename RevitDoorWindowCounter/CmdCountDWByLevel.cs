using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitDoorWindowCounter
{
    [Transaction(TransactionMode.Manual)]
    public class CmdCountDWByLevel : IExternalCommand
    {
        // Ubah ini kalau mau tampilkan lebih banyak tipe per level
        private const int TOP_TYPES_PER_LEVEL = 10;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var doc = commandData.Application.ActiveUIDocument.Document;

            var doors = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Doors)
                .WhereElementIsNotElementType()
                .Cast<Element>()
                .ToList();

            var windows = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Windows)
                .WhereElementIsNotElementType()
                .Cast<Element>()
                .ToList();

            // Kumpulkan level order (urut elevation)
            var levelOrder = GetLevelOrder(doc);

            // Siapkan data: Level -> (TypeKey -> count)
            var doorMap = GroupByLevelAndType(doc, doors);
            var windowMap = GroupByLevelAndType(doc, windows);

            // Semua level yang muncul (gabungan doors & windows)
            var allLevels = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var k in doorMap.Keys) allLevels.Add(k);
            foreach (var k in windowMap.Keys) allLevels.Add(k);

            // Urutkan: level known dulu sesuai elevation, sisanya di belakang (mis. "(No Level)")
            var orderedLevels = levelOrder.Where(allLevels.Contains).ToList();
            foreach (var extra in allLevels.Where(l => !orderedLevels.Contains(l))) orderedLevels.Add(extra);

            var sb = new StringBuilder();
            sb.AppendLine("Door & Window Counter (by Level + Type)");
            sb.AppendLine("---------------------------------------");
            sb.AppendLine($"Total Doors   : {doors.Count}");
            sb.AppendLine($"Total Windows : {windows.Count}");
            sb.AppendLine();

            foreach (var levelName in orderedLevels)
            {
                int doorTotal = doorMap.TryGetValue(levelName, out var dTypes) ? dTypes.Values.Sum() : 0;
                int winTotal = windowMap.TryGetValue(levelName, out var wTypes) ? wTypes.Values.Sum() : 0;

                sb.AppendLine($"[{levelName}]  Doors: {doorTotal} | Windows: {winTotal}");

                // Doors by type (Top N)
                if (doorTotal > 0 && dTypes != null)
                {
                    sb.AppendLine("  Doors by Type:");
                    AppendTopTypes(sb, dTypes, TOP_TYPES_PER_LEVEL);
                }

                // Windows by type (Top N)
                if (winTotal > 0 && wTypes != null)
                {
                    sb.AppendLine("  Windows by Type:");
                    AppendTopTypes(sb, wTypes, TOP_TYPES_PER_LEVEL);
                }

                sb.AppendLine();
            }

            TaskDialog.Show("DW Counter", sb.ToString());
            return Result.Succeeded;
        }

        private static Dictionary<string, Dictionary<string, int>> GroupByLevelAndType(Document doc, IList<Element> instances)
        {
            var result = new Dictionary<string, Dictionary<string, int>>(StringComparer.OrdinalIgnoreCase);

            foreach (var e in instances)
            {
                var levelName = ResolveLevelName(doc, e);
                var typeKey = ResolveTypeKey(doc, e);

                if (!result.TryGetValue(levelName, out var typeDict))
                {
                    typeDict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    result[levelName] = typeDict;
                }

                if (!typeDict.ContainsKey(typeKey)) typeDict[typeKey] = 0;
                typeDict[typeKey]++;
            }

            return result;
        }

        private static string ResolveTypeKey(Document doc, Element e)
        {
            var type = doc.GetElement(e.GetTypeId()) as ElementType;
            if (type == null) return "(Unknown Type)";
            return $"{type.FamilyName} : {type.Name}";
        }

        private static void AppendTopTypes(StringBuilder sb, Dictionary<string, int> typeCounts, int top)
        {
            int i = 0;
            foreach (var kv in typeCounts.OrderByDescending(x => x.Value))
            {
                sb.AppendLine($"    - {kv.Value}x  {kv.Key}");
                i++;
                if (i >= top) break;
            }

            if (typeCounts.Count > top)
                sb.AppendLine($"    (…and {typeCounts.Count - top} more types)");
        }

        private static string ResolveLevelName(Document doc, Element e)
        {
            // Primary: FamilyInstance.LevelId
            if (e is FamilyInstance fi)
            {
                var lid = fi.LevelId;
                if (lid != ElementId.InvalidElementId)
                {
                    var lvl = doc.GetElement(lid) as Level;
                    if (lvl != null) return lvl.Name;
                }
            }

            // Fallback parameters
            var p1 = e.get_Parameter(BuiltInParameter.FAMILY_LEVEL_PARAM);
            if (p1 != null && p1.StorageType == StorageType.ElementId)
            {
                var lvl = doc.GetElement(p1.AsElementId()) as Level;
                if (lvl != null) return lvl.Name;
            }

            var p2 = e.get_Parameter(BuiltInParameter.INSTANCE_REFERENCE_LEVEL_PARAM);
            if (p2 != null && p2.StorageType == StorageType.ElementId)
            {
                var lvl = doc.GetElement(p2.AsElementId()) as Level;
                if (lvl != null) return lvl.Name;
            }

            return "(No Level)";
        }

        private static List<string> GetLevelOrder(Document doc)
        {
            return new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .OrderBy(l => l.Elevation)
                .Select(l => l.Name)
                .ToList();
        }
    }
}