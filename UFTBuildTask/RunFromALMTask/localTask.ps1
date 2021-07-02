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
$attemptNumber = $env:SYSTEM_STAGEATTEMPT
[int]$rerunIdx = [convert]::ToInt32($attemptNumber, 10) - 1
$resDir = Join-Path $uftworkdir -ChildPath "res\Report_$buildNumber"

Import-Module $uftworkdir\bin\PSModule.dll

# delete old "ALM Execution Report" file and create a new one
if (-Not $varReportName) {
	$varReportName = "ALM Execution Report"
}

$report = "$resDir\$varReportName"

if (Test-Path $report) {
	Remove-Item $report
}

$uftReport = "$resDir\UFT Report"
$runSummary = "$resDir\Run Summary"
$retcodefile = "$resDir\TestRunReturnCode.txt"
$failedTests = "$resDir\Failed Tests"

if ($rerunIdx) {
	Write-Host "Rerun attempt = $rerunIdx"
}

Invoke-RunFromAlmTask $varAlmserv $varSSOEnabled $varClientID $varApiKeySecret $varUserName $varPass $varDomain $varProject $varTestsets $varTimeout $varReportName $runMode $testingToolHost $buildNumber -Verbose

#---------------------------------------------------------------------------------------------------
# uploads report files to build artifacts
# upload and display Run Summary
if (Test-Path $runSummary) {
	if ($rerunIdx) {
		Write-Host "##vso[task.addattachment type=Distributedtask.Core.Summary;name=Run Summary (rerun $rerunIdx);]$runSummary"
	} else {
		Write-Host "##vso[task.uploadsummary]$runSummary"
	}
}

# upload and display UFT report
if (Test-Path $uftReport) {
	if ($rerunIdx) {
		Write-Host "##vso[task.addattachment type=Distributedtask.Core.Summary;name=UFT Report (rerun $rerunIdx);]$uftReport"
	} else {
		Write-Host "##vso[task.uploadsummary]$uftReport"
	}
}

# upload and display Failed Tests
if (Test-Path $failedTests) {
	if ($rerunIdx) {
		Write-Host "##vso[task.addattachment type=Distributedtask.Core.Summary;name=Failed Tests (rerun $rerunIdx);]$failedTests"
	} else {
		Write-Host "##vso[task.uploadsummary]$failedTests"
	}
}

# read return code
if (Test-Path $retcodefile) {
	$content = Get-Content $retcodefile
	if ($content) {
		$sep = [Environment]::NewLine
		$option = [System.StringSplitOptions]::RemoveEmptyEntries
		$arr = $content.Split($sep, $option)
		[int]$retcode = [convert]::ToInt32($arr[-1], 10)
	
		if ($retcode -eq 0) {
			Write-Host "Test passed"
		}

		if ($retcode -eq -3) {
			Write-Error "Task Failed with message: Closed by user"
		} elseif ($retcode -ne 0) {
			Write-Error "Task Failed"
		}
	} else {
		Write-Error "The file [$retcodefile] is empty!"
	}
}
