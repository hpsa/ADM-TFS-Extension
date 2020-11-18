 #
# localTask.ps1
#

$varAlmserv = Get-VstsInput -Name 'varAlmserv' -Require
$varSSOEnabled = Get-VstsInput -Name 'varSSOEnabled'
$varClientID = Get-VstsInput -Name 'varClientID'
$varApiKeySecret = Get-VstsInput -Name 'varApiKeySecret'
$varUserName = Get-VstsInput -Name 'varUsername'
$varPass = Get-VstsInput -Name 'varPass'
$varDomain = Get-VstsInput -Name 'varDomain' -Require
$varProject = Get-VstsInput -Name 'varProject' -Require
$varTestsets = Get-VstsInput -Name 'varTestsets' -Require
$varTimeout = Get-VstsInput -Name 'varTimeout'
$varReportName = Get-VstsInput -Name 'varReportName'
$runMode = Get-VstsInput -Name 'runMode'
$testingToolHost = Get-VstsInput -Name 'testingToolHost'

$uftworkdir = $env:UFT_LAUNCHER

Import-Module $uftworkdir\bin\PSModule.dll

# delete old "ALM Execution Report" file and create a new one
if (-Not $varReportName)
{
	$varReportName = "ALM Execution Report"
}

$report = Join-Path $env:UFT_LAUNCHER -ChildPath "res\$($varReportName)"

if (Test-Path $report)
{
	Remove-Item $report
}

# delete old "UFT Report" file and create a new one
$summaryReport = Join-Path $env:UFT_LAUNCHER -ChildPath "res\UFT Report"
if (Test-Path $summaryReport)
{
	Remove-Item $summaryReport
}

# delete old "TestRunReturnCode" file and create a new one
if (-Not $varReturnCodeFile)
{
	$varReturnCodeFile = "TestRunReturnCode.txt"
}
$retcodefile = Join-Path $env:UFT_LAUNCHER -ChildPath "res\$($varReturnCodeFile)"
if (Test-Path $retcodefile)
{
	Remove-Item $retcodefile
}

# remove temporary files complited
$results = Join-Path $env:UFT_LAUNCHER -ChildPath "res\*.xml"
#Get-ChildItem -Path $results | foreach ($_) { Remove-Item $_.fullname }


Invoke-RunFromAlmTask $varAlmserv $varSSOEnabled $varClientID $varApiKeySecret $varUserName $varPass $varDomain $varProject $varTestsets $varTimeout $varReportName $runMode $testingToolHost -Verbose

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