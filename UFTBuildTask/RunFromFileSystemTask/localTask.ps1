#
# localTask.ps1
#
param(
	[string][Parameter(Mandatory=$true)] $testPathInput, 
	[string] $timeOutIn
)

$uftworkdir = $env:UFT_LAUNCHER
Import-Module $uftworkdir\bin\PSModule.dll

# delete old "UFT Report" file and create a new one
$summaryReport = Join-Path $env:UFT_LAUNCHER -ChildPath "res\UFT Report"
if (Test-Path $summaryReport)
{
	Remove-Item $summaryReport
}

# delete old "TestRunReturnCode" file and create a new one
$retcodefile = Join-Path $env:UFT_LAUNCHER -ChildPath "res\TestRunReturnCode.txt"
if (Test-Path $retcodefile)
{
	Remove-Item $retcodefile
}

# remove temporary files complited
$results = Join-Path $env:UFT_LAUNCHER -ChildPath "res\*.xml"
Get-ChildItem -Path $results | foreach ($_) { Remove-Item $_.fullname }


Invoke-FSTask $testPathInput $timeOutIn -Verbose 

# create summary UFT report
if (Test-Path $summaryReport)
{
	#uploads report files to build artifacts
	Write-Host "##vso[task.uploadsummary]$($summaryReport)" | ConvertTo-Html
}

# read return code
if (Test-Path $retcodefile)
{
	$content = Get-Content $retcodefile
	[int]$retcode = [convert]::ToInt32($content, 10)
	
	if($retcode -eq 0)
	{
		Write-Host "Test passed"
	}

	if ($retcode -eq -3)
	{
		Write-Error "Task Failed with message: Closed by user"
	}
	elseif ($retcode -ne 0)
	{
		Write-Host "Return code: $($retcode)"
		Write-Host "Task failed"
		Write-Error "Task Failed"
	}
}