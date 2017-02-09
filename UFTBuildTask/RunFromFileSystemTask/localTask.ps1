#
# localTask.ps1
#
param(
	[string][Parameter(Mandatory=$true)] $testPathInput, 
	[string] $timeOutIn
)

$uftworkdir = $env:UFT_LAUNCHER
Import-Module $uftworkdir\bin\PSModule.dll

$output = Invoke-FSTask $testPathInput $timeOutIn -Verbose 

$arr = @($output)
$count = $arr.Count

for ($i = 0; $i -lt $count - 1; $i++) 
{ 
	Write-Host $arr[$i]; 
}

Write-Verbose "Remove temp files"
$results = Join-Path $env:UFT_LAUNCHER -ChildPath "res\*.xml"
Write-Verbose $results
Get-ChildItem -Path $results | foreach ($_) { Remove-Item $_.fullname }
Write-Verbose "Remove temp files complited"

$retcode = $arr[$count - 1]

if ($retcode -eq 3)
{
	Write-Error "Task Failed with message: Closed by user"
}
elseif ($retcode -ne 0)
{
	Write-Host "Return code: $($retcode)"
	Write-Error "Task Failed"
}
