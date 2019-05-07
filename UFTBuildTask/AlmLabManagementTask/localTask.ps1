#
# localTask.ps1
#
param(
	[string][Parameter(Mandatory=$true)] $varAlmServ, 
	[string][Parameter(Mandatory=$true)] $varUserName,
	[string][Parameter(Mandatory=$true)] $varDomain,
	[string][Parameter(Mandatory=$true)] $varProject,
	[string][Parameter(Mandatory=$true)] $varTestSet,
	[string][Parameter(Mandatory=$true)] $varTimeslotDuration,
	[string] $varPass,
	[string] $varRunType,
	[string] $varDescription,
	[string] $varEnvironmentConfigurationID,
	[string] $varUseCDA,
	[string] $varDeploymentAction,
	[string] $varDeploymentEnvironmentName,
	[string] $varDeprovisioningAction,
	[string] [Parameter(Mandatory=$false)] $varReportName
)


$uftworkdir = $env:UFT_LAUNCHER
Import-Module $uftworkdir\bin\PSModule.dll

if (-Not $varReportName)
{
	$varReportName = "ALM Lab Management Report"
}
$report = Join-Path $env:UFT_LAUNCHER -ChildPath "res\$($varReportName)"
if (Test-Path $report)
{
	Remove-Item $report
}


if (-Not $varReturnCodeFile)
{
	$varReturnCodeFile = "RunFromAlmLabManagementTestRetCode.txt"
}
$retcodefile = Join-Path $env:UFT_LAUNCHER -ChildPath "res\$($varReturnCodeFile)" 
if (Test-Path $retcodefile)
{
	Remove-Item $retcodefile
}


$CDA1 = [bool]($varUseCDA) 
Invoke-AlmLabManagementTask $varAlmServ $varUserName $varDomain $varProject $varTestSet $varTimeslotDuration $varPass $varRunType $varDescription $varEnvironmentConfigurationID $CDA1 $varDeploymentAction $varDeploymentEnvironmentName $varDeprovisioningAction $varReportName -Verbose

Write-Verbose "Remove temp files"
$results = Join-Path $env:UFT_LAUNCHER -ChildPath "res\*.xml"
Write-Verbose $results

if (Test-Path $report)
{
	#uploads report files to build artifacts
	Write-Host "##vso[task.uploadsummary]$($report)"
}

if (Test-Path $retcodefile)
{
	$content = Get-Content $retcodefile
	[int]$retcode = [convert]::ToInt32($content, 10)
	
	if($retcode -eq 0)
	{
		Write-Host "Test passed"
	}


	if ($retcode -eq 3)
	{
		#writes log messages in case of errors
		Write-Error "Task Failed with message: Closed by user"
		Write-Host "Task Failed with message: Closed by user"
	}
	elseif ($retcode -ne 0)
	{
		Write-Host "Return code: $($retcode)"
		Write-Host "Task failed"
		Write-Error "Task Failed"
	}

	<# Remove-Item $retcodefile #>
}



