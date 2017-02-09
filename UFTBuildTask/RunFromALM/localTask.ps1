#
# localTask.ps1
#
param(
	[string][Parameter(Mandatory=$true)] $varAlmserv, 
	[string][Parameter(Mandatory=$true)] $varUserName,
	[string] $varPass,
	[string][Parameter(Mandatory=$true)] $varDomain,
	[string][Parameter(Mandatory=$true)] $varProject,
	[string] $varTestsets,
	[string] $varTimeout,
	[string] $runMode,
	[string] $testingToolHost,
	[string][Parameter(Mandatory=$false)] $varReportName
)

$uftworkdir = $env:UFT_LAUNCHER
Import-Module $uftworkdir\bin\PSModule.dll
if (-Not $varReportName)
{
	$varReportName = "ALM Execution Report"
}
$report = Join-Path $env:UFT_LAUNCHER -ChildPath "res\$($varReportName)"
if (Test-Path $report)
{
	Remove-Item $report
}

$output = Invoke-RunFromAlmTask $varAlmserv $varUserName $varPass $varDomain $varProject $runMode $testingToolHost $varTimeout $varTestsets $varReportName -Verbose

$arr = @($output)
$count = $arr.Count

for ($i = 0; $i -lt $count - 1; $i++) 
{ 
	Write-Host $arr[$i]; 
}

Write-Verbose "Remove temp files"
$results = Join-Path $env:UFT_LAUNCHER -ChildPath "res\*.xml"
Write-Verbose $results
Get-ChildItem -Path $results | foreach ($_) { Remove-Item $_.fullname }
Write-Verbose "Remove temp files complited"

if (Test-Path $report)
{
	Write-Host "##vso[task.uploadsummary]$($report)"
}

$retcode = $arr[$count - 1]

if ($retcode -eq 3)
{
	Write-Error "Task Failed with message: Closed by user"
}
elseif ($retcode -ne 0)
{
	Write-Host "Return code: $($retcode)"
	Write-Error "Task Failed"
}
