\# Revit Door \& Window Counter



A minimal Revit add-in that counts \*\*Doors\*\* and \*\*Windows\*\* (kusen) in the current model, including a \*\*breakdown by Type\*\*.



\## Features



\- Counts Door instances (`OST\_Doors`)

\- Counts Window instances (`OST\_Windows`)

\- Shows count by \*\*Family : Type\*\* (e.g. `Single-Flush : 900x2100 = 12`)



\## Requirements



\- Autodesk Revit (any version that supports .NET Framework add-ins)

\- Visual Studio (recommended) with \*\*.NET desktop development\*\*

\- Revit API references:

&nbsp; - `RevitAPI.dll`

&nbsp; - `RevitAPIUI.dll`



\## Build



1\. Open the solution in \*\*Visual Studio\*\*

2\. Add references to:

&nbsp;  - `RevitAPI.dll`

&nbsp;  - `RevitAPIUI.dll`  

&nbsp;  (from your Revit installation folder, e.g. `C:\\Program Files\\Autodesk\\Revit 20XX\\`)

3\. Set \*\*Copy Local = False\*\* for both references

4\. Build the project



The output DLL will be in:

\- `bin\\Debug\\` or `bin\\Release\\`



\## Install (Manual)



\### 1) Copy DLL to a fixed location



Example:

\- `C:\\MyRevitAddins\\RevitDoorWindowCounter\\RevitDoorWindowCounter.dll`



\### 2) Create the `.addin` manifest



Create a file named `RevitDoorWindowCounter.addin` with the following content

(edit `<Assembly>` path and the Revit year folder `20XX`):



```xml

<?xml version="1.0" encoding="utf-8" standalone="no"?>

<RevitAddIns>

&nbsp; <AddIn Type="Command">

&nbsp;   <Name>Revit Door \& Window Counter</Name>

&nbsp;   <Assembly>C:\\MyRevitAddins\\RevitDoorWindowCounter\\RevitDoorWindowCounter.dll</Assembly>

&nbsp;   <AddInId>8D83C886-B739-4ACD-A9DB-1BC78F315B2C</AddInId>

&nbsp;   <FullClassName>RevitDoorWindowCounter.CmdCountKusen</FullClassName>

&nbsp;   <VendorId>TEST</VendorId>

&nbsp;   <VendorDescription>Learning</VendorDescription>

&nbsp; </AddIn>

</RevitAddIns>

