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
using PSModule.Models;
using System.Xml;

namespace PSModule
{
    public abstract class AbstractLauncherTaskCmdlet : PSCmdlet
    {
        private const string HpToolsLauncher_EXE = "HpToolsLauncher.exe";
        private const string HpToolsAborter_EXE = "HpToolsAborter.exe";
        private const string ReportConverter_EXE = "ReportConverter.exe";

        private ConcurrentQueue<string> outputToProcess = new ConcurrentQueue<string>();
        private ConcurrentQueue<string> errorToProcess = new ConcurrentQueue<string>();

        public AbstractLauncherTaskCmdlet() { }

        public abstract Dictionary<string, string> GetTaskProperties();

        protected override void ProcessRecord()
        {
            string launcherPath, aborterPath = string.Empty, converterPath, paramFileName = string.Empty, resultsFileName;

            try
            {
                Dictionary<string, string> properties = new Dictionary<string, string>();
                try
                {
                    properties = GetTaskProperties();
                }
                catch (Exception e)
                {
                    ThrowTerminatingError(new ErrorRecord(e, "GetTaskProperties", ErrorCategory.ParserError, string.Empty));
                }

                string ufttfsdir = Environment.GetEnvironmentVariable("UFT_LAUNCHER");

                launcherPath = Path.GetFullPath(Path.Combine(ufttfsdir, HpToolsLauncher_EXE));

                aborterPath = Path.GetFullPath(Path.Combine(ufttfsdir, HpToolsAborter_EXE));

                converterPath = Path.GetFullPath(Path.Combine(ufttfsdir, ReportConverter_EXE));

                string propdir = Path.GetFullPath(Path.Combine(ufttfsdir, "props"));

                if (!Directory.Exists(propdir))
                    Directory.CreateDirectory(propdir);

                string resdir = Path.GetFullPath(Path.Combine(ufttfsdir, $@"res\Report_{properties["buildNumber"]}"));

                if (!Directory.Exists(resdir))
                    Directory.CreateDirectory(resdir);

                string timeSign = DateTime.Now.ToString("ddMMyyyyHHmmssSSS");

                paramFileName = Path.Combine(propdir, $"Props{timeSign}.txt");
                resultsFileName = Path.Combine(resdir, $"Results{timeSign}.xml");

                properties.Add("resultsFilename", resultsFileName);

                if (!SaveProperties(paramFileName, properties))
                {
                    WriteError(new ErrorRecord(new Exception("cannot save properties"), string.Empty, ErrorCategory.WriteError, string.Empty));
                    return;
                }

                //run the build task
                Run(launcherPath, paramFileName);
               
                //collect results
                CollateResults(resultsFileName, _launcherConsole.ToString(), resdir);

                int retCode = -1;
                if (File.Exists(resultsFileName) && (new FileInfo(resultsFileName).Length > 0))//if results file exists
                {
                    //create UFT report from the results file
                    List<ReportMetaData> listReport = Helper.ReadReportFromXMLFile(resultsFileName, new Dictionary<string, List<ReportMetaData>>(), false);

                    string storageAccount = properties.GetValueOrDefault("storageAccount", string.Empty);
                    string container = properties.GetValueOrDefault("container", string.Empty);

                    RunType runType = (RunType)Enum.Parse(typeof(RunType), properties["runType"]);
                    //create html report
                    if (runType == RunType.FileSystem && properties["uploadArtifact"].Equals("yes"))
                    {
                        Helper.CreateSummaryReport(ufttfsdir, ref listReport,
                                                        properties["uploadArtifact"], properties["artifactType"], storageAccount, container,
                                                        properties["reportName"], properties["archiveName"], properties["buildNumber"],
                                                        properties["runType"]);
                    }
                    else
                    {
                        Helper.CreateSummaryReport(ufttfsdir, ref listReport,
                                                  string.Empty, string.Empty, string.Empty, string.Empty,
                                                  string.Empty, string.Empty,
                                                  properties["buildNumber"],
                                                  properties["runType"]);
                    }
                    //get task return code
                    retCode = Helper.GetErrorCode(listReport);
                    
                    //create run status summary report
                    string runStatus;
                   
                    switch (retCode)
                    {
                        case 0: runStatus = "PASSED"; break;
                        case -1: runStatus = "FAILED"; break;
                        case -2: runStatus = "UNSTABLE"; break;
                        case -3: runStatus = "CLOSED BY USER"; break;
                        default: runStatus = "UNDEFINED"; break;
                    }

                    var nrOfTests = new Dictionary<string, int>
                    {
                        { "Passed", 0 },
                        { "Failed", 0 },
                        { "Error", 0 },
                        { "Warning", 0 }
                    };

                    int totalTests = Helper.GetNumberOfTests(listReport, ref nrOfTests);
                    
                    Helper.CreateRunStatusSummary(runStatus, totalTests, nrOfTests, ufttfsdir, properties["buildNumber"], storageAccount, container);
                    
                    var testNames = new List<string>();
                    var reportFolders = new List<string>();
                    foreach(var item in listReport)
                    {
                        testNames.Add(item.getDisplayName().Substring(item.getDisplayName().LastIndexOf(@"\") + 1));
                        reportFolders.Add(item.getReportPath());
                    }

                    if (runType == RunType.FileSystem)
                    {
                        //run junit report converter
                        string outputFileReport = Path.Combine(resdir, "junit_report.xml");
                        RunConverter(converterPath, outputFileReport, reportFolders);
                        var steps = new Dictionary<string, List<ReportMetaData>>();
                        List<ReportMetaData> junitReportList = Helper.ReadReportFromXMLFile(outputFileReport, steps, true);
                        if (nrOfTests["Failed"] > 0 || nrOfTests["Error"] > 0)
                        {
                            Helper.CreateJUnitReport(steps, ufttfsdir, properties["buildNumber"]);
                        }
                    }
                }

                CollateRetCode(resdir, retCode);
            }
            catch (IOException ioe)
            {
                WriteError(new ErrorRecord(ioe, "IOException", ErrorCategory.ResourceExists, string.Empty));
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
                    foreach (string prop in properties.Keys.ToArray())
                    {
                        file.WriteLine(prop + "=" + properties[prop]);
                    }

                }
                catch (Exception e)
                {
                    result = false;
                    WriteError(new ErrorRecord(e, string.Empty, ErrorCategory.WriteError, string.Empty));
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
                ProcessStartInfo info = new ProcessStartInfo
                {
                    UseShellExecute = false,
                    Arguments = $" -paramfile \"{paramFile}\"",
                    FileName = launcherPath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                Process launcher = new Process { StartInfo = info };

                launcher.OutputDataReceived += Launcher_OutputDataReceived;
                launcher.ErrorDataReceived += Launcher_ErrorDataReceived;

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
                WriteError(new ErrorRecord(e, "ThreadInterruptedException", ErrorCategory.InvalidData, "ThreadInterruptedException target"));
                return -1;
            }
        }

        private StringBuilder _converterConsole = new StringBuilder();

        private void RunConverter(string converterPath, string outputfile, List<string> inputReportFolders)
        {
            try
            {
                var info = new ProcessStartInfo
                {
                    UseShellExecute = false,
                    Arguments = $" -j \"{outputfile}\" --aggregate",
                    FileName = converterPath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                foreach (var reportFolder in inputReportFolders)
                {
                    info.Arguments += " \"" + reportFolder + "\"";
                }
                
                Process converter = new Process { StartInfo = info };

                converter.OutputDataReceived += Launcher_OutputDataReceived;
                converter.ErrorDataReceived += Launcher_ErrorDataReceived;

                converter.Start();

                converter.BeginOutputReadLine();
                converter.BeginErrorReadLine();

                while (!converter.HasExited)
                {
                    if (outputToProcess.TryDequeue(out string line))
                    {
                        _converterConsole.Append(line);
                        WriteObject(line);
                    }

                    if (errorToProcess.TryDequeue(out line))
                    {
                        _converterConsole.Append(line);
                        WriteObject(line);
                    }
                }

                converter.OutputDataReceived -= Launcher_OutputDataReceived;
                converter.ErrorDataReceived -= Launcher_ErrorDataReceived;

                converter.WaitForExit();

               // return launcher.ExitCode;
            }
            catch (Exception e)
            {
                WriteError(new ErrorRecord(e, "ThreadInterruptedException", ErrorCategory.InvalidData, "ThreadInterruptedException target"));
               // return -1;
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
            if (string.IsNullOrEmpty(fileName))
            {
                WriteError(new ErrorRecord(new Exception("Method GetRetCodeFileName() did not return a value"), string.Empty, ErrorCategory.WriteError, string.Empty));
                return;
            }
            if (!Directory.Exists(resdir))
            {
                WriteError(new ErrorRecord(new DirectoryNotFoundException(resdir), string.Empty, ErrorCategory.WriteError, string.Empty));
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
                WriteError(new ErrorRecord(e, string.Empty, ErrorCategory.WriteError, string.Empty));
            }
        }

        protected virtual string GetReportFilename()
        {
            return string.Empty;
        }

        protected virtual void CollateResults(string resultFile, string log, string resdir)
        {
            if (!File.Exists(resultFile))
            {
                WriteError(new ErrorRecord(new Exception("result file does not exist"), string.Empty, ErrorCategory.WriteError, string.Empty));
                File.Create(resultFile).Dispose();
            }

            string reportFileName = GetReportFilename();

            if (string.IsNullOrEmpty(reportFileName))
            {
                WriteError(new ErrorRecord(new Exception("collate results, empty reportFileName "), string.Empty, ErrorCategory.WriteError, string.Empty));
                return;
            }

            if ((string.IsNullOrEmpty(resultFile) || !File.Exists(resultFile)) && string.IsNullOrEmpty(log))
            {
                WriteError(new ErrorRecord(new FileNotFoundException($"No results file ({resultFile}) nor result log provided"), string.Empty, ErrorCategory.WriteError, string.Empty));

                return;
            }

            //read result xml file
            string s = File.ReadAllText(resultFile);

            if (string.IsNullOrEmpty(s))
            {
                WriteError(new ErrorRecord(new FileNotFoundException("collate results, empty results file"), string.Empty, ErrorCategory.WriteError, string.Empty));
                return;
            }
            List<Tuple<string, string>> links = GetRequiredLinksFromString(s);
            if (links == null || links.Count == 0)
            {
                links = GetRequiredLinksFromString(log);
                if (links == null || links.Count == 0)
                {
                    WriteError(new ErrorRecord(new FileNotFoundException("No report links in results file or log found"), string.Empty, ErrorCategory.WriteError, string.Empty));
                    return;
                }
            }

            try
            {
                string reportPath = Path.Combine(resdir, reportFileName);
                using (StreamWriter file = new StreamWriter(reportPath, true))
                {
                    foreach (var link in links)
                    {
                        file.WriteLine($"[Report {link.Item2}]({link.Item1})  ");
                    }
                }
            }
            catch (Exception e)
            {
                WriteError(new ErrorRecord(e, "error writing the results", ErrorCategory.WriteError, string.Empty));
            }
        }

        private List<Tuple<string, string>> GetRequiredLinksFromString(string s)
        {
            if (string.IsNullOrEmpty(s))
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
                WriteError(new ErrorRecord(e, string.Empty, ErrorCategory.WriteError, string.Empty));
            }
            return results;
        }
    }
}
