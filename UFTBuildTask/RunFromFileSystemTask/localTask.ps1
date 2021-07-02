#
# localTask.ps1
#

param()
$testPathInput = Get-VstsInput -Name 'testPathInput' -Require
$timeOutIn = Get-VstsInput -Name 'timeOutIn'
$uploadArtifact = Get-VstsInput -Name 'uploadArtifact' -Require
$artifactType = Get-VstsInput -Name 'artifactType'
$rptFileName = Get-VstsInput -Name 'reportFileName'

$uftworkdir = $env:UFT_LAUNCHER
$buildNumber = $env:BUILD_BUILDNUMBER
$pipelineName = $env:BUILD_DEFINITIONNAME
$attemptNumber = $env:SYSTEM_STAGEATTEMPT
[int]$rerunIdx = [convert]::ToInt32($attemptNumber, 10) - 1
$resDir = Join-Path $uftworkdir -ChildPath "res\Report_$buildNumber"

Import-Module $uftworkdir\bin\PSModule.dll

#---------------------------------------------------------------------------------------------------

function UploadArtifactToAzureStorage($storageContext, $container, $testPathReportInput, $artifact) {
	#upload artifact to storage container
	Set-AzStorageBlobContent -Container "$($container)" -File $testPathReportInput -Blob $artifact -Context $storageContext
}

function ArchiveReport($artifact, $rptFolder) {
	if (Test-Path $rptFolder) {
		$fullPathZipFile = Join-Path $rptFolder -ChildPath $artifact
		Compress-Archive -Path $rptFolder -DestinationPath $fullPathZipFile
		return $fullPathZipFile
	}
	return $null
}

function UploadHtmlReport() {
	$index = 0
	foreach ( $item in $rptFolders ) {
		$testPathReportInput = Join-Path $item -ChildPath "run_results.html"
		if (Test-Path $testPathReportInput) {
			$artifact = $rptFileNames[$index]
			# upload resource to container
			UploadArtifactToAzureStorage $storageContext $container $testPathReportInput $artifact
		}
		$index += 1
	}
}

function UploadArchive() {
	$index = 0
	foreach ( $item in $rptFolders ) {
		#archive report folder	
		$artifact = $zipFileNames[$index]
		
		$fullPathZipFile = ArchiveReport $artifact $item
		if ($fullPathZipFile) {
			UploadArtifactToAzureStorage $storageContext $container $fullPathZipFile $artifact
		}
					
		$index += 1
	}
}

#---------------------------------------------------------------------------------------------------

$uftReport = "$resDir\UFT Report"
$runSummary = "$resDir\Run Summary"
$retcodefile = "$resDir\TestRunReturnCode.txt"
$results = "$resDir\Results*SSS.xml"
$failedTests = "$resDir\Failed Tests"

$rptFolders = New-Object System.Collections.Generic.List[System.String]
$rptFileNames = New-Object System.Collections.Generic.List[System.String]
$zipFileNames = New-Object System.Collections.Generic.List[System.String]

if ($rptFileName) {
	$rptFileName += "_$buildNumber"
} else {
	$rptFileName = "${pipelineName}_${buildNumber}"
}
if ($rerunIdx) {
	$rptFileName += "_rerun$rerunIdx"
}

$archiveNamePattern = "${rptFileName}_Report"

#---------------------------------------------------------------------------------------------------
#storage variables validation

if($uploadArtifact -eq "yes") {
	# get resource group
	if ($null -eq $env:RESOURCE_GROUP) {
		Write-Error "Missing resource group."
	} else {
		$group = $env:RESOURCE_GROUP
		$resourceGroup = Get-AzResourceGroup -Name "$($group)"
		$groupName = $resourceGroup.ResourceGroupName
	}

	# get storage account
	$account = $env:STORAGE_ACCOUNT

	$storageAccounts = Get-AzStorageAccount -ResourceGroupName "$($groupName)"

	$correctAccount = 0
	foreach($item in $storageAccounts) {
		if ($item.storageaccountname -like $account) {
			$storageAccount = $item
			$correctAccount = 1
			break
		}
	}

	if ($correctAccount -eq 0) {
		if ([string]::IsNullOrEmpty($account)) {
			Write-Error "Missing storage account."
		} else {
			Write-Error ("Provided storage account {0} not found." -f $account)
		}
	} else {
		$storageContext = $storageAccount.Context
		
		#get container
		$container = $env:CONTAINER

		$storageContainer = Get-AzStorageContainer -Context $storageContext -ErrorAction Stop | where-object {$_.Name -eq $container}
		if ($storageContainer -eq $null) {
			if ([string]::IsNullOrEmpty($container)) {
				Write-Error "Missing storage container."
			} else {
				Write-Error ("Provided storage container {0} not found." -f $container)
			}
		}
	}
}

if ($rerunIdx) {
	Write-Host "Rerun attempt = $rerunIdx"
}

#---------------------------------------------------------------------------------------------------
#Run the tests
Invoke-FSTask $testPathInput $timeOutIn $uploadArtifact $artifactType $env:STORAGE_ACCOUNT $env:CONTAINER $rptFileName $archiveNamePattern $buildNumber -Verbose 

if ($testPathInput.Contains(".mtb")) { #batch file with multiple tests
	$XMLfile = $testPathInput
	[XML]$testDetails = Get-Content $XMLfile
	foreach($test in $testDetails.Mtbx.Test) {
		$rptFolder = Join-Path $test.path -ChildPath "Report"
		$rptFolders.Add($rptFolder)
	}
} else { #single test or multiline tests
	$resFile = (Get-ChildItem -File $results | Sort-Object -Property CreationTime -Descending | Select-Object -First 1)
	if ($resFile -and (Test-Path $resFile.FullName)) {
		[XML]$testDetails = Get-Content $resFile.FullName
		$rptAttributes = $testDetails.SelectNodes("/testsuites/testsuite/testcase[@report != '']/@report")
		if ($rptAttributes) {
			foreach($attr in $rptAttributes) {
				$rptFolders.Add($attr.Value)
			}
		}
	} else {
		Write-Error "Cannot find the $results file."
	}
}
$ind = 1
foreach ($item in $rptFolders) {
	$rptFileNames.Add("${rptFileName}_${ind}.html")
	$zipFileNames.Add("${rptFileName}_Report_${ind}.zip")
	$ind += 1
}

#---------------------------------------------------------------------------------------------------
#upload artifacts to Azure storage
if ($uploadArtifact -eq "yes") {
	if ($artifactType -eq "onlyReport") { #upload only report
		UploadHtmlReport
	} elseif ($artifactType -eq "onlyArchive") { #upload only archive
		UploadArchive
	} else { #upload both report and archive
		UploadHtmlReport
		UploadArchive
	}
}

#---------------------------------------------------------------------------------------------------
# uploads report files to build artifacts
# upload and display Run Summary
if (Test-Path $runSummary) {
	if ($rerunIdx) {
		Write-Host "##vso[task.addattachment type=Distributedtask.Core.Summary;name=Run Summary (rerun $rerunIdx);]$runSummary"
	} else {
		Write-Host "##vso[task.uploadsummary]$runSummary"
	}
}

# upload and display UFT report
if (Test-Path $uftReport) {
	if ($rerunIdx) {
		Write-Host "##vso[task.addattachment type=Distributedtask.Core.Summary;name=UFT Report (rerun $rerunIdx);]$uftReport"
	} else {
		Write-Host "##vso[task.uploadsummary]$uftReport"
	}
}

# upload and display Failed Tests
if (Test-Path $failedTests) {
	if ($rerunIdx) {
		Write-Host "##vso[task.addattachment type=Distributedtask.Core.Summary;name=Failed Tests (rerun $rerunIdx);]$failedTests"
	} else {
		Write-Host "##vso[task.uploadsummary]$failedTests"
	}
}

# read return code
if (Test-Path $retcodefile) {
	$content = Get-Content $retcodefile
	if ($content) {
		$sep = [Environment]::NewLine
		$option = [System.StringSplitOptions]::RemoveEmptyEntries
		$arr = $content.Split($sep, $option)
		[int]$retcode = [convert]::ToInt32($arr[-1], 10)
	
		if ($retcode -eq 0) {
			Write-Host "Test passed"
		}

		if ($retcode -eq -3) {
			Write-Error "Task Failed with message: Closed by user"
		} elseif ($retcode -ne 0) {
			Write-Error "Task Failed"
		}
	} else {
		Write-Error "The file [$retcodefile] is empty!"
	}
}
