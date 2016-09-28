/// <reference path="../../definitions/vsts-task-lib.d.ts" />

import path = require('path');
import tl = require('vsts-task-lib/task');

tl.setResourcePath(path.join(__dirname, 'task.json'));

// content is a folder contain artifacts needs to publish.
var pathtoPublish: string = "C:\\Tests\\GUITest1\\Res2\\Report";
var artifactName: string = "result"
var artifactType: string = "container";
// targetPath is used for file shares
var targetPath: string = "";

artifactType = artifactType.toLowerCase();

try {
    var data = {
        artifacttype: artifactType,
        artifactname: artifactName
    };

    // upload or copy
    if (artifactType === "container") {
        data["containerfolder"] = artifactName;

        // add localpath to ##vso command's properties for back compat of old Xplat agent
        data["localpath"] = pathtoPublish;
        tl.command("artifact.upload", data, pathtoPublish);
    }
    else if (artifactType === "filepath") {
        var artifactPath: string = path.join(targetPath, artifactName);
        tl.mkdirP(artifactPath);
        tl.cp("-Rf", path.join(pathtoPublish, "*"), artifactPath);

        // add artifactlocation to ##vso command's properties for back compat of old Xplat agent
        data["artifactlocation"] = targetPath;
        tl.command("artifact.associate", data, targetPath);
    }
}
catch (err) {
    tl.setResult(tl.TaskResult.Failed, tl.loc('PublishBuildArtifactsFailed', err.message));
}