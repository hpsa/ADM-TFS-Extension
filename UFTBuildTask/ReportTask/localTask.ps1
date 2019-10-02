#
# localTask.ps1
#
param(
	[string][Parameter(Mandatory=$false)] $resultsFileName,
	[string][Parameter(Mandatory=$false)] $uftWorkingFolder
	)

$uftworkdir = $env:UFT_LAUNCHER
Import-Module $uftworkdir\bin\PSModule.dll


$results = Join-Path $env:UFT_LAUNCHER -ChildPath "res\*.xml"
$imagePath = Join-Path $env:UFT_LAUNCHER -ChildPath "res\passed.png"

$summaryReport = Join-Path $env:UFT_LAUNCHER -ChildPath "res\UFT Report"
if (Test-Path $summaryReport)
{
	Remove-Item $summaryReport
}

$files = Get-ChildItem -Path $results
foreach($resultFile in $files)
{ 
	$resultsFileName = $resultFile.fullname
	Write-Host $resultsFileName
}

$uftWorkingFolder = $env:UFT_LAUNCHER

Invoke-ReportTask $resultsFileName $uftWorkingFolder -Verbose

if (Test-Path $summaryReport)
{
	#uploads report files to build artifacts
	Write-Host "##vso[task.uploadsummary]$($summaryReport)" | ConvertTo-Html
	
}

