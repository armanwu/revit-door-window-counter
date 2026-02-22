# Revit Door & Window Counter

Two commands in one DLL:

- **Count by Level** (`RevitDoorWindowCounter.CmdCountDWByLevel`)  
  Shows Doors/Windows totals grouped by **Level** (and type breakdown if enabled in code).

- **Create Schedules** (`RevitDoorWindowCounter.CmdCreateDoorWindowSchedules`)  
  Creates schedules grouped by **Level + Family and Type + Count**.

Schedules appear in: **Project Browser → Schedules/Quantities**

## Install (Manual)

1. Copy DLL to a fixed path, e.g. `C:\MyRevitAddins\RevitDoorWindowCounter.dll`
2. Copy `.addin` to `C:\ProgramData\Autodesk\Revit\Addins\20XX\`

Example `.addin` (register BOTH commands):

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

</RevitAddIns>
