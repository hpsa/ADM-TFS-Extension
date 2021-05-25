#
# localTask.ps1
#

$varAlmServ = Get-VstsInput -Name 'varAlmserv' -Require
$varUserName = Get-VstsInput -Name 'varUserName' -Require
$varPass = Get-VstsInput -Name 'varPass'
$varDomain = Get-VstsInput -Name 'varDomain' -Require
$varProject = Get-VstsInput -Name 'varProject' -Require
$varRunType = Get-VstsInput -Name 'varRunType'
$varTestSet = Get-VstsInput -Name 'varTestSet' -Require
$varDescription = Get-VstsInput -Name 'varDescription'
$varTimeslotDuration = Get-VstsInput -Name 'varTimeslotDuration' -Require
$varEnvironmentConfigurationID = Get-VstsInput -Name 'varEnvironmentConfigurationID'
$varReportName = Get-VstsInput -Name 'varReportName'
$varUseCDA = Get-VstsInput -Name 'varUseCDA'
$varDeploymentAction = Get-VstsInput -Name 'varDeploymentAction'
$varDeploymentEnvironmentName = Get-VstsInput -Name 'varDeploymentEnvironmentName'
$varDeprovisioningAction = Get-VstsInput -Name 'varDeprovisioningAction'


$uftworkdir = $env:UFT_LAUNCHER
$buildNumber = $env:BUILD_BUILDNUMBER

Import-Module $uftworkdir\bin\PSModule.dll

# delete old "ALM Lab Management Report" file and create a new one
if (-Not $varReportName)
{
	$varReportName = "ALM Lab Management Report"
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

#junit report file 
$outputJUnitFile = Join-Path $uftworkdir -ChildPath ("res\Report_" + $buildNumber + "\Failed tests")

# create return code file
#if (-Not $varReturnCodeFile)
#{
#	$varReturnCodeFile = "TestRunReturnCode.txt"
#}
#$retcodefile = Join-Path $env:UFT_LAUNCHER -ChildPath ("res\Report_" + $buildNumber + "\$($varReturnCodeFile)") 
$retcodefile = Join-Path $env:UFT_LAUNCHER -ChildPath ("res\Report_" + $buildNumber + "\TestRunReturnCode.txt")
if (Test-Path $retcodefile)
{
	Remove-Item $retcodefile
}

# remove temporary files complited
$results = Join-Path $env:UFT_LAUNCHER -ChildPath ("res\Report_" + $buildNumber +"\*.xml")
 #Get-ChildItem -Path $results | foreach ($_) { Remove-Item $_.fullname }


$CDA1 = [bool]($varUseCDA) 
Invoke-AlmLabManagementTask $varAlmServ $varUserName $varPass $varDomain $varProject $varRunType $varTestSet $varDescription $varTimeslotDuration $varEnvironmentConfigurationID $varReportName $CDA1 $varDeploymentAction $varDeploymentEnvironmentName $varDeprovisioningAction $buildNumber -Verbose

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
		#writes log messages in case of errors
		Write-Error "Task Failed with message: Closed by user"
	}
	elseif ($retcode -ne 0)
	{
		Write-Error "Task Failed"
	}
}



