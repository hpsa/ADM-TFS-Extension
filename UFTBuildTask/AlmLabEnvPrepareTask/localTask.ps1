#
# localTask.ps1
#
    
$varAlmserv = Get-VstsInput -Name 'varAlmserv' -Require
$varUserName = Get-VstsInput -Name 'varUserName' -Require
$varPass = Get-VstsInput -Name 'varPass'
$varDomain = Get-VstsInput -Name 'varDomain' -Require
$varProject = Get-VstsInput -Name 'varProject' -Require
$varEnvId = Get-VstsInput -Name 'varEnvId' -Require

$javaHomeSelection = Get-VstsInput -Name 'javaHomeSelection' -Require
$createNewNamed = Get-VstsInput -Name 'createNewNamed'
$assignMessage = Get-VstsInput -Name 'assignMessage'
$jdkUserInputPath = Get-VstsInput -Name 'jdkUserInputPath'
$varPathToJSON = Get-VstsInput -Name 'varPathToJSON'
$paramOnlyFirst = Get-VstsInput -Name 'paramOnlyFirst' -Require

$AddParam1 = Get-VstsInput -Name 'AddParam1'
$paramType1 = Get-VstsInput -Name 'paramType1'
$paramName1 = Get-VstsInput -Name 'paramName1'
$paramValue1 = Get-VstsInput -Name 'paramValue1'

$AddParam2 = Get-VstsInput -Name 'AddParam2'
$paramType2 = Get-VstsInput -Name 'paramType2'
$paramName2 = Get-VstsInput -Name 'paramName2'
$paramValue2 = Get-VstsInput -Name 'paramValue2'

$AddParam3 = Get-VstsInput -Name 'AddParam3'
$paramType3 = Get-VstsInput -Name 'paramType3'
$paramName3 = Get-VstsInput -Name 'paramName3'
$paramValue3 = Get-VstsInput -Name 'paramValue3'

$AddParam4 = Get-VstsInput -Name 'AddParam4'
$paramType4 = Get-VstsInput -Name 'paramType4'
$paramName4 = Get-VstsInput -Name 'paramName4'
$paramValue4 = Get-VstsInput -Name 'paramValue4'

$AddParam5 = Get-VstsInput -Name 'AddParam5'
$paramType5 = Get-VstsInput -Name 'paramType5'
$paramName5 = Get-VstsInput -Name 'paramName5'
$paramValue5 = Get-VstsInput -Name 'paramValue5'

$AddParam6 = Get-VstsInput -Name 'AddParam6'
$paramType6 = Get-VstsInput -Name 'paramType6'
$paramName6 = Get-VstsInput -Name 'paramName6'
$paramValue6 = Get-VstsInput -Name 'paramValue6'

$AddParam7 = Get-VstsInput -Name 'AddParam7'
$paramType7 = Get-VstsInput -Name 'paramType7'
$paramName7 = Get-VstsInput -Name 'paramName7'
$paramValue7 = Get-VstsInput -Name 'paramValue7'

$AddParam8 = Get-VstsInput -Name 'AddParam8'
$paramType8 = Get-VstsInput -Name 'paramType8'
$paramName8 = Get-VstsInput -Name 'paramName8'
$paramValue8 = Get-VstsInput -Name 'paramValue8'

$AddParam9 = Get-VstsInput -Name 'AddParam9'
$paramType9 = Get-VstsInput -Name 'paramType9'
$paramName9 = Get-VstsInput -Name 'paramName9'
$paramValue9 = Get-VstsInput -Name 'paramValue9'

$AddParam10 = Get-VstsInput -Name 'AddParam10'
$paramType10 = Get-VstsInput -Name 'paramType10'
$paramName10 = Get-VstsInput -Name 'paramName10'
$paramValue10 = Get-VstsInput -Name 'paramValue10'

#Import-Module "Microsoft.TeamFoundation.DistributedTask.Task.Common"

$uftworkdir = $env:UFT_LAUNCHER

$stdout = "$uftworkdir\temp_build.log"
$stderr = "$uftworkdir\temp_error_build.log"
$jar = """$uftworkdir\bin\hpe.application.automation.tfs.almrestrunner-1.0-jar-with-dependencies.jar"""

$args = "-jar $jar lep  ""$varAlmserv"" ""$varUserName"" ""pass:$varPass"" ""$varDomain"" ""$varProject"" ""$varEnvId"" ""$javaHomeSelection"" ""newnamed:$createNewNamed"" ""assign:$assignMessage"" ""useasexisting:$jdkUserInputPath"" ""jsonpath:$varPathToJSON"" ""$paramOnlyFirst"""

if ($AddParam1 -eq $True)
{
	$args = "$($args) ""partype1:$paramType1"" ""parname1:$paramName1"" ""parval1:$paramValue1"""
	if ($AddParam2 -eq $True)
	{
		$args = "$($args) ""partype2:$paramType2"" ""parname2:$paramName2"" ""parval2:$paramValue2"""
		if ($AddParam3 -eq $True)
		{
			$args = "$($args) ""partype3:$paramTyp3"" ""parname3:$paramNam3"" ""parval3:$paramValue3"""
			if ($AddParam4 -eq $True)
			{
				$args = "$($args) ""partype4:$paramType4"" ""parname4:$paramName4"" ""parval4:$paramValue4"""
				if ($AddParam5 -eq $True)
				{
					$args = "$($args) ""partype5:$paramTyp5"" ""parname5:$paramNam5"" ""parval5:$paramValue5"""
					if ($AddParam6 -eq $True)
					{
						$args = "$($args) ""partype6:$paramType6"" ""parname6:$paramName6"" ""parval6:$paramValue6"""
						if ($AddParam7 -eq $True)
						{
							$args = "$($args) ""partype7:$paramType7"" ""parname7:$paramName7"" ""parval7:$paramValue7"""
							if ($AddParam8 -eq $True)
							{
								$args = "$($args) ""partype8:$paramType8"" ""parname8:$paramName8"" ""parval8:$paramValue8"""
								if ($AddParam9 -eq $True)
								{
									$args = "$($args) ""partype9:$paramType9"" ""parname9:$paramName9"" ""parval9:$paramValue9"""
									if ($AddParam10 -eq $True)
									{
										$args = "$($args) ""partype10:$paramType10"" ""parname10:$paramName10"" ""parval10:$paramValue10"""
									}
								}
							}
						}
					}
				}
			}
		}
	}
}

$updateVariableFile = Join-Path $env:UFT_LAUNCHER -ChildPath "res\updateVariable.txt"
if (Test-Path $updateVariableFile)
{
	Remove-Item $updateVariableFile
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
	if (Test-Path $stdout)
	{
		Get-Content $stdout
	}
	if (Test-Path $stderr)
	{
		Get-Content $stderr
	}
	if ($assignMessage)
	{
		if (Test-Path $updateVariableFile)
		{
			$content = [IO.File]::ReadAllText($updateVariableFile)
			Set-TaskVariable $assignMessage $content

			$varVal = Get-TaskVariable $distributedTaskContext $assignMessage

			Write-Host "Variable '$($assignMessage)' updated with a new value '$($varVal)'"
		}
	}
}

if (Test-Path $stdout)
{
	Remove-Item $stdout
}
if (Test-Path $stderr)
{
	Remove-Item $stderr
}

