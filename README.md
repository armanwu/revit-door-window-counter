# Revit Door & Window Counter → CSV (Host + Links)

A minimal Revit add-in (single DLL) that counts **Doors** and **Windows** from the **host model + all loaded RVT links**, then exports a **CSV** sorted by:

**Level → TypeMark → Family:Type → Count**

**Command class:** `RevitDoorWindowCounter.CmdCountDWHostAndLinksExportCsv`  
**Output folder:** `Documents\RevitDoorWindowCounter\`

## Manual Install

1. Copy the DLL to a fixed path, e.g.  
   `C:\MyRevitAddins\RevitDoorWindowCounter.dll`
2. Copy the `.addin` file to:  
   `C:\ProgramData\Autodesk\Revit\Addins\20XX\`

## Example .addin (CSV export only)

```xml
<?xml version="1.0" encoding="utf-8" standalone="no"?>
<RevitAddIns>
  <AddIn Type="Command">
    <Name>Count Doors &amp; Windows (Host + Links) → CSV (TypeMark)</Name>
    <Assembly>C:\MyRevitAddins\RevitDoorWindowCounter.dll</Assembly>
    <AddInId>6C2C4A0E-1B5D-4E61-9E6B-2C9F2D4B1A11</AddInId>
    <FullClassName>RevitDoorWindowCounter.CmdCountDWHostAndLinksExportCsv</FullClassName>
    <VendorId>TEST</VendorId>
  </AddIn>
</RevitAddIns>
