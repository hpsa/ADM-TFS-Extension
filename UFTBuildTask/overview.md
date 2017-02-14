# Overview

This plugin allows the Microsoft Team Foundation Server CI system to trigger tests using UFT from the local file system, from ALM, or from the ALM Test Lab module. In addition, you can use the Extension to prepare the testing environment before running a test set from the ALM Test Lab.

## You can run the following tasks:

* **Run a test on the local file system**
You can run UFT tests stored on the file system ([Wiki](https://github.com/hpsa/ADM-TFS-Extension/wiki/Run-UFT-tests-from-the-file-system)).

* **Run a test from ALM**
You can run UFT tests saved in ALM ([Wiki](https://github.com/hpsa/ADM-TFS-Extension/wiki/Run-UFT-tests-from-ALM)).

* **Run a test from ALM using Lab Management**
You can run UFT test sets from ALM using Lab Management ([Wiki](https://github.com/hpsa/ADM-TFS-Extension/wiki/Run-a-UFT-test-from-ALM-using-Lab-Management)).

* **Prepare the application environment for an ALM test running with Lab Managemet**
If you are running UFT tests saved in ALM, as part of a test set or build verification suite from the Test Lab module in ALM, you can prepare the environment configuration for the test before running the test from the Test Lab as a build step ([Wiki](https://github.com/hpsa/ADM-TFS-Extension/wiki/Configure-the-application-environment-for-an-ALM-Test-using-Lab-Management)).

## This extension currently supports:

* UFT 14.00

## System prerequisites

* UFT installed on the same computer as the Extension
* Powershell version 4.0 or higher
* JRE installed with the Path environment variable pointing to the JRE installation folder

