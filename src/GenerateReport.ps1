[CmdletBinding()]
param($CheckoutDir)
$scriptPath = if($PSScriptRoot -eq $null){"."} else {$PSScriptRoot}

function Fix-CsvFileHeader{
    [CmdletBinding()]
    param($FilePath, $NewHeader)
    $FileContent = Get-Content $FilePath 
    $FileContent[0] = $NewHeader
    $FileContent | Set-Content $FilePath -Encoding UTF8
}

function Get-FilesLOC
{
    <#
        .SYNOPSIS
            Generate calculate LOC for every file inside CheckoutDir
    #>
    [CmdletBinding()]
    param($OutFilePath)
    $clocExePath = "$scriptPath\cloc-1.70.exe"    
    if(-not (Test-Path $clocExePath))
    {
        $PSCmdlet.ThrowTerminatingError("Cannot find file: $clocExePath")
    }
    Write-Verbose "Start colecting LOC statistics"
    Remove-Item $OutFilePath -ErrorAction SilentlyContinue
    & $clocExePath --by-file --csv --skip-uniqueness --out="$OutFilePath" $CheckoutDir | Write-Verbose
    if(-not(Test-Path $OutFilePath))
    {
        $PSCmdlet.ThrowTerminatingError("Cannot create LOC statistics file")
    }

    Fix-CsvFileHeader -FilePath $OutFilePath -NewHeader "language,filename,blank,comment,code"    
    Write-Verbose "Finish colecting LOC statistics"
}

function Get-SvnStatistics(){
     <#
        .SYNOPSIS
            Generate SVN log file for CheckoutDir
    #>
    [CmdletBinding()]
    Param($OutFilePath)
    Write-Verbose "Start collecting SVN log"
    $svnExePath = "c:\Program Files\TortoiseSVN\bin\svn.exe"     
    if(-not (Test-Path $svnExePath))
    {
        $PSCmdlet.ThrowTerminatingError("Cannot find svn.exe")
    }
    Remove-Item $OutFilePath -ErrorAction SilentlyContinue
    & $svnExePath log -v --xml $CheckoutDir | Out-File $OutFilePath -Encoding utf8
    if(-not(Test-Path $OutFilePath))
    {
        $PSCmdlet.ThrowTerminatingError("Cannot collect SVN log file")
    }
    Write-Verbose "Finish collecting SVN log"
}

function Get-SvnModulePath
{
     <#
        .SYNOPSIS
            Get SVN module path for CheckoutDir
    #>
    $svnExePath = "c:\Program Files\TortoiseSVN\bin\svn.exe"   
    $data = (& $svnExePath info $CheckoutDir) -split '\r'    
    $url =""
    $repoRoot = ""
    foreach($attr in $data)
    {
        $parts = $attr.Split(":",2)
        if($parts[0] -eq "Repository Root")
        {
           $repoRoot=  $parts[1].Trim()
        }
        if($parts[0] -eq "URL")
        {
           $url=  $parts[1].Trim()
        }
    }
    $url.Replace($repoRoot,"")+"/"
}

function Set-ScriptEncoding($Encoding){
    $OutputEncoding = New-Object -typename $Encoding
    [Console]::OutputEncoding = New-Object -typename $OutputEncoding

}

function Bundle-Report{
    [CmdletBinding()]
    Param($ClocDataFile, $SvnLogFile, $OutDataFile)
    Write-Verbose "Start bundling report"
    $cleanCheckoutDir = $CheckoutDir -replace "\\","/" 
    & "$scriptPath\CodeTopologyBuilder.exe" $cleanCheckoutDir $(Get-SvnModulePath) $SvnLogFile $ClocDataFile $OutDataFile
    Write-Verbose "Svn module: $(Get-SvnModulePath)"
    $raportTemplate = Get-Content -Path "$scriptPath\report_template.html" -Raw
    $data = Get-Content -Path  $OutDataFile -Raw
    $raportTemplate -replace "#DATA_PLACEHOLDER#", $data | Out-File -FilePath "report.html" -Encoding utf8
    Write-Verbose "Finish bundling report $(Get-Item report.html)"
}

function New-TempDirectory{
    $tempDirectoryPath = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), [System.IO.Path]::GetRandomFileName()) 
    [System.IO.Directory]::CreateDirectory($tempDirectoryPath) | Out-Null  
    $tempDirectoryPath
}

$tmpDir = New-TempDirectory
Write-Verbose $tmpDir
Set-ScriptEncoding -Encoding System.Text.UTF8Encoding
Get-FilesLOC -OutFilePath "$tmpDir\cloc.csv"
Get-SvnStatistics -OutFilePath "$tmpDir\svnlogfile.xml"
Bundle-Report -ClocDataFile "$tmpDir\cloc.csv" -SvnLogFile "$tmpDir\svnlogfile.xml" -OutDataFile "$tmpDir\result.json"
Remove-Item $tmpDir -Recurse -Force