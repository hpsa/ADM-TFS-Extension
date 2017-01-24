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

Function CmdletHasMember($memberName) {
    $publishParameters = (gcm Publish-TestResults).Parameters.Keys.Contains($memberName) 
    return $publishParameters
}

Write-Verbose "Entering script PublishTestResults.ps1"

# Import the Task.Common, Task.Internal and Task.TestResults dll that has all the cmdlets we need
import-module "Microsoft.TeamFoundation.DistributedTask.Task.Internal"
import-module "Microsoft.TeamFoundation.DistributedTask.Task.Common"
import-module "Microsoft.TeamFoundation.DistributedTask.Task.TestResults"

Write-Host "##vso[task.logissue type=warning;TaskName=VSTest]"

$testRunner = "JUnit";
$testResultsFiles = Join-Path $env:UFT_LAUNCHER -ChildPath "res\Result*.xml";
$testRunTitle = "UFT Title";
$platform = "AMD";
$configuration = "bestOfThebest";
$publishRunAttachments = "true";
$mergeResults = "false"

try
{
    if(!$testRunner)
    {        
        throw ("Test runner parameter has to be specified")
    }

    if (!$testResultsFiles)
    {        
        throw ("Test results files parameter has to be specified")
    }

    # check for pattern in testResultsFiles
    if ($testResultsFiles.Contains("*") -or $testResultsFiles.Contains("?"))
    {
        Write-Verbose "Pattern found in testResultsFiles parameter."
        Write-Verbose "Find-Files -SearchPattern $testResultsFiles"
        $matchingTestResultsFiles = Find-Files -SearchPattern $testResultsFiles
        Write-Verbose "matchingTestResultsFiles = $matchingTestResultsFiles"
    }
    else
    {
        Write-Verbose "No Pattern found in testResultsFiles parameter."
        $matchingTestResultsFiles = ,$testResultsFiles
    }

    if (!$matchingTestResultsFiles)
    {
        Write-Warning ("No test result files were found using search pattern.")
    }
    else
    {
        $publishResultsOption = Convert-String $publishRunAttachments Boolean
        $mergeResults = Convert-String $mergeTestResults Boolean
        Write-Verbose "Calling Publish-TestResults"
        
        $publishRunLevelAttachmentsExists = CmdletHasMember "PublishRunLevelAttachments"
        $runTitleMemberExists = CmdletHasMember "RunTitle"
	    if(!($runTitleMemberExists))
	    {
		    if(!([string]::IsNullOrWhiteSpace($testRunTitle)))
		    {
			    Write-Warning "Update the build agent to be able to use the custom run title feature."
		    }
		    if($publishRunLevelAttachmentsExists)
		    {
			    Publish-TestResults -TestRunner $testRunner -TestResultsFiles $matchingTestResultsFiles -MergeResults $mergeResults -Platform $platform -Configuration $configuration -Context $distributedTaskContext -PublishRunLevelAttachments $publishResultsOption
		    }
		    else 
		    {
			    if(!$publishResultsOption)
			    {
			        Write-Warning "Update the build agent to be able to opt out of test run attachment upload." 
			    }
			    Publish-TestResults -TestRunner $testRunner -TestResultsFiles $matchingTestResultsFiles -MergeResults $mergeResults -Platform $platform -Configuration $configuration -Context $distributedTaskContext
		    }
	    }
	    else
	    {
		    if($publishRunLevelAttachmentsExists)
		    {
			    Publish-TestResults -TestRunner $testRunner -TestResultsFiles $matchingTestResultsFiles -MergeResults $mergeResults -Platform $platform -Configuration $configuration -Context $distributedTaskContext -PublishRunLevelAttachments $publishResultsOption -RunTitle $testRunTitle
		    }
		    else 
		    {
			    if(!$publishResultsOption)
			    {
			        Write-Warning "Update the build agent to be able to opt out of test run attachment upload." 
			    }
			    Publish-TestResults -TestRunner $testRunner -TestResultsFiles $matchingTestResultsFiles -MergeResults $mergeResults -Platform $platform -Configuration $configuration -Context $distributedTaskContext -RunTitle $testRunTitle
		    }
	    }
    }
}
catch
{
    Write-Host "##vso[task.logissue type=error;code=" $_.Exception.Message ";TaskName=VSTest]"
    throw
}

Write-Verbose "Remove temp files"
$results = Join-Path $env:UFT_LAUNCHER -ChildPath "res"
Write-Verbose $results

Get-ChildItem -Path $results -Include * | remove-Item

Write-Verbose "Remove temp files complited"

