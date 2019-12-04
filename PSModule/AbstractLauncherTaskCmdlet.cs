using System;
using System.IO;
using System.Management.Automation;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Text;
using System.Collections.Concurrent;

namespace PSModule
{
    public abstract class AbstractLauncherTaskCmdlet : PSCmdlet
    {
        const string UFTFolder = "UFTWorking";
        const string HpToolsLauncher_SCRIPT_NAME = "HpToolsLauncher.exe";
        const string HpToolsAborter_SCRIPT_NAME = "HpToolsAborter.exe";

        private ConcurrentQueue<string> outputToProcess = new ConcurrentQueue<string>();
        private ConcurrentQueue<string> errorToProcess = new ConcurrentQueue<string>();

        public AbstractLauncherTaskCmdlet() {}

        public abstract Dictionary<string, string> GetTaskProperties();

        protected override void ProcessRecord()
        {
            Trace.WriteLine("CTRACE: DEBUG EXTENSION");
            string launcherPath = "";
            string aborterPath = "";
            string paramFileName = "";
            string resultsFileName = "";
           
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
                      
                launcherPath = Path.GetFullPath(Path.Combine(ufttfsdir, HpToolsLauncher_SCRIPT_NAME));
            
                aborterPath = Path.GetFullPath(Path.Combine(ufttfsdir, HpToolsAborter_SCRIPT_NAME));
             
                string propdir = Path.GetFullPath(Path.Combine(ufttfsdir, "props"));
               
                if (!Directory.Exists(propdir))
                    Directory.CreateDirectory(propdir);

                string resdir = Path.GetFullPath(Path.Combine(ufttfsdir, "res"));
             
                if (!Directory.Exists(resdir))
                    Directory.CreateDirectory(resdir);

                string timeSign = DateTime.Now.ToString("ddMMyyyyHHmmssSSS");

                paramFileName = Path.Combine(propdir, "Props" + timeSign + ".txt");
               
                resultsFileName = Path.Combine(resdir, "Results" + timeSign + ".xml");
              
                resultsFileName = resultsFileName.Replace("\\", "\\\\");
               
                /*if (!File.Exists(resultsFileName))
                {
                    Trace.WriteLine("CTRACE: result file does not exist (!!!!!!!!!!!!)");
                    WriteError(new ErrorRecord(new Exception("result file does not exist !!!!!!!!!!!!!"), "", ErrorCategory.WriteError, ""));

                    File.Create(resultsFileName).Dispose();
                }*/

                properties.Add("resultsFilename", resultsFileName);

                if (!SaveProperties(paramFileName, properties))
                {
                    WriteError(new ErrorRecord(new Exception("cannot save properties"), "", ErrorCategory.WriteError, ""));
                    return;
                }

                /*foreach (var prop in properties)
                {
                    WriteVerbose(string.Format("{0} : {1}", prop.Key, prop.Value));
                }*/

                int retCode = Run(launcherPath, paramFileName);
                WriteVerbose("Return code: {retCode}");
               
                CollateResults(resultsFileName, _launcherConsole.ToString(), resdir);
                CollateRetCode(resdir, retCode);
                /*if (retCode == 3) {
                    ThrowTerminatingError(new ErrorRecord(new ThreadInterruptedException(), "ClosedByUser", ErrorCategory.OperationStopped, ""));
                } else {
                    ThrowTerminatingError(new ErrorRecord(new ThreadInterruptedException(), "Task failed", ErrorCategory.OperationStopped, ""));
                }*/
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
        }

        private bool SaveProperties(string paramsFile, Dictionary<string, string> properties)
        {
            bool result = true;

            using (StreamWriter file = new StreamWriter(paramsFile, true))
            {
                try
                {
                    foreach (String prop in properties.Keys.ToArray())
                    {
                        file.WriteLine(prop + "=" + properties[prop]);
                    }

                }
                catch (Exception e)
                {
                    result = false;
                    WriteError(new ErrorRecord(e, "", ErrorCategory.WriteError, ""));
                }
            }

            return result;
        }

        private StringBuilder _launcherConsole = new StringBuilder();
        private int Run(string launcherPath, string paramFile)
        {
            _launcherConsole.Clear();
            try
            {
                ProcessStartInfo info = new ProcessStartInfo();
                info.UseShellExecute = false;
                info.Arguments = $" -paramfile \"{paramFile}\"";
                info.FileName = launcherPath;
                info.RedirectStandardOutput = true;
                info.RedirectStandardError = true;

                Process launcher = new Process();
                launcher.OutputDataReceived += Launcher_OutputDataReceived;
                launcher.ErrorDataReceived += Launcher_ErrorDataReceived;

                launcher.StartInfo = info;

                launcher.Start();

                launcher.BeginOutputReadLine();
                launcher.BeginErrorReadLine();

                while (!launcher.HasExited)
                {
                    if (outputToProcess.TryDequeue(out string line))
                    {
                        _launcherConsole.Append(line);
                        WriteObject(line);
                    }

                    if (errorToProcess.TryDequeue(out line))
                    {
                        _launcherConsole.Append(line);
                        WriteObject(line);
                    }
                }

                launcher.OutputDataReceived -= Launcher_OutputDataReceived;
                launcher.ErrorDataReceived -= Launcher_ErrorDataReceived;

                launcher.WaitForExit();

                return launcher.ExitCode;
            }

            catch (Exception e)
            {
                WriteError(new ErrorRecord(e, "ThreadInterruptedException", ErrorCategory.InvalidData, "ThreadInterruptedException targer"));
                return -1;
            }
        }

        private void Launcher_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            errorToProcess.Enqueue(e.Data);
        }   

        private void Launcher_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            outputToProcess.Enqueue(e.Data);
        }

        protected abstract string GetRetCodeFileName();

        protected virtual void CollateRetCode(string resdir, int retCode)
        {

            string fileName = GetRetCodeFileName();
            if (String.IsNullOrEmpty(fileName))
            {
                WriteError(new ErrorRecord(new Exception("Method GetRetCodeFileName() did not return a value"), "", ErrorCategory.WriteError, ""));
                return;
            }
            if (!Directory.Exists(resdir))
            {
                WriteError(new ErrorRecord(new DirectoryNotFoundException(resdir), "", ErrorCategory.WriteError, ""));
                return;
            }
            string retCodeFilename = Path.Combine(resdir, fileName);
            try
            {
                using (StreamWriter file = new StreamWriter(retCodeFilename, true))
                {
                    file.WriteLine(retCode.ToString());
                }
            }
            catch (Exception e)
            {
                WriteError(new ErrorRecord(e, "", ErrorCategory.WriteError, ""));
            }
        }

        protected virtual string GetReportFilename()
        {
            return String.Empty;
        }

        protected virtual void CollateResults(string resultFile, string log, string resdir)
        {
            if (!File.Exists(resultFile)) {
                WriteError(new ErrorRecord(new Exception("result file does not exist"), "", ErrorCategory.WriteError, ""));
                File.Create(resultFile).Dispose();
            }

            string reportFileName = GetReportFilename();

            if (String.IsNullOrEmpty(reportFileName))
            {
                WriteError(new ErrorRecord(new Exception("collate results, empty reportFileName "), "", ErrorCategory.WriteError, ""));
                return;
            }

            if ((String.IsNullOrEmpty(resultFile) || !File.Exists(resultFile)) && String.IsNullOrEmpty(log))
            {
                WriteError(new ErrorRecord(new FileNotFoundException($"No results file ({resultFile}) nor result log provided"), "", ErrorCategory.WriteError, ""));
                
                return;
            }

            //read result xml file
            string s = File.ReadAllText(resultFile);

            if (String.IsNullOrEmpty(s))
            {
                WriteError(new ErrorRecord(new FileNotFoundException("collate results, empty results file"), "", ErrorCategory.WriteError, ""));
                return;
            }
            List<Tuple<string, string>> links = GetRequiredLinksFromString(s);
            if (links == null || links.Count == 0)
            {
                links = GetRequiredLinksFromString(log);
                if (links == null || links.Count == 0)
                {
                    WriteError(new ErrorRecord(new FileNotFoundException("No report links in results file or log found"), "", ErrorCategory.WriteError, ""));
                    return;
                }
            }

            try
            {
                string reportPath = Path.Combine(resdir, reportFileName);

                using (StreamWriter file = new StreamWriter(Path.Combine(resdir, reportFileName), true))
                {
                    foreach (var link in links)
                    {
                        file.WriteLine($"[Report {link.Item2}]({link.Item1})  ");
                    }
                }
            }
            catch (Exception e)
            {
                WriteError(new ErrorRecord(e, "error writing the results", ErrorCategory.WriteError, ""));
            }
        }

        private List<Tuple<string, string>> GetRequiredLinksFromString(string s)
        {
            if (String.IsNullOrEmpty(s))
            {
                return null;
            }
            var results = new List<Tuple<string, string>>();
            try
            {
                //report link example: td://Automation.AUTOMATION.mydph0271.hpswlabs.adapps.hp.com:8080/qcbin/TestLabModule-000000003649890581?EntityType=IRun&amp;EntityID=1195091
                Match match1 = Regex.Match(s, "td://.+?EntityID=([0-9]+)");
                Match match2 = Regex.Match(s, "tds://.+?EntityID=([0-9]+)");
                while (match1.Success)
                {
                    results.Add(new Tuple<string, string>(match1.Groups[0].Value, match1.Groups[1].Value));
                    match1 = match1.NextMatch();
                }

                while (match2.Success)
                {
                    results.Add(new Tuple<string, string>(match2.Groups[0].Value, match2.Groups[1].Value));
                    match2 = match2.NextMatch();
                }
            }
            catch (Exception e)
            {
                WriteError(new ErrorRecord(e, "", ErrorCategory.WriteError, ""));
            }
            return results;
        }
    }
}
