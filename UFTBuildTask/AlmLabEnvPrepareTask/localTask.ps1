#
# localTask.ps1
#

param(
	[string][Parameter(Mandatory=$true)] $varAlmserv,
	[string][Parameter(Mandatory=$true)] $varUserName,
	[string] $varPass,
	[string][Parameter(Mandatory=$true)] $varDomain,
	[string][Parameter(Mandatory=$true)] $varProject,
	[string][Parameter(Mandatory=$true)] $varEnvId,

	[string][Parameter(Mandatory=$true)] $javaHomeSelection,
	[string][Parameter(Mandatory=$false)] $createNewNamed,
	[string][Parameter(Mandatory=$false)] $assignMessage,
	[string][Parameter(Mandatory=$false)] $jdkUserInputPath,
	[string][Parameter(Mandatory=$false)] $varPathToJSON,
	[string][Parameter(Mandatory=$true)] $paramOnlyFirst,

	[string][Parameter(Mandatory=$false)] $AddParam1,
	[string][Parameter(Mandatory=$false)] $paramType1,
	[string][Parameter(Mandatory=$false)] $paramName1,
	[string][Parameter(Mandatory=$false)] $paramValue1,

	[string][Parameter(Mandatory=$false)] $AddParam2,
	[string][Parameter(Mandatory=$false)] $paramType2,
	[string][Parameter(Mandatory=$false)] $paramName2,
	[string][Parameter(Mandatory=$false)] $paramValue2,

	[string][Parameter(Mandatory=$false)] $AddParam3,
	[string][Parameter(Mandatory=$false)] $paramType3,
	[string][Parameter(Mandatory=$false)] $paramName3,
	[string][Parameter(Mandatory=$false)] $paramValue3,

	[string][Parameter(Mandatory=$false)] $AddParam4,
	[string][Parameter(Mandatory=$false)] $paramType4,
	[string][Parameter(Mandatory=$false)] $paramName4,
	[string][Parameter(Mandatory=$false)] $paramValue4,

	[string][Parameter(Mandatory=$false)] $AddParam5,
	[string][Parameter(Mandatory=$false)] $paramType5,
	[string][Parameter(Mandatory=$false)] $paramName5,
	[string][Parameter(Mandatory=$false)] $paramValue5,

	[string][Parameter(Mandatory=$false)] $AddParam6,
	[string][Parameter(Mandatory=$false)] $paramType6,
	[string][Parameter(Mandatory=$false)] $paramName6,
	[string][Parameter(Mandatory=$false)] $paramValue6,

	[string][Parameter(Mandatory=$false)] $AddParam7,
	[string][Parameter(Mandatory=$false)] $paramType7,
	[string][Parameter(Mandatory=$false)] $paramName7,
	[string][Parameter(Mandatory=$false)] $paramValue7,

	[string][Parameter(Mandatory=$false)] $AddParam8,
	[string][Parameter(Mandatory=$false)] $paramType8,
	[string][Parameter(Mandatory=$false)] $paramName8,
	[string][Parameter(Mandatory=$false)] $paramValue8,

	[string][Parameter(Mandatory=$false)] $AddParam9,
	[string][Parameter(Mandatory=$false)] $paramType9,
	[string][Parameter(Mandatory=$false)] $paramName9,
	[string][Parameter(Mandatory=$false)] $paramValue9,

	[string][Parameter(Mandatory=$false)] $AddParam10,
	[string][Parameter(Mandatory=$false)] $paramType10,
	[string][Parameter(Mandatory=$false)] $paramName10,
	[string][Parameter(Mandatory=$false)] $paramValue10
)

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

echo $args

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
	Get-Content $stdout
	Get-Content $stderr
}

Remove-Item $stdout
Remove-Item $stderr
