#
# localTask.ps1
#
param(
	[string][Parameter(Mandatory=$true)] $varAlmserv, 
	[string][Parameter(Mandatory=$true)] $varUserName,
	[string] $varPass,
	[string][Parameter(Mandatory=$true)] $varDomain,
	[string][Parameter(Mandatory=$true)] $varProject,
	[string] $varRunType,
	[string] $varTestSet,
	[string] $varDescription,
	[string][Parameter(Mandatory=$true)] $varTimeslotDuration,
	[string] $varEnvironmentConfigurationID,
	[string] $varUseCDA,
	[string] $varDeploymentAction,
	[string] $varDeploymentEnvironmentName,
	[string] $varDeprovisioningAction
)

$uftworkdir = $env:UFT_LAUNCHER
Import-Module $uftworkdir\bin\PSModule.dll
Invoke-RunFromAlmTask $varAlmserv $varUserName $varPass $varDomain $varProject $varRunType $varTestSet $varDescription $varTimeslotDuration $varEnvironmentConfigurationID $varUseCDA $varDeploymentAction $varDeploymentEnvironmentName $varDeprovisioningAction -Verbose

$../TaskCommonPart.ps1

