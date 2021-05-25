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
$buildNumber = $env:BUILD_BUILDNUMBER

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
$summaryReport = Join-Path $env:UFT_LAUNCHER -ChildPath ("res\Report_" + $buildNumber + "\UFT Report")

#run status summary Report
$runStatus = Join-Path $env:UFT_LAUNCHER -ChildPath ("res\Report_" + $buildNumber + "\Run status summary")

# delete old "TestRunReturnCode" file and create a new one
$retcodefile = Join-Path $env:UFT_LAUNCHER -ChildPath ("res\Report_" + $buildNumber + "\TestRunReturnCode.txt")

# remove temporary files complited
$results = Join-Path $env:UFT_LAUNCHER -ChildPath ("res\Report_" + $buildNumber +"\*.xml")

#junit report file 
$outputJUnitFile = Join-Path $uftworkdir -ChildPath ("res\Report_" + $buildNumber + "\Failed tests")

Invoke-RunFromAlmTask $varAlmserv $varSSOEnabled $varClientID $varApiKeySecret $varUserName $varPass $varDomain $varProject $varTestsets $varTimeout $varReportName $runMode $testingToolHost $buildNumber -Verbose

# create summary UFT report
if (Test-Path $summaryReport)
{
	#uploads report files to build artifacts
	Write-Host "##vso[task.uploadsummary]$($summaryReport)" | ConvertTo-Html
}

if (Test-Path $runStatus)
{
	#uploads report files to build artifacts
	Write-Host "##vso[task.uploadsummary]$($runStatus)" | ConvertTo-Html
}

# upload junit report
if (Test-Path $outputJUnitFile)
{
	#uploads report files to build artifacts
	Write-Host "##vso[task.uploadsummary]$($outputJUnitFile)" | ConvertTo-Html
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
		Write-Error "Task Failed"
	}
}