param(
    [Switch]$CreateArchive,  
    [Parameter()][ValidateSet('Debug','Release')] $Version
    )
$outDir = "dist"
$outAppDir = "dist\CodeTopology"
Remove-Item $outDir -Force -Recurse -ErrorAction SilentlyContinue
New-Item $outAppDir -ItemType Directory -ErrorAction SilentlyContinue

Copy-Item "GenerateReport.ps1" -Destination $outAppDir
Copy-Item "cloc-1.70.exe" -Destination $outAppDir
Copy-Item "report_template.html" -Destination $outAppDir
Copy-Item "CodeTopologyBuilder\bin\$Version\*" -Destination $outAppDir -Recurse
if($CreateArchive){
    Compress-Archive -Path $outAppDir -DestinationPath "$outDir\CodeTopology.zip"
}
