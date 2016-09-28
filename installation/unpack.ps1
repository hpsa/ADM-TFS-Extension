function Expand-ZIPFile($file, $destination)
{
    $shell = new-object -com shell.application
    $zip = $shell.NameSpace($file)
    $dest = $shell.NameSpace($destination)
    foreach($item in $zip.items())
    {
        $dest.copyhere($item)
    }
}

function AbsPath($folder)
{
    [System.IO.Directory]::SetCurrentDirectory(((Get-Location -PSProvider FileSystem).ProviderPath))
    $path = [IO.Path]::GetFullPath($folder)
    
    return $path
}

$Registry_Key ="HKLM:\SOFTWARE\Wow6432Node\Mercury Interactive\QuickTest Professional\CurrentVersion"
$result = Test-Path $Registry_Key

if($result)
{
    $value = "QuickTest Professional"
    $uftPath = Get-ItemProperty -Path $Registry_Key | Select-Object -ExpandProperty $value

    $currdir = AbsPath -folder .\
    $zipFile = Join-Path -Path $currdir -ChildPath “UFT.zip”
    Expand-ZIPFile $zipFile $currdir

    $launcherPath = Join-Path $currdir -ChildPath "UFTWorking"
    
    [Environment]::SetEnvironmentVariable("UFT_LAUNCHER", $launcherPath, "Machine")

}else
{
     Write-Host "requared HPE UFT installed"
}