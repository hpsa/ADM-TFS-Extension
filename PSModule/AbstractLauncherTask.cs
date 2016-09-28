using System;
using System.IO;
using System.Management.Automation;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace PSModule
{
    public abstract class AbstractLauncherTask : PSCmdlet
    {
        const string UFTFolder = "UFTWorking";
        const string HpToolsLauncher_SCRIPT_NAME = "HpToolsLauncher.exe";
        const string HpToolsAborter_SCRIPT_NAME = "HpToolsAborter.exe";

        protected override void ProcessRecord()
        {
            string aborterPath = "";
            string paramFileName = "";

            //System.Diagnostics.Debugger.Launch();

            try
            {
                int myId = 0;
                string myActivity = "Govering properties ";
                string myStatus = "Start";
                ProgressRecord pr = new ProgressRecord(myId, myActivity, myStatus);
                pr.CurrentOperation = "Task execution: ";

                WriteProgress(pr);
                Thread.Sleep(3000);

                Dictionary<string, string> properties = new Dictionary<string, string>();
                try
                {
                    properties = GetTaskProperties();
                }
                catch (Exception e)
                {
                    WriteError(new ErrorRecord(e, "Properties", ErrorCategory.ParserError, "targetObject"));
                    return;
                }

                WriteObject(properties);

                pr.Activity = "Save to temprary file";

                //TODO see in bamboo

                string launcherPath = Path.GetFullPath(Path.Combine(UFTFolder, HpToolsLauncher_SCRIPT_NAME));
                WriteObject("*****Launcher***** " + launcherPath);

                aborterPath = Path.GetFullPath(Path.Combine(UFTFolder, HpToolsAborter_SCRIPT_NAME));
                WriteObject("****aborter****** " + launcherPath);
                // buildLogger.addBuildLogEntry("********** " + aborterPath);
                // buildLogger.addBuildLogEntry("********** " + error);
                //if (!error.isEmpty())
                //{
                //    buildLogger.addErrorLogEntry(error);
                //    return TaskResultBuilder.create(taskContext).failedWithError().build();
                //}


                string timeSign = DateTime.Now.ToString("ddMMyyyyHHmmssSSS");
                paramFileName = Path.GetFullPath(Path.Combine("UFTWorking", "props" + timeSign + ".txt"));
                string resultsFileName = Path.GetFullPath(Path.Combine("UFTWorking", "Results" + timeSign + ".xml"));

                properties.Add("resultsFilename", resultsFileName);

                if (!SaveProperties(paramFileName, properties))
                    return;

                pr.StatusDescription = "Complited";
                pr.PercentComplete = 10;
                WriteProgress(pr);

                pr.Activity = "Launcher execution.";
                pr.StatusDescription = "Start";

                WriteProgress(pr);

                int retCode = Run(launcherPath, paramFileName);

                if (retCode == 3)
                {
                    WriteError(new ErrorRecord(new ThreadInterruptedException(), "ClosedByUser", ErrorCategory.OperationStopped, ""));
                }
                else if (retCode == 0)
                {
                    //collateResults();
                }
            }
            catch (IOException ioe)
            {
                WriteError(new ErrorRecord(ioe, "IOException", ErrorCategory.ResourceExists, ""));
            }
            catch (ThreadInterruptedException e)
            {
                WriteError(new ErrorRecord(e, "ThreadInterruptedException", ErrorCategory.OperationStopped, "ThreadInterruptedException targer"));
                Run(aborterPath, paramFileName);
            }

            /*catch (InterruptedException e) {
            //buildLogger.addErrorLogEntry("Aborted by user. Aborting process.");
            try
            {
                run(wd, aborterPath, paramsFile.getAbsolutePath(), buildLogger);
            }
            catch (IOException ioe)
            {
                //buildLogger.addErrorLogEntry(ioe.getMessage(), ioe);
                return TaskResultBuilder.create(taskContext).failedWithError().build();
            }
            catch (InterruptedException ie)
            {
                //buildLogger.addErrorLogEntry(ie.getMessage(), ie);
                return TaskResultBuilder.create(taskContext).failedWithError().build();
            }
            }*/

            //return collateResults(taskContext);
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

        public abstract Dictionary<string, string> GetTaskProperties();

        private string extractBinaryResource(string pathToExtract, string resourceName)
        {


            return "";
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

                Process launcher = new Process();
                launcher.StartInfo = info;

                //launcher.OutputDataReceived += (sender, args) => WriteObject(args.Data);

                launcher.Start();
                launcher.BeginOutputReadLine();

                WriteObject(launcher.StandardError.ReadToEnd());
                WriteObject(launcher.StandardOutput.ReadToEnd());

                launcher.WaitForExit();

                return launcher.ExitCode;
            }
           
            catch (Exception e)
            {
                WriteError(new ErrorRecord(e, "", ErrorCategory.InvalidData, ""));
                return -1;
            }
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
