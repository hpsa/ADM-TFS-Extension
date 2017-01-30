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
	[string] $varDeprovisioningAction
)

$uftworkdir = $env:UFT_LAUNCHER

$stdout = "$uftworkdir\temp_build.log"
$stderr = "$uftworkdir\temp_error_build.log"
$jar = """$uftworkdir\bin\hpe.application.automation.tfs.almrestrunner-1.0-jar-with-dependencies.jar"""

$args = "-jar $jar AlmLabManagement  ""serv:$varAlmserv"" ""user:$varUserName"" ""domain:$varDomain"" ""project:$varProject"" ""testSet:$varTestSet"" ""timeSlotDuration:$varTimeslotDuration"" ""pass:$varPass"" ""runType:$varRunType"" ""envconfID:$varEnvironmentConfigurationID"" ""desc:$varDescription"" ""useCDA:$varUseCDA"" ""deploymentAction:$varDeploymentAction"" ""depEnvName:$varDeploymentEnvironmentName"" ""deprovisioningAction:$varDeprovisioningAction"""

echo $args

Start-Process "C:\Program Files\Java\jre1.8.0_111\bin\java.exe" -ArgumentList $args -RedirectStandardOutput $stdout -RedirectStandardError $stderr -Wait

Get-Content $stdout
Get-Content $stderr

Remove-Item $stdout
Remove-Item $stderr
