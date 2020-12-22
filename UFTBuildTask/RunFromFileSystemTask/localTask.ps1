#
# localTask.ps1
#


param()
$testPathInput = Get-VstsInput -Name 'testPathInput' -Require
$timeOutIn = Get-VstsInput -Name 'timeOutIn'
$uploadArtifact = Get-VstsInput -Name 'uploadArtifact' -Require
$artifactType = Get-VstsInput -Name 'artifactType'
$reportFileName = Get-VstsInput -Name 'reportFileName'

$uftworkdir = $env:UFT_LAUNCHER
Import-Module $uftworkdir\bin\PSModule.dll


# delete old "UFT Report" file and create a new one
$summaryReport = Join-Path $env:UFT_LAUNCHER -ChildPath "res\UFT Report"
if (Test-Path $summaryReport)
{
	Remove-Item $summaryReport
}


# delete old "TestRunReturnCode" file and create a new one
$retcodefile = Join-Path $env:UFT_LAUNCHER -ChildPath "res\TestRunReturnCode.txt"
if (Test-Path $retcodefile)
{
	Remove-Item $retcodefile
}

# remove temporary files complited
$results = Join-Path $env:UFT_LAUNCHER -ChildPath "res\*.xml"

# Get-ChildItem -Path $results | foreach ($_) { Remove-Item $_.fullname }

if ($reportFileName)
{
	$reportFileName = $reportFileName + '_' +  $env:BUILD_BUILDNUMBER
} else
{
	$reportFileName = "FileSystemExecutionReport_" + $env:BUILD_BUILDNUMBER
}

$archiveName = "Report_" + $env:BUILD_BUILDNUMBER

Invoke-FSTask $testPathInput $timeOutIn $uploadArtifact $artifactType $env:STORAGE_ACCOUNT $env:CONTAINER $reportFileName $archiveName -Verbose 

$testPathReportInput = Join-Path $testPathInput -ChildPath "Report\run_results.html"

if($uploadArtifact -eq "yes")
{
# connect to Azure account
Connect-AzAccount

# get resource group
$group = $env:RESOURCE_GROUP
$resourceGroup = Get-AzResourceGroup -Name "$($group)"
$groupName = $resourceGroup.ResourceGroupName

# get storage account
$account = $env:STORAGE_ACCOUNT
$storageAccount =  Get-AzStorageAccount -ResourceGroupName "$($groupName)" -Name  "$($account)"

# get storage context
$storageContext = $storageAccount.Context

# get storage container
$container = $env:CONTAINER

if ($artifactType -eq "onlyReport") #upload only report
{
	$artifact = $reportFileName + ".html"
	# upload resource to container
	Set-AzStorageBlobContent -Container "$($container)" -File $testPathReportInput -Blob  $artifact -Context $storageContext
	
} elseif ($artifactType -eq "onlyArchive") #upload only archive
{
	#archive report folder
	$artifact = "Report_" + $env:BUILD_BUILDNUMBER + ".zip"
	
	$sourceFolder = Join-Path $testPathInput -ChildPath "Report"
	$destinationFolder = Join-Path $testPathInput -ChildPath $artifact
	Compress-Archive -Path $sourceFolder -DestinationPath $destinationFolder
	
	# upload resource to container
	Set-AzStorageBlobContent -Container "$($container)" -File $destinationFolder -Blob  $artifact -Context $storageContext

} else { #upload both report and archive
	$artifact = $reportFileName + ".html"
	# upload resource to container
	Set-AzStorageBlobContent -Container "$($container)" -File $testPathReportInput -Blob $artifact -Context $storageContext

	#archive report folder	
	$artifact = "Report_" + $env:BUILD_BUILDNUMBER + ".zip"
	$sourceFolder = Join-Path $testPathInput -ChildPath "Report"
	$destinationFolder = Join-Path $testPathInput -ChildPath $artifact
	Compress-Archive -Path $sourceFolder -DestinationPath $destinationFolder

	# upload resource to container
	Set-AzStorageBlobContent -Container "$($container)" -File $destinationFolder -Blob  $artifact -Context $storageContext
}
}

# create summary UFT report
if (Test-Path $summaryReport)
{
	#uploads report files to build artifacts
	Write-Host "##vso[task.uploadsummary]$($summaryReport)" | ConvertTo-Html
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
		Write-Error "Task Failed with message: Closed by user"
	}
	elseif ($retcode -ne 0)
	{
		Write-Host "Return code: $($retcode)"
		Write-Host "Task failed"
		Write-Error "Task Failed"
	}
}