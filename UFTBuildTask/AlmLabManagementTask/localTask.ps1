#
# localTask.ps1
#
param(
	[string][Parameter(Mandatory=$true)] $varAlmserv, 
	[string][Parameter(Mandatory=$true)] $varUserName,
	[string][Parameter(Mandatory=$true)] $varDomain,
	[string][Parameter(Mandatory=$true)] $varProject,
	[string][Parameter(Mandatory=$true)] $varTestSet,
	[string][Parameter(Mandatory=$true)] $varTimeslotDuration,
	[string] $varPass,
	[string] $varRunType,
	[string] $varEnvironmentConfigurationID,
	[string] $varDescription,
	[string] $varUseCDA,
	[string] $varDeploymentAction,
	[string] $varDeploymentEnvironmentName,
	[string] $varDeprovisioningAction,
	[string][Parameter(Mandatory=$false)] $varReportName
)

$uftworkdir = $env:UFT_LAUNCHER

$stdout = "$uftworkdir\temp_build.log"
$stderr = "$uftworkdir\temp_error_build.log"
$jar = """$uftworkdir\bin\hpe.application.automation.tfs.almrestrunner-1.0-jar-with-dependencies.jar"""

if (-Not $varReportName)
{
	$varReportName = "ALM Lab Management Report"
}
$args = "-jar $jar AlmLabManagement  ""serv:$varAlmserv"" ""user:$varUserName"" ""domain:$varDomain"" ""project:$varProject"" ""testSet:$varTestSet"" ""timeSlotDuration:$varTimeslotDuration"" ""pass:$varPass"" ""runType:$varRunType"" ""envconfID:$varEnvironmentConfigurationID"" ""desc:$varDescription"" ""useCDA:$varUseCDA"" ""deploymentAction:$varDeploymentAction"" ""depEnvName:$varDeploymentEnvironmentName"" ""deprovisioningAction:$varDeprovisioningAction"" ""repname:$varReportName"""

echo $args

$report = Join-Path $env:UFT_LAUNCHER -ChildPath "res\$($varReportName)"
if (Test-Path $report)
{
	Remove-Item $report
}

$process = (Start-Process java -ArgumentList $args -RedirectStandardOutput $stdout -RedirectStandardError $stderr -PassThru -Wait)

if ($process.ExitCode -ne 0)
{
	$content = [IO.File]::ReadAllText($stdout)
	Write-Error ($content)
	$content = [IO.File]::ReadAllText($stderr)
	Write-Error ($content)
}
else
{
	Get-Content $stdout
	Get-Content $stderr
}

if (Test-Path $report)
{
	Write-Host "##vso[task.uploadsummary]"$report
}

if (Test-Path $report)
{
	Remove-Item $stdout
}
if (Test-Path $report)
{
	Remove-Item $stderr
}
