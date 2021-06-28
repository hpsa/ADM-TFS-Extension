# UFT One Azure DevOps integration files
These are the files that are required to run **UFT One** tests on an Azure DevOps agent machine. The server must have the **UFT One Azure DevOps extension** installed.

## Important Notes
- This folder may contain files that are under development, which might not be compatible with the latest release.
- Always download the integration files from the [release page][release-page].

## UFT.zip
This zip package contains the UFT One Azure DevOps integration files. To unzip and set up the files, run the **unpack.ps1** script file.

## unpack.ps1
This is a Windows PowerShell script file that extracts the integration files from the **UFT.zip** file and sets up the agent machine to use them.

## uftpublisher.UFT-Azure-extension-x.y.z.vsix
This is the UFT One Azure DevOps extension. This file is not required on the agent machine. You can use it if you want to install the extension manually on the Azure DevOps server or services.


[release-page]: https://github.com/MicroFocus/ADM-TFS-Extension/releases
