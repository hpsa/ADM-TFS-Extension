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

namespace PSModule
{
    using H = Helper;
    public abstract class AbstractLauncherTaskCmdlet : PSCmdlet
    {
        #region - Private Constants

        private const string HpToolsLauncher_EXE = "HpToolsLauncher.exe";
        private const string HpToolsAborter_EXE = "HpToolsAborter.exe";
        private const string ReportConverter_EXE = "ReportConverter.exe";
        private const string UFT_LAUNCHER = "UFT_LAUNCHER";
        private const string PROPS = "props";
        private const string BUILD_NUMBER = "buildNumber";
        private const string DDMMYYYYHHMMSSSSS = "ddMMyyyyHHmmssSSS";
        private const string RESULTS_FILENAME = "resultsFilename";
        private const string STORAGE_ACCOUNT = "storageAccount";
        private const string CONTAINER = "container";
        private const string RUN_TYPE = "runType";
        private const string UPLOAD_ARTIFACT = "uploadArtifact";
        private const string ARTIFACT_TYPE = "artifactType";
        private const string REPORT_NAME = "reportName";
        private const string ARCHIVE_NAME = "archiveName";
        private const string YES = "yes";
        private const string JUNIT_REPORT_XML = "junit_report.xml";

        #endregion

        private readonly StringBuilder _launcherConsole = new StringBuilder();
        private readonly ConcurrentQueue<string> outputToProcess = new ConcurrentQueue<string>();
        private readonly ConcurrentQueue<string> errorToProcess = new ConcurrentQueue<string>();

        protected AbstractLauncherTaskCmdlet() { }

        public abstract Dictionary<string, string> GetTaskProperties();

        protected override void ProcessRecord()
        {
            string launcherPath, aborterPath = string.Empty, converterPath, paramFileName = string.Empty, resultsFileName;
            try
            {
                Dictionary<string, string> properties;
                try
                {
                    properties = GetTaskProperties();
                    if (properties == null || !properties.Any())
                    {
                        ThrowTerminatingError(new ErrorRecord(new Exception("Invalid or missing properties!"), nameof(GetTaskProperties), ErrorCategory.ParserError, string.Empty));
                        return;
                    }
                }
                catch (Exception e)
                {
                    ThrowTerminatingError(new ErrorRecord(e, nameof(GetTaskProperties), ErrorCategory.ParserError, string.Empty));
                    return;
                }

                string ufttfsdir = Environment.GetEnvironmentVariable(UFT_LAUNCHER);

                launcherPath = Path.GetFullPath(Path.Combine(ufttfsdir, HpToolsLauncher_EXE));
                aborterPath = Path.GetFullPath(Path.Combine(ufttfsdir, HpToolsAborter_EXE));
                converterPath = Path.GetFullPath(Path.Combine(ufttfsdir, ReportConverter_EXE));

                string propdir = Path.GetFullPath(Path.Combine(ufttfsdir, PROPS));

                if (!Directory.Exists(propdir))
                    Directory.CreateDirectory(propdir);

                string resdir = Path.GetFullPath(Path.Combine(ufttfsdir, $@"res\Report_{properties[BUILD_NUMBER]}"));

                if (!Directory.Exists(resdir))
                    Directory.CreateDirectory(resdir);

                string timeSign = DateTime.Now.ToString(DDMMYYYYHHMMSSSSS);

                paramFileName = Path.Combine(propdir, $"Props{timeSign}.txt");
                resultsFileName = Path.Combine(resdir, $"Results{timeSign}.xml");

                properties.Add(RESULTS_FILENAME, resultsFileName.Replace(@"\", @"\\")); // double backslashes are expected by HpToolsLauncher.exe (JavaProperties.cs, in LoadInternal method)

                if (!SaveProperties(paramFileName, properties))
                {
                    WriteError(new ErrorRecord(new Exception("cannot save properties"), string.Empty, ErrorCategory.WriteError, string.Empty));
                    return;
                }

                //run the build task
                Run(launcherPath, paramFileName);
               
                //collect results
                CollateResults(resultsFileName, _launcherConsole.ToString(), resdir);

                RunStatus runStatus = RunStatus.FAILED;
                if (File.Exists(resultsFileName) && new FileInfo(resultsFileName).Length > 0)//if results file exists
                {
                    //create UFT report from the results file
                    var listReport = H.ReadReportFromXMLFile(resultsFileName);

                    string storageAccount = properties.GetValueOrDefault(STORAGE_ACCOUNT, string.Empty);
                    string container = properties.GetValueOrDefault(CONTAINER, string.Empty);

                    var runType = (RunType)Enum.Parse(typeof(RunType), properties[RUN_TYPE]);
                    //create html report
                    if (runType == RunType.FileSystem && properties[UPLOAD_ARTIFACT] == YES)
                    {
                        var artifactType = (ArtifactType)Enum.Parse(typeof(ArtifactType), properties[ARTIFACT_TYPE]);
                        H.CreateSummaryReport(resdir, runType, listReport, true, artifactType, storageAccount, container, properties[REPORT_NAME], properties[ARCHIVE_NAME]);
                    }
                    else
                    {
                        H.CreateSummaryReport(resdir, runType, listReport);
                    }
                    //get task return code
                    runStatus = H.GetRunStatus(listReport);
                    int totalTests = H.GetNumberOfTests(listReport, out IDictionary<string, int> nrOfTests);
                    H.CreateRunSummary(runStatus, totalTests, nrOfTests, resdir);
                    
                    var reportFolders = new List<string>();
                    foreach (var item in listReport)
                    {
                        if (!item.ReportPath.IsNullOrWhiteSpace())
                            reportFolders.Add(item.ReportPath);
                    }

                    if (runType == RunType.FileSystem && reportFolders.Any())
                    {
                        //run junit report converter
                        string outputFileReport = Path.Combine(resdir, JUNIT_REPORT_XML);
                        RunConverter(converterPath, outputFileReport, reportFolders);
                        if (File.Exists(outputFileReport) && new FileInfo(outputFileReport).Length > 0 && (nrOfTests[H.FAIL] > 0 || nrOfTests[H.ERROR] > 0))
                        {
                            IDictionary<string, IList<ReportMetaData>> steps = new Dictionary<string, IList<ReportMetaData>>();
                            H.ReadReportFromXMLFile(outputFileReport, true, ref steps);
                            H.CreateFailedStepsReport(steps, resdir);
                        }
                    }
                }

                CollateRetCode(resdir, (int)runStatus);
            }
            catch (IOException ioe)
            {
                WriteError(new ErrorRecord(ioe, nameof(IOException), ErrorCategory.ResourceExists, string.Empty));
            }
            catch (ThreadInterruptedException e)
            {
                WriteError(new ErrorRecord(e, nameof(ThreadInterruptedException), ErrorCategory.OperationStopped, "ThreadInterruptedException target"));
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
                WriteError(new ErrorRecord(e, nameof(ThreadInterruptedException), ErrorCategory.InvalidData, "ThreadInterruptedException target"));
                return -1;
            }
        }

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
                    info.Arguments += $" \"{reportFolder}\"";
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
                        WriteObject(line);
                    }

                    if (errorToProcess.TryDequeue(out line))
                    {
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
                WriteError(new ErrorRecord(e, nameof(ThreadInterruptedException), ErrorCategory.InvalidData, "ThreadInterruptedException target"));
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
            if (fileName.IsNullOrWhiteSpace())
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
                using StreamWriter file = new StreamWriter(retCodeFilename, true);
                file.WriteLine(retCode.ToString());
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

            if (reportFileName.IsNullOrWhiteSpace())
            {
                WriteError(new ErrorRecord(new Exception("collate results, empty reportFileName "), string.Empty, ErrorCategory.WriteError, string.Empty));
                return;
            }

            if ((resultFile.IsNullOrWhiteSpace() || !File.Exists(resultFile)) && log.IsNullOrWhiteSpace())
            {
                WriteError(new ErrorRecord(new FileNotFoundException($"No results file ({resultFile}) nor result log provided"), string.Empty, ErrorCategory.WriteError, string.Empty));

                return;
            }

            //read result xml file
            string s = File.ReadAllText(resultFile);

            if (s.IsNullOrWhiteSpace())
            {
                WriteError(new ErrorRecord(new FileNotFoundException("collate results, empty results file"), string.Empty, ErrorCategory.WriteError, string.Empty));
                return;
            }
            var links = GetRequiredLinksFromString(s);
            if (links.IsNullOrEmpty())
            {
                links = GetRequiredLinksFromString(log);
                if (links.IsNullOrEmpty())
                {
                    WriteError(new ErrorRecord(new FileNotFoundException("No report links in results file or log found"), string.Empty, ErrorCategory.WriteError, string.Empty));
                    return;
                }
            }

            try
            {
                string reportPath = Path.Combine(resdir, reportFileName);
                using StreamWriter file = new StreamWriter(reportPath, true);
                foreach (var link in links)
                {
                    file.WriteLine($"[Report {link.Item2}]({link.Item1})  ");
                }
            }
            catch (Exception e)
            {
                WriteError(new ErrorRecord(e, "error writing the results", ErrorCategory.WriteError, string.Empty));
            }
        }

        private List<Tuple<string, string>> GetRequiredLinksFromString(string s)
        {
            if (s.IsNullOrWhiteSpace())
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
