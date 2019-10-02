#
# localTask.ps1
#
param(
	[string][Parameter(Mandatory=$true)] $testPathInput, 
	[string] $timeOutIn
)

$uftworkdir = $env:UFT_LAUNCHER
Import-Module $uftworkdir\bin\PSModule.dll

$retcodefile = Join-Path $env:UFT_LAUNCHER -ChildPath "res\FSTaskRetCode.txt"
if (Test-Path $retcodefile)
{
	Remove-Item $retcodefile
}

Write-Verbose "Remove temp files"
$results = Join-Path $env:UFT_LAUNCHER -ChildPath "res\*.xml"
#Write-Verbose $results

#Remove temp files complited
Get-ChildItem -Path $results | foreach ($_) { Remove-Item $_.fullname }
#Write-Verbose "Remove temp files complited"

Invoke-FSTask $testPathInput $timeOutIn -Verbose 

if (Test-Path $retcodefile)
{
	$content = Get-Content $retcodefile
	[int]$retcode = [convert]::ToInt32($content, 10)

	if ($retcode -eq 3)
	{
		Write-Error "Task Failed with message: Closed by user"
	}
	elseif ($retcode -ne 0)
	{
		Write-Host "Return code: $($retcode)"
		Write-Host "Task failed"
		Write-Error "Task Failed"
	}
	<#Remove-Item $retcodefile#>
}