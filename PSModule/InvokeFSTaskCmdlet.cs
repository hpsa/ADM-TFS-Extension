using System.Management.Automation;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
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

        public MobileSettings mobile;

        protected override void CollateResults(string resultFile, string log, string resdir)
        {
            //do nothing here. Collate results should be made by the standard "Copu and Publish Artifacts" TFS task
        }

        public override Dictionary<string, string> GetTaskProperties()
        {
            LauncherParamsBuilder builder = new LauncherParamsBuilder();

            builder.SetRunType(RunType.FileSystem);
            builder.SetPerScenarioTimeOut(Timeout);

            var tests = TestsPath.Split(";".ToArray());

            for (int i = 0; i < tests.Length; i++)
            {
                string pathToTest = tests[i].Replace("\\", "\\\\");
                builder.SetTest(i + 1, pathToTest);
            }

            return builder.GetProperties();
        }

        protected override string GetRetCodeFileName()
        {
            return "FSTaskRetCode.txt";
        }
    }
}
