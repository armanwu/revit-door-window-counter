# Revit Door & Window Counter

A small Revit add-in (single DLL) with **3 commands**:

1) **Count by Level**  
   `RevitDoorWindowCounter.CmdCountDWByLevel`  
   Shows a quick Doors/Windows summary by **Level** (type breakdown depends on your current code).

2) **Create Schedules (Level + Type + Count)**  
   `RevitDoorWindowCounter.CmdCreateDoorWindowSchedules`  
   Creates two schedules:
   - `SCHEDULE - DOORS by LEVEL+TYPE+COUNT (Auto)`
   - `SCHEDULE - WINDOWS by LEVEL+TYPE+COUNT (Auto)`  
   Location: **Project Browser → Schedules/Quantities**

3) **Host + Links → CSV (Type Mark)**
   `RevitDoorWindowCounter.CmdCountDWHostAndLinksExportCsv`  
   Counts Doors/Windows from the **host model + all loaded RVT links**, then exports a **CSV** sorted by:  
   **Level → TypeMark → Family:Type → Count**  
   Output folder: `Documents\RevitDoorWindowCounter\`

## Manual Install

1. Copy the DLL to a fixed path, e.g.  
   `C:\MyRevitAddins\RevitDoorWindowCounter.dll`
2. Copy the `.addin` file to:  
   `C:\ProgramData\Autodesk\Revit\Addins\20XX\`

## Example .addin (register ALL commands)

```xml
<?xml version="1.0" encoding="utf-8" standalone="no"?>
<RevitAddIns>

  <AddIn Type="Command">
    <Name>Count Doors &amp; Windows by Level</Name>
    <Assembly>C:\MyRevitAddins\RevitDoorWindowCounter.dll</Assembly>
    <AddInId>0F7E6B6D-6B77-4F0D-9D7C-3D1B3C7C8F01</AddInId>
    <FullClassName>RevitDoorWindowCounter.CmdCountDWByLevel</FullClassName>
    <VendorId>TEST</VendorId>
  </AddIn>

  <AddIn Type="Command">
    <Name>Create Door/Window Schedules (Level+Type+Count)</Name>
    <Assembly>C:\MyRevitAddins\RevitDoorWindowCounter.dll</Assembly>
    <AddInId>3A7A74D2-7F5D-4F61-9D7F-7C6A0D7E7F21</AddInId>
    <FullClassName>RevitDoorWindowCounter.CmdCreateDoorWindowSchedules</FullClassName>
    <VendorId>TEST</VendorId>
  </AddIn>

  <AddIn Type="Command">
    <Name>Count Doors &amp; Windows (Host + Links) → CSV (TypeMark)</Name>
    <Assembly>C:\MyRevitAddins\RevitDoorWindowCounter.dll</Assembly>
    <AddInId>6C2C4A0E-1B5D-4E61-9E6B-2C9F2D4B1A11</AddInId>
    <FullClassName>RevitDoorWindowCounter.CmdCountDWHostAndLinksExportCsv</FullClassName>
    <VendorId>TEST</VendorId>
  </AddIn>

</RevitAddIns>
