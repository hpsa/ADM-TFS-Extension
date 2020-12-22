using System.Management.Automation;
using System.Linq;
using System.Collections.Generic;
using PSModule.Models;

namespace PSModule
{
    [Cmdlet(VerbsLifecycle.Invoke, "FSTask")]
    public class InvokeFSTaskCmdlet : AbstractLauncherTaskCmdlet
    {
        [Parameter(Position = 0, Mandatory = true)]
        public string TestsPath;

        [Parameter(Position = 1)]
        public string Timeout;

        [Parameter(Position = 2, Mandatory = true)]
        public string UploadArtifact;

        [Parameter(Position = 3)]
        public ArtifactType ArtType;

        [Parameter(Position = 4)]
        public string StorageAccount;

        [Parameter(Position = 5)]
        public string Container;

        [Parameter(Position = 6)]
        public string ReportFileName;

        [Parameter(Position = 7)]
        public string ArchiveName;

        //public MobileSettings mobile;

        protected override void CollateResults(string resultFile, string log, string resdir)
        {
            //do nothing here. Collate results should be made by the standard "Copy and Publish Artifacts" TFS task
        }

        public override Dictionary<string, string> GetTaskProperties()
        {
            LauncherParamsBuilder builder = new LauncherParamsBuilder();

            builder.SetRunType(RunType.FileSystem);
            builder.SetPerScenarioTimeOut(Timeout);

            var tests = TestsPath.Split("\n".ToArray());

            for (int i = 0; i < tests.Length; i++)
            {
                string pathToTest = tests[i].Replace("\\", "\\\\");
                builder.SetTest(i + 1, pathToTest);
            }

            builder.SetUploadArtifact(UploadArtifact);
            builder.SetArtifactType(ArtType);
            builder.SetReportName(ReportFileName);
            builder.SetArchiveName(ArchiveName);
            builder.SetStorageAccount(StorageAccount);
            builder.SetContainer(Container);

            return builder.GetProperties();
        }

        protected override string GetRetCodeFileName()
        {
            return "TestRunReturnCode.txt";
        }
    }
}
