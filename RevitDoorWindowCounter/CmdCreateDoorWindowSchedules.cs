using System;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitDoorWindowCounter
{
    [Transaction(TransactionMode.Manual)]
    public class CmdCreateDoorWindowSchedules : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var doc = commandData.Application.ActiveUIDocument.Document;

            try
            {
                using (var t = new Transaction(doc, "Create Door/Window Schedules (Level+Type+Count)"))
                {
                    t.Start();

                    CreateSchedule(doc, BuiltInCategory.OST_Doors, "SCHEDULE - DOORS by LEVEL+TYPE+COUNT (Auto)");
                    CreateSchedule(doc, BuiltInCategory.OST_Windows, "SCHEDULE - WINDOWS by LEVEL+TYPE+COUNT (Auto)");

                    t.Commit();
                }

                TaskDialog.Show("Done", "Schedules created. Check Project Browser > Schedules/Quantities.");
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.ToString();
                return Result.Failed;
            }
        }

        private static void CreateSchedule(Document doc, BuiltInCategory bic, string name)
        {
            var vs = ViewSchedule.CreateSchedule(doc, new ElementId((long)bic));
            vs.Name = name;

            var def = vs.Definition;

            // Summary schedule => rows will be grouped, and Count becomes meaningful
            def.IsItemized = false;

            // Add Level
            ScheduleFieldId levelFieldId;
            bool hasLevel = TryAddFieldBySchedulableName(doc, def, "Level", out levelFieldId);

            // Add/Ensure Family and Type
            ScheduleFieldId famTypeFieldId;
            bool hasFamType = TryEnsureFieldBySchedulableName(doc, def, "Family and Type", out famTypeFieldId);

            // Add Count column (this is the key in your API version)
            ScheduleFieldId countFieldId;
            bool hasCount =
                TryAddFieldBySchedulableName(doc, def, "Count", out countFieldId) ||
                TryAddFieldBySchedulableName(doc, def, "Qty", out countFieldId) ||
                TryAddFieldBySchedulableName(doc, def, "Quantity", out countFieldId);

            // Sort/Group: Level then Family and Type
            if (hasLevel)
                def.AddSortGroupField(new ScheduleSortGroupField(levelFieldId));

            if (hasFamType)
                def.AddSortGroupField(new ScheduleSortGroupField(famTypeFieldId));

            // Optional: if Count was added, you can try rename heading (won't crash if not supported)
            if (hasCount)
            {
                TrySetColumnHeading(def, countFieldId, "Count");
            }
        }

        private static bool TryAddFieldBySchedulableName(
            Document doc,
            ScheduleDefinition def,
            string targetName,
            out ScheduleFieldId fieldId)
        {
            fieldId = default(ScheduleFieldId);

            // Find schedulable field by its display name (your API requires Document)
            var sf = def.GetSchedulableFields()
                .FirstOrDefault(x => string.Equals(SafeString(x.GetName(doc)), targetName, StringComparison.OrdinalIgnoreCase));

            if (sf == null)
                return false;

            // Avoid duplicates: reuse if exists
            int n = def.GetFieldCount();
            for (int i = 0; i < n; i++)
            {
                var existing = def.GetField(i);
                if (string.Equals(SafeString(existing.GetName()), targetName, StringComparison.OrdinalIgnoreCase))
                {
                    fieldId = existing.FieldId;
                    return true;
                }
            }

            var added = def.AddField(sf); // returns ScheduleField in your API
            fieldId = added.FieldId;
            return true;
        }

        private static bool TryEnsureFieldBySchedulableName(
            Document doc,
            ScheduleDefinition def,
            string targetName,
            out ScheduleFieldId fieldId)
        {
            fieldId = default(ScheduleFieldId);

            int n = def.GetFieldCount();
            for (int i = 0; i < n; i++)
            {
                var f = def.GetField(i);
                if (string.Equals(SafeString(f.GetName()), targetName, StringComparison.OrdinalIgnoreCase))
                {
                    fieldId = f.FieldId;
                    return true;
                }
            }

            return TryAddFieldBySchedulableName(doc, def, targetName, out fieldId);
        }

        private static void TrySetColumnHeading(ScheduleDefinition def, ScheduleFieldId fieldId, string heading)
        {
            try
            {
                int n = def.GetFieldCount();
                for (int i = 0; i < n; i++)
                {
                    var f = def.GetField(i);
                    if (f.FieldId == fieldId)
                    {
                        f.ColumnHeading = heading;
                        return;
                    }
                }
            }
            catch
            {
                // ignore if not supported
            }
        }

        private static string SafeString(string s)
        {
            return s ?? "";
        }
    }
}