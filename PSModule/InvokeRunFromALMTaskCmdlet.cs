using PSModule.Models;
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace PSModule
{
    [Cmdlet(VerbsLifecycle.Invoke, "RunFromAlmTask")]
    public class InvokeRunFromALMTaskCmdlet : AbstractLauncherTaskCmdlet
    {
        [Parameter(Position = 0, Mandatory = true)]
        public string ALMServerPath;

        [Parameter(Position = 1, Mandatory = false)]
        public string SSOEnabled;

        [Parameter(Position = 2)]
        public string ClientID;

        [Parameter(Position = 3)]
        public string ApiKeySecret;

        [Parameter(Position = 4)]
        public string ALMUserName;

        [Parameter(Position = 5)]
        public string ALMPassword;

        [Parameter(Position = 6)]
        public string ALMDomain;

        [Parameter(Position = 7)]
        public string ALMProject;

        [Parameter(Position = 8)]
        public string ALMTestSet;

        [Parameter(Position = 9)]
        public string TimeOut;

        [Parameter(Position = 10)]
        public string ReportName;

        [Parameter(Position = 11)]
        public string RunMode;

        [Parameter(Position = 12)]
        public string ALMRunHost;

        /*[Parameter(Position = 13, Mandatory = true)]
        public string UploadArtifact;

        [Parameter(Position = 14)]
        public ArtifactType ArtType;

        [Parameter(Position = 15)]
        public string StorageAccount;

        [Parameter(Position = 16)]
        public string Container;

        [Parameter(Position = 17)]
        public string ReportFileName;

        [Parameter(Position = 18)]
        public string ArchiveName;*/

        [Parameter(Position = 13)]
        public string BuildNumber;


        protected override string GetReportFilename()
        {
            return String.IsNullOrEmpty(ReportName) ? base.GetReportFilename() : ReportName;
        }

        public override Dictionary<string, string> GetTaskProperties()
        {
            LauncherParamsBuilder builder = new LauncherParamsBuilder();

            builder.SetRunType(RunType.Alm);
            builder.SetAlmServerUrl(ALMServerPath);
            builder.SetSSOEnabled(SSOEnabled);
            builder.SetClientID(ClientID);
            builder.SetApiKeySecret(ApiKeySecret);
            builder.SetAlmUserName(ALMUserName);
            builder.SetAlmPassword(ALMPassword);
            builder.SetAlmDomain(ALMDomain);
            builder.SetAlmProject(ALMProject);
            builder.SetAlmRunHost(ALMRunHost);
            builder.SetAlmTimeout(TimeOut);
            builder.SetBuildNumber(BuildNumber);

            switch (RunMode)
            {
                case "runLocally":
                    builder.SetAlmRunMode(AlmRunMode.RUN_LOCAL);
                    break;
                case "runOnPlannedHost":
                    builder.SetAlmRunMode(AlmRunMode.RUN_PLANNED_HOST);
                    break;
                case "runRemotely":
                    builder.SetAlmRunMode(AlmRunMode.RUN_REMOTE);
                    break;
            }

            if (!string.IsNullOrEmpty(ALMTestSet))
            {
                int i = 1;
                foreach (string testSet in ALMTestSet.Split('\n'))
                {
                    builder.SetTestSet(i++, testSet.Replace(@"\",@"\\"));
                }
            }
            else
            {
                builder.SetAlmTestSet("");
            }

           /* builder.SetUploadArtifact(UploadArtifact);
            builder.SetArtifactType(ArtType);
            builder.SetReportName(ReportFileName);
            builder.SetArchiveName(ArchiveName);
            builder.SetStorageAccount(StorageAccount);
            builder.SetContainer(Container);*/

            return builder.GetProperties();
        }

        protected override string GetRetCodeFileName()
        {
            return "TestRunReturnCode.txt";
        }

    }
}
