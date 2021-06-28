# UFT One Azure DevOps extension
Enables you to run UFT One tests as a build in an Azure DevOps build process. This extension includes 4 tasks.
## Table of contents
1. [Integration with UFT One](#Integration-with-UFT-One)
2. [Configuration](#Configuration)
3. [Extension functionality](#Extension-functionality)
4. [Resources](#Additional-resources)

# Integration with UFT One
In a build step, run UFT One tests stored in the local file system or on an ALM server. When running tests from ALM Lab Management, you can also include a build step that prepares the test environment using test sets or build verification suites before running the tests. After the build is complete, you can view comprehensive test results. 
#  Configuration
#### Prerequisites
- UFT One (version >=**14.00**)
- Powershell (version **4** or later)
- JRE installed

#### Setup
1. From [Visual Studio Marketplace][marketplace]: Install the **UFT One Azure DevOps extension** for the relevant organization
2. On our [GitHub][repository]: Navigate to a specific release (latest: **1.1.0**)
3. From [Azure DevOps][azure-devops]: Navigate to **agent pools** and set up an agent (interactive or run as a service) 
4. On your agent machine:    
4.1. Download the resources provided by a specific release (UFT.zip, unpack.ps1 and optionally the .vsix file)    
4.2. Run the *unpack.ps1* script    

# Extension Functionality
##### Run from File System
- Use this task to run tests located in your file system by specifying the tests' names, folders that contain tests, or an MTBX file (code sample below).
``` xml 
<Mtbx>
    <Test name="Test-Name-11" path="Test-Path-1">
    </Test>
    <Test name="Test-Name-2" path="Test-Path-2">
    </Test>
</Mtbx>
```
- More information is available [here][fs-docs]

##### Run from ALM
- Use this task to run tests located on an ALM server, to which you can connect using SSO or a username and password.
- More information is available [here][alm-docs]

##### Run from ALM Lab Management
- Use this task to run ALM server-side functional test sets and build verification suites.
- More information is available [here][alm-lab-docs]

##### Run from ALM Lab Environment Preparation
- Use this task to assign values to AUT Environment Configurations located in ALM.
- More information is available [here][alm-env-docs]
#
#
# Additional Resources
For assistance or more information on configuring and using this extension, please consult the following resources:
- [Extension Marketplace page][marketplace]
- [Help Center][docs]
- [UFT One Forum][forum]
- [Support][support]

[//]: # (References)
   [docs]:<https://admhelp.microfocus.com/uft/en/latest/UFT_Help/Content/UFT_Tools/Azure_DevOps_Extension/uft-azure-devops.htm>
   [forum]:<https://community.microfocus.com/adtd/uft/f/sws-fun_test_sf/>
   [support]:<https://softwaresupport.softwaregrp.com/>
   [repository]:<https://github.com/MicroFocus/ADM-TFS-Extension/>
   [marketplace]:<https://marketplace.visualstudio.com/items?itemName=uftpublisher.UFT-Azure-extension>
   [fs-docs]:<http://adm-uft-staging.s3-us-west-2.amazonaws.com/uft/en/staging/UFT_Help/Content/UFT_Tools/Azure_DevOps_Extension/uft-azure-devops-run-local.htm>
   [alm-docs]:<http://adm-uft-staging.s3-us-west-2.amazonaws.com/uft/en/staging/UFT_Help/Content/UFT_Tools/Azure_DevOps_Extension/uft-azure-devops-run-alm.htm>
   [alm-lab-docs]:<http://adm-uft-staging.s3-us-west-2.amazonaws.com/uft/en/staging/UFT_Help/Content/UFT_Tools/Azure_DevOps_Extension/uft-azure-devops-run-alm-lm.htm>
   [alm-env-docs]:<https://admhelp.microfocus.com/uft/en/15.0-15.0.2/UFT_Help/Content/UFT_Tools/Azure_DevOps_Extension/uft-azure-devops-run-alm-lm.htm#mt-item-0>
   [azure-devops]:<https://dev.azure.com/>
   [azure-portal]:<http://portal.azure.com/>
   [azure-powershell]:<https://docs.microsoft.com/en-us/powershell/azure/install-az-ps?view=azps-6.0.0>
   [azure-connect]:<https://docs.microsoft.com/en-us/powershell/module/az.accounts/connect-azaccount?view=azps-6.0.0>