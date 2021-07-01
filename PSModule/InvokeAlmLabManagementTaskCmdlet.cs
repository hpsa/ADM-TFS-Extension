using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management.Automation;

namespace PSModule
{
    [Cmdlet(VerbsLifecycle.Invoke, "AlmLabManagementTask")]

    public class InvokeAlmLabManagementTaskCmdlet : AbstractLauncherTaskCmdlet
    {
        [Parameter(Position = 0, Mandatory = true)]
        public string ALMServerPath { get; set; }

        [Parameter(Position = 1, Mandatory = true)]
        public string ALMUserName { get; set; }

        [Parameter(Position = 2)]
        public string ALMPassword { get; set; }

        [Parameter(Position = 3, Mandatory = true)]
        public string ALMDomain { get; set; }

        [Parameter(Position = 4, Mandatory = true)]
        public string ALMProject { get; set; }

        [Parameter(Position = 5)]
        public string TestRunType { get; set; }

        [Parameter(Position = 6, Mandatory = true)]
        public string ALMTestSet { get; set; }

        [Parameter(Position = 7)]
        public string Description { get; set; }

        [Parameter(Position = 8, Mandatory = true)]
        public string TimeslotDuration { get; set; }

        [Parameter(Position = 9)]
        public string EnvironmentConfigurationID { get; set; }

        [Parameter(Position = 10)]
        public string ReportName { get; set; }

        [Parameter(Position = 11)]
        public bool UseCDA { get; set; }

        [Parameter(Position = 12)]
        public string DeploymentAction { get; set; }

        [Parameter(Position = 13)]
        public string DeploymentEnvironmentName { get; set; }

        [Parameter(Position = 14)]
        public string DeprovisioningAction { get; set; }

        [Parameter(Position = 15)]
        public string BuildNumber { get; set; }

        protected override string GetReportFilename()
        {
            return string.IsNullOrEmpty(ReportName) ? base.GetReportFilename() : ReportName;
        }


        public override Dictionary<string, string> GetTaskProperties()
        {
            LauncherParamsBuilder builder = new LauncherParamsBuilder();

            builder.SetRunType(RunType.AlmLabManagement);
            builder.SetAlmServerUrl(ALMServerPath);
            builder.SetAlmUserName(ALMUserName);
            builder.SetAlmPassword(ALMPassword);
            builder.SetAlmDomain(ALMDomain);
            builder.SetAlmProject(ALMProject);
            builder.SetBuildNumber(BuildNumber);

            switch (TestRunType)
            {
                case "testSet":
                    builder.SetTestRunType(RunTestType.TEST_SUITE);
                    break;
                case "buildVerificationSuite":
                    builder.SetTestRunType(RunTestType.BUILD_VERIFICATION_SUITE);
                    break;
            }

            if (!string.IsNullOrEmpty(ALMTestSet))
            {
                int i = 1;
                foreach (string testSet in ALMTestSet.Split('\n'))
                {
                    builder.SetTestSet(i++, testSet.Replace(@"\", @"\\"));
                }
            }
            else
            {
                builder.SetAlmTestSet("");
            }


            if (UseCDA)
            {
                builder.SetDeploymentAction(DeploymentAction);
                builder.SetDeployedEnvironmentName(DeploymentEnvironmentName);
                builder.SetDeprovisioningAction(DeprovisioningAction);
            }

            //set ALM mandatory parameters
            builder.SetAlmTimeout(TimeslotDuration);
            builder.SetAlmRunMode(AlmRunMode.RUN_LOCAL);
            builder.SetAlmRunHost("localhost");

            return builder.GetProperties();
        }

        protected override string GetRetCodeFileName()
        {
            return "TestRunReturnCode.txt";
        }
    }
}
