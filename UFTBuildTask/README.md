# Integration with HPE UFT

This extension enables you to include UFT tests as a build step in a TFS build process. Run UFT tests stored on the local file system or on ALM, and then view your test results after the build is complete.

# Release Notes
> **8-6-2016**

> - Added: Test with HPE UFT on local file system

# Installation Instructions

To install the Extension, do the following:

NOTE: You must have Administrator privileges to install the Extension.

1) Install UFT on the machine that hosts the TFS server.
2) Download the [TFS extension files}(https://github.com/hpsa/ADM-TFS-Extension/tree/master/installation).
3) Unzip the downloaded folder from the download location.
4) Run the unpack Powershell script. This unpacks the necessary files for the extension and UFT agent to run and sets the system environmental variables appropriately.
5) From the main Start Page of the server, in the toolbar, click the Browse Store button and select Browse TFS Extensions. The Team Foundation Server Extensions page opens.
6) At the bottom of the page, in the Manage Extensions box, click the Manage Extensions button. The Manage Extensions page opens.
7) In the Manage Extensions page, click the Upload New Extension button. Then, in the dialog, navigate to the director where the .vsix file you download is saved.
8) Select the .vsix file you downloaded and click Upload.
9) In the Install new extension dialog box, click OK again. The TFS server pauses for a few moments while uploading and installing the extension.
10) In the window that opens, select which collections for which the extension is used.

The HPE Application Automation Tools Extension extension is now displayed a valid extension for the server and can be used to run UFT tests.

# Documentation

Please check the [Wiki](https://github.com/hpsa/ADM-TFS-Extension/wiki/HPE-ADM-TFS-Extension).
