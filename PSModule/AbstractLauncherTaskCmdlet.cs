using System;
using System.IO;
using System.Management.Automation;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Text;

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
            //MessageBox.Show("DEBUG");
            string aborterPath = "";
            string paramFileName = "";

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
                WriteVerbose("****Aborter****** " + aborterPath);

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
                {
                    return;
                }

                WriteVerbose(string.Format("Properties saved in  : {0}", paramFileName));
                foreach (var prop in properties)
                {
                    WriteVerbose(string.Format("{0} : {1}", prop.Key, prop.Value));
                }

                int retCode = Run(launcherPath, paramFileName);
                WriteVerbose($"Return code: {retCode}");

                CollateResults(resultsFileName, _launcherConsole.ToString(), resdir);
                if (retCode != 0)
                { 
                    CollateRetCode(resdir, retCode);
                }
                //WriteObject(retCode);
                //else if (retCode == 3)
                //{
                //    ThrowTerminatingError(new ErrorRecord(new ThreadInterruptedException(), "ClosedByUser", ErrorCategory.OperationStopped, ""));
                //}
                //else
                //{
                //    ThrowTerminatingError(new ErrorRecord(new ThreadInterruptedException(), "Task failed", ErrorCategory.OperationStopped, ""));
                //}
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
                //info.RedirectStandardError = true;

                Process launcher = new Process();

                launcher.StartInfo = info;

                launcher.Start();

                while (!launcher.StandardOutput.EndOfStream)// || !launcher.StandardError.EndOfStream)
                {
                    //if (!launcher.StandardOutput.EndOfStream)
                    //{
                    string line = launcher.StandardOutput.ReadLine();
                    _launcherConsole.Append(line);
                    WriteObject(line);
                    //}
                    //if (!launcher.StandardError.EndOfStream)
                    //{
                    //    string lineErr = launcher.StandardError.ReadLine();
                    //    _launcherConsole.Append(lineErr);
                    //    WriteObject(lineErr);
                    //}
                }
                launcher.WaitForExit();
                return launcher.ExitCode;
            }

            catch (Exception e)
            {
                WriteError(new ErrorRecord(e, "ThreadInterruptedException", ErrorCategory.InvalidData, "ThreadInterruptedException targer"));
                return -1;
            }
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
            string reportFileName = GetReportFilename();
            if (String.IsNullOrEmpty(reportFileName))
            {
                return;
            }
            if ((String.IsNullOrEmpty(resultFile) || !File.Exists(resultFile)) && String.IsNullOrEmpty(log))
            {
                WriteError(new ErrorRecord(new FileNotFoundException($"No results file ({resultFile}) nor result log provided"), "", ErrorCategory.WriteError, ""));
                return;
            }
            string s = File.ReadAllText(resultFile);
            if (String.IsNullOrEmpty(s))
            {
                WriteVerbose($"Empty results file: {resultFile}");
                return;
            }
            List<Tuple<string, string>> links = GetRequiredLinksFromString(s);
            if (links == null || links.Count == 0)
            {
                links = GetRequiredLinksFromString(log);
                if (links == null || links.Count == 0)
                {
                    WriteVerbose($"No report likns in results file or log found: {resultFile}");
                    return;
                }
            }

            try
            {
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
                WriteError(new ErrorRecord(e, "", ErrorCategory.WriteError, ""));
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
                Match match = Regex.Match(s, "td://.+?EntityID=([0-9]+)");
                while (match.Success)
                {
                    results.Add(new Tuple<string, string>(match.Groups[0].Value, match.Groups[1].Value));
                    match = match.NextMatch();
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
