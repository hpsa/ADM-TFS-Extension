#
# localTask.ps1
#
param(
	[string][Parameter(Mandatory=$true)] $testPathInput, 
	[string] $timeOutIn,
	[string] $testsResult,
	[string] $pollInterval,
	[string] $executeTimeout,
	[string] $ignoreErrors,
	[string] $useMC,
	[string] $mcUserNameIn,
	[string] $mcPasswordIn,
	[string] $useProxy,
	[string] $proxyAddress,
	[string] $chkAuth,
	[string] $proxyUserName,
	[string] $proxyPassword,
	[string] $deviceId,
	[string] $OS,
	[string] $manufacturerAndModel,
	[string] $targetLab,
	[string] $extraApps,
	[string] $launchApplicationName,
	[string] $autPackaging,
	[string] $autActions,
	[string] $deviceMetrics,
	[string] $testDefinition,
	[string] $lableDevice
)

$uftworkdir = $env:UFT_LAUNCHER
Import-Module $uftworkdir\bin\PSModule.dll
Invoke-FSTask $testPathInput $timeOutIn -Verbose -Debug

$../TaskCommonPart.ps1

