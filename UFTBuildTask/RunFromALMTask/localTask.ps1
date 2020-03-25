 #
# localTask.ps1
#
param(
	[string][Parameter(Mandatory=$true)] $varAlmserv, 
	[string][Parameter(Mandatory=$false)] $varSSOEnabled,
	[string] $varClientID,
	[string] $varApiKeySecret,
	[string] $varUserName,
	[string] $varPass,
	[string] $varDomain,
	[string] $varProject,
	[string] $varTestsets,
	[string] $varTimeout,
	[string] $varReportName,
	[string] $runMode,
	[string] $testingToolHost
)


$uftworkdir = $env:UFT_LAUNCHER
Import-Module $uftworkdir\bin\PSModule.dll
if (-Not $varReportName)
{
	$varReportName = "ALM Execution Report"
}
$report = Join-Path $env:UFT_LAUNCHER -ChildPath "res\$($varReportName)"

if (Test-Path $report)
{
	Remove-Item $report
}

if (-Not $varReturnCodeFile)
{
	$varReturnCodeFile = "TestRunReturnCode.txt"
}
$retcodefile = Join-Path $env:UFT_LAUNCHER -ChildPath "res\$($varReturnCodeFile)"
if (Test-Path $retcodefile)
{
	Remove-Item $retcodefile
}

Write-Verbose "Remove temp files"
$results = Join-Path $env:UFT_LAUNCHER -ChildPath "res\*.xml"
#Write-Verbose $results

 Get-ChildItem -Path $results | foreach ($_) { Remove-Item $_.fullname }
 #Write-Verbose "Remove temp files complited" 
 

Invoke-RunFromAlmTask $varAlmserv $varSSOEnabled $varClientID $varApiKeySecret $varUserName $varPass $varDomain $varProject $varTestsets $varTimeout $varReportName $runMode $testingToolHost -Verbose

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
		Write-Host "Return code: $($retcode)"
		Write-Host "Tests passed"
	}

	if($retcode -eq -1)
	{
		Write-Host "Return code: $($retcode)"
		Write-Host "Task failed"
		<#Write-Error "Task Failed"#>
	}

	if($retcode -eq -2)
	{
		Write-Host "Return code: $($retcode)"
		Write-Host "Job unstable"
	}

	if ($retcode -eq -3)
	{
		#writes log messages in case of errors
		Write-Host "Return code: $($retcode)"
		Write-Error "Task Failed with message: Closed by user"
	}
		
	<#Remove-Item $retcodefile#>
}