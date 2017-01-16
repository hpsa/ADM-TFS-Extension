using System;
using System.IO;
using System.Management.Automation;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Diagnostics;

namespace PSModule
{
    public abstract class AbstractLauncherTaskCmdlet : PSCmdlet
    {
        const string UFTFolder = "UFTWorking";
        const string HpToolsLauncher_SCRIPT_NAME = "HpToolsLauncher.exe";
        const string HpToolsAborter_SCRIPT_NAME = "HpToolsAborter.exe";

        public abstract Dictionary<string, string> GetTaskProperties();

        protected override void ProcessRecord()
        {
            WriteDebug("WriteDebug ProcessRecord");

            ErrorRecord err = new ErrorRecord(new Exception("my exceptiom"), "errId", ErrorCategory.CloseError, "something goeas wrong");
            ThrowTerminatingError(err);

            string aborterPath = "";
            string paramFileName = "";

           // System.Diagnostics.Debugger.Launch();

            try
            {
                Dictionary<string, string> properties = new Dictionary<string, string>();
                try
                {
                    properties = GetTaskProperties();
                }
                catch (Exception e)
                {
                    ThrowTerminatingError(new ErrorRecord(e, "GetTaskProperties", ErrorCategory.ParserError, ""));
                }

                string ufttfsdir = Environment.GetEnvironmentVariable("UFT_LAUNCHER");

                string launcherPath = Path.GetFullPath(Path.Combine(ufttfsdir, HpToolsLauncher_SCRIPT_NAME));
                WriteVerbose("*****Launcher***** " + launcherPath);

                aborterPath = Path.GetFullPath(Path.Combine(ufttfsdir, HpToolsAborter_SCRIPT_NAME));
                WriteVerbose("****aborter****** " + launcherPath);

                string propdir = Path.GetFullPath(Path.Combine(ufttfsdir, "props"));
                if (!Directory.Exists(propdir))
                    Directory.CreateDirectory(propdir);

                string resdir = Path.GetFullPath(Path.Combine(ufttfsdir, "res"));
                if (!Directory.Exists(resdir))
                    Directory.CreateDirectory(resdir);

                string timeSign = DateTime.Now.ToString("ddMMyyyyHHmmssSSS");
                paramFileName = Path.Combine(propdir, "props" + timeSign + ".txt");
                string resultsFileName = Path.Combine(resdir, "Results" + timeSign + ".xml");
                resultsFileName = resultsFileName.Replace("\\", "\\\\");

                properties.Add("resultsFilename", resultsFileName);

                if (!SaveProperties(paramFileName, properties))
                    return;

                WriteVerbose(string.Format("Properties saved in  : {0}", paramFileName));
                foreach (var prop in properties)
                    WriteVerbose(string.Format("{0} : {1}", prop.Key, prop.Value));

                int retCode = Run(launcherPath, paramFileName);

                if (retCode == 3)
                {
                    ThrowTerminatingError(new ErrorRecord(new ThreadInterruptedException(), "ClosedByUser", ErrorCategory.OperationStopped, ""));

                }
                else if (retCode == 0)
                {
                    WriteVerbose("return code: " + retCode);
                    //collateResults();
                }
            }
            catch (IOException ioe)
            {
                ThrowTerminatingError(new ErrorRecord(ioe, "IOException", ErrorCategory.ResourceExists, ""));
            }
            catch (ThreadInterruptedException e)
            {
                WriteError(new ErrorRecord(e, "ThreadInterruptedException", ErrorCategory.OperationStopped, "ThreadInterruptedException targer"));
                Run(aborterPath, paramFileName);
            }

           // return collateResults();
        }

        private bool SaveProperties(string paramsFile, Dictionary<string, string> properties)
        {
            bool result = true;             

               using (StreamWriter file = new StreamWriter(paramsFile, true))
                {
                    try
                    {
                    foreach (String prop in properties.Keys.ToArray())
                        if (!String.IsNullOrWhiteSpace(properties[prop]))
                            file.WriteLine(prop + "=" + properties[prop]);
                }
                    catch (Exception e)
                    {
                        result = false;
                        WriteError(new ErrorRecord(e, "", ErrorCategory.WriteError, ""));
                    }
                }

            return result;           
        }

        private int Run(string launcherPath, string paramFile)
        {
            try
            {
                ProcessStartInfo info = new ProcessStartInfo();
                info.Arguments = "-paramfile " + paramFile;
                info.FileName = launcherPath;
                info.UseShellExecute = false;

                info.RedirectStandardOutput = true;
                info.RedirectStandardError = true;

                Process launcher = new Process();
                launcher.StartInfo = info;

                launcher.Start();

                WriteObject(launcher.StandardError.ReadToEnd());
                WriteObject(launcher.StandardOutput.ReadToEnd());

                launcher.WaitForExit();

                return launcher.ExitCode;
            }
           
            catch (Exception e)
            {
                WriteError(new ErrorRecord(e, "ThreadInterruptedException", ErrorCategory.InvalidData, "ThreadInterruptedException targer"));
                return -1;
            }
        }

        private void collateResults()
        {

        }

        //private TaskResult collateResults(@NotNull final TaskContext taskContext)
        //   {
        //       try
        //       {
        //           TestResultHelper.CollateResults(testCollationService, taskContext);
        //           PrepareArtifacts(taskContext);
        //           return TaskResultBuilder.create(taskContext).checkTestFailures().build();
        //       }
        //       catch (Exception ex)
        //       {
        //           return TaskResultBuilder.create(taskContext).failed().build();
        //       }
        //   }

    }
}
