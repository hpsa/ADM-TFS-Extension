#
# localTask.ps1
#
param(
	[string][Parameter(Mandatory=$true)] $varAlmserv, 
	[string][Parameter(Mandatory=$true)] $varUserName,
	[string] $varPass,
	[string][Parameter(Mandatory=$true)] $varDomain,
	[string][Parameter(Mandatory=$true)] $varProject,
	[string] $varTestsets,
	[string] $varTimeout,
	[string] $runMode,
	[string] $testingToolHost
)

$uftworkdir = $env:UFT_LAUNCHER
Import-Module $uftworkdir\bin\PSModule.dll
Invoke-RunFromAlmTask $varAlmserv $varUserName $varPass $varDomain $varProject $runMode $testingToolHost $varTimeout $varTestsets -Verbose

$../TaskCommonPart.ps1

