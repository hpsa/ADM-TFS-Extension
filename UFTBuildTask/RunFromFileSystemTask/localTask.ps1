#
# localTask.ps1
#
param(
	[string][Parameter(Mandatory=$true)] $testPathInput, 
	[string] $timeOutIn
)

$uftworkdir = $env:UFT_LAUNCHER
Import-Module $uftworkdir\bin\PSModule.dll

Invoke-FSTask $testPathInput $timeOutIn -Verbose 

Write-Host "##vso[task.logissue type=warning;TaskName=VSTest]"

Write-Verbose "Remove temp files"
$results = Join-Path $env:UFT_LAUNCHER -ChildPath "res"
Get-ChildItem -Path $results -Include * | remove-Item

Write-Verbose "Remove temp files complited"


