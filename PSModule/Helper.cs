using PSModule.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Xml;

namespace PSModule
{
    class Helper
    {
        #region - Private & Internal Constants

        private const string IMG_LINK_PREFIX = "https://extensionado.blob.core.windows.net/uft-extension-images";
        internal const string PASS = "pass";
        internal const string FAIL = "fail";
        internal const string ERROR = "error";
        internal const string WARNING = "warning";
        private const string DIEZ = "#";
        private const char DASH = '-';
        private const char COLON = ':';
        private const string NAME = "name";
        private const string REPORT = "report";
        private const string STATUS = "status";
        private const string TIME = "time";
        private const string FAILURE = "failure";
        private const string MESSAGE = "message";
        private const string SYSTEM_OUT = "system-out";
        private const string TEST_NAME = "Test name";
        private const string TIMESTAMP = "Timestamp";
        private const string FAILED_STEPS = "Failed steps";
        private const string DURATIONS = "Duration(s)";
        private const string ERROR_DETAILS = "Error details";
        private const string RUN_STATUS = "Run status";
        private const string TOTAL_TESTS = "Total tests";
        private const string _STATUS = "Status";
        private const string NO_OF_TESTS = "No. of tests";
        private const string PASSING_RATE = "Passing rate";
        private const string STYLE = "style";
        private const string UFT_REPORT = "UFT report";
        private const string UFT_REPORT_ARCHIVE = "UFT report archive";
        private const string VIEW_REPORT = "View report";
        private const string DOWNLOAD = "Download";

        private const string CENTER = "center";
        private const string LEFT = "left";
        private const string RIGHT = "right";
        private const string _50 = "50";
        private const string _80 = "80";
        private const string _100 = "100";
        private const string _150 = "150";
        private const string _300 = "300";
        private const string _600 = "600";
        private const string HEIGHT_30PX = "height: 30px;";
        private const string FONT_WEIGHT_BOLD = "font-weight: bold;";
        private const string FONT_WEIGHT_BOLD_UNDERLINE = "font-weight: bold; text-decoration: underline;";

        #endregion

        public static IList<ReportMetaData> ReadReportFromXMLFile(string reportPath)
        {
            IDictionary<string, IList<ReportMetaData>> testSteps = null;
            return ReadReportFromXMLFile(reportPath, false, ref testSteps);
        }
        public static IList<ReportMetaData> ReadReportFromXMLFile(string reportPath, bool isJUnitReport, ref IDictionary<string, IList<ReportMetaData>> testSteps)
        {
            var listReport = new List<ReportMetaData>();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(reportPath);

            foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes) //inside <testsuite> node 
            {
                var steps = new List<ReportMetaData>();
                string testName = string.Empty;
                if (isJUnitReport)
                {
                    string currentTest = node.Attributes[NAME].Value;
                    testName = currentTest.Substring(currentTest.LastIndexOf(DASH) + 1);
                }

                foreach (XmlNode currentNode in node) //inside <testcase> nodes
                {
                    var reportmetadata = new ReportMetaData();
                    var attributesList = currentNode.Attributes;
                    foreach (XmlAttribute attribute in attributesList)
                    {
                        switch (attribute.Name)
                        {
                            case NAME  : reportmetadata.setDisplayName(attribute.Value); break;
                            case REPORT: reportmetadata.setReportPath(attribute.Value); break;
                            case STATUS: reportmetadata.setStatus(attribute.Value); break;
                            case TIME  : reportmetadata.setDuration(attribute.Value); break;
                            default    : break;
                        }
                    }

                    if (isJUnitReport)
                    {
                        //remove the number in front of each step
                        string stepName = reportmetadata.getDisplayName();
                        if (stepName?.StartsWith(DIEZ) == true)
                        {
                            reportmetadata.setDisplayName(stepName.Substring(stepName.IndexOf(COLON) + 1));
                        }
                    }

                    var nodes = currentNode.ChildNodes;
                    foreach (XmlNode xmlNode in nodes)//inside nodes in <testcase> nodes
                    {
                        if (xmlNode.Name == FAILURE)
                        {
                            foreach (XmlAttribute attribute in xmlNode.Attributes)
                            {
                                if (attribute.Name == MESSAGE)
                                {
                                    reportmetadata.setErrorMessage(attribute.Value);
                                    reportmetadata.setStatus(FAIL);
                                }
                            }
                        }
                        if (xmlNode.Name == SYSTEM_OUT)
                        {
                            reportmetadata.setDateTime(xmlNode.InnerText.Substring(0, 19));
                        }
                    }
                    if (isJUnitReport)
                    {
                        steps.Add(reportmetadata);
                    }
                    listReport.Add(reportmetadata);
                }
                if (isJUnitReport)
                {
                    testSteps.Add(testName, steps);
                }
            }

            return listReport;
        }

        public static RunStatus GetRunStatus(IList<ReportMetaData> listReport)
        {
            var errorCode = RunStatus.PASSED;
            int passedTests = 0;
            int failedTests = 0;

            foreach (ReportMetaData report in listReport)
            {
                if (report.getStatus() == PASS)
                {
                    passedTests++;
                }
                else if (report.getStatus().In(ERROR, FAIL))
                {
                    failedTests++;
                }
            }

            if (passedTests > 0 && failedTests > 0)
            {
                errorCode = RunStatus.UNSTABLE;
            }
            else if (passedTests == 0 && failedTests > 0)
            {
                errorCode = RunStatus.FAILED;
            }

            return errorCode;
        }

        public static int GetNumberOfTests(IList<ReportMetaData> listReport, out IDictionary<string, int> nrOfTests)
        {
            nrOfTests = new Dictionary<string, int>
            {
                { PASS, 0 },
                { FAIL, 0 },
                { ERROR, 0 },
                { WARNING, 0 }
            };

            foreach (ReportMetaData item in listReport)
            {
                nrOfTests[item.getStatus()]++;
            }

            return listReport.Count;
        }

        public static void CreateSummaryReport(string uftWorkingFolder, string buildNumber, RunType runType, ref IList<ReportMetaData> reportList,
                                               bool uploadArtifact = false, ArtifactType artifactType = ArtifactType.None,
                                               string storageAccount = "", string container = "", string reportName = "", string archiveName = "")
        {
            var table = new HtmlTable();
            var header = new HtmlTableRow();
            var h1 = new HtmlTableCell { InnerText = TEST_NAME, Width = _100, Align = CENTER };
            h1.Attributes.Add(STYLE, FONT_WEIGHT_BOLD);
            header.Cells.Add(h1);

            var h2 = new HtmlTableCell { InnerText = TIMESTAMP, Width = _150, Align = CENTER };
            h2.Attributes.Add(STYLE, FONT_WEIGHT_BOLD);
            header.Cells.Add(h2);

            var h3 = new HtmlTableCell { InnerText = _STATUS, Width = _50, Align = CENTER };
            h3.Attributes.Add(STYLE, FONT_WEIGHT_BOLD);
            header.Cells.Add(h3);

            if (runType == RunType.FileSystem && uploadArtifact)
            {
                if (artifactType.In(ArtifactType.onlyReport, ArtifactType.bothReportArchive))
                {
                    var h4 = new HtmlTableCell { InnerText = UFT_REPORT, Width = _100, Align = CENTER };
                    h4.Attributes.Add(STYLE, FONT_WEIGHT_BOLD);
                    header.Cells.Add(h4);

                    if (artifactType == ArtifactType.bothReportArchive)
                    {
                        var h5 = new HtmlTableCell { InnerText = UFT_REPORT_ARCHIVE, Width = _150, Align = CENTER };
                        h5.Attributes.Add(STYLE, FONT_WEIGHT_BOLD);
                        header.Cells.Add(h5);
                    }
                }
                else if (artifactType == ArtifactType.onlyArchive)
                {
                    var h4 = new HtmlTableCell { InnerText = UFT_REPORT_ARCHIVE, Width = _150, Align = CENTER };
                    h4.Attributes.Add(STYLE, FONT_WEIGHT_BOLD);
                    header.Cells.Add(h4);
                }
            }

            header.BgColor = KnownColor.Azure.ToString();
            table.Rows.Add(header);

            //create table content
            int index = 1;
            var linkPrefix = $"https://{storageAccount}.blob.core.windows.net/{container}";
            var zipLinkPrefix = $"{linkPrefix}/{archiveName}";
            var htmlLinkPrefix = $"{linkPrefix}/{reportName}";
            foreach (ReportMetaData report in reportList)
            {
                var row = new HtmlTableRow();
                var cell1 = new HtmlTableCell { InnerText = GetTestName(report.getDisplayName()), Align = CENTER };
                row.Cells.Add(cell1);

                var cell2 = new HtmlTableCell { InnerText = report.getDateTime(), Align = CENTER };
                row.Cells.Add(cell2);

                var cell3 = new HtmlTableCell { Align = CENTER };
                cell3.Controls.Add(new HtmlImage { Src = $"{IMG_LINK_PREFIX}/{report.getStatus()}.svg" });
                row.Cells.Add(cell3);

                if (runType == RunType.FileSystem && uploadArtifact && !report.getReportPath().IsNullOrWhiteSpace())
                {
                    if (artifactType.In(ArtifactType.onlyReport, ArtifactType.bothReportArchive))
                    {
                        var cell4 = new HtmlTableCell { Align = CENTER };
                        var reportLink = new HtmlAnchor { HRef = $"{htmlLinkPrefix}_{index}.html", InnerText = VIEW_REPORT };
                        cell4.Controls.Add(reportLink);
                        row.Cells.Add(cell4);

                        if (artifactType == ArtifactType.bothReportArchive)
                        {
                            var cell5 = new HtmlTableCell { Align = CENTER };
                            cell5.Controls.Add(new HtmlAnchor { HRef = $"{zipLinkPrefix}_{index}.zip", InnerText = DOWNLOAD });
                            row.Cells.Add(cell5);
                        }
                    }
                    else if (artifactType == ArtifactType.onlyArchive)
                    {
                        var cell4 = new HtmlTableCell { Align = CENTER };
                        cell4.Controls.Add(new HtmlAnchor { HRef = $"{zipLinkPrefix}_{index}.zip", InnerText = DOWNLOAD });
                        row.Cells.Add(cell4);
                    }
                }

                table.Rows.Add(row);
                index++;
            }

            //add table to file
            string html;
            var reportMessage = new StringBuilder();

            using (var sw = new StringWriter())
            {
                table.RenderControl(new HtmlTextWriter(sw));
                html = sw.ToString();
            }

            reportMessage.AppendFormat(html);

            File.WriteAllText($@"{uftWorkingFolder}\res\Report_{buildNumber}\UFT Report", reportMessage.ToString());

        }

        public static void CreateRunStatusSummary(RunStatus runStatus, int totalTests, IDictionary<string, int> nrOfTests,
                                                  string uftWorkingFolder, string buildNumber)
        {
            var table = new HtmlTable();
            var header = new HtmlTableRow();

            var h1 = new HtmlTableCell { InnerText = RUN_STATUS, Width = _100, Align = CENTER };
            h1.Attributes.Add(STYLE, FONT_WEIGHT_BOLD);
            header.Cells.Add(h1);

            var h2 = new HtmlTableCell { InnerText = TOTAL_TESTS, Width = _150, Align = CENTER };
            h2.Attributes.Add(STYLE, FONT_WEIGHT_BOLD);
            header.Cells.Add(h2);

            var h3 = new HtmlTableCell { InnerText = _STATUS, Width = _100, Align = CENTER, ColSpan = 2 };
            h3.Attributes.Add(STYLE, FONT_WEIGHT_BOLD);
            header.Cells.Add(h3);

            var h4 = new HtmlTableCell { InnerText = NO_OF_TESTS, Width = _80, Align = CENTER };
            h4.Attributes.Add(STYLE, FONT_WEIGHT_BOLD);
            header.Cells.Add(h4);

            var h5 = new HtmlTableCell { InnerText = PASSING_RATE, Width = _150, Align = CENTER };
            h5.Attributes.Add(STYLE, FONT_WEIGHT_BOLD);
            header.Cells.Add(h5);

            header.BgColor = KnownColor.Azure.ToString();
            table.Rows.Add(header);

            string[] statuses = nrOfTests.Keys.ToArray();
            int length = statuses.Length;

            //create table content
            for (int index = 0; index < length; index++)
            {
                HtmlTableRow row = new HtmlTableRow();
                if (index == 0)
                {
                    var cell1 = new HtmlTableCell { InnerText = runStatus.ToString(), Align = CENTER, RowSpan = 4 };
                    cell1.Attributes.Add(STYLE, FONT_WEIGHT_BOLD);
                    row.Cells.Add(cell1);

                    var cell2 = new HtmlTableCell { InnerText = $"{totalTests}", Align = CENTER, RowSpan = 4 };
                    cell2.Attributes.Add(STYLE, FONT_WEIGHT_BOLD);
                    row.Cells.Add(cell2);
                }

                var cell3 = new HtmlTableCell { Align = RIGHT };
                var statusImage = new HtmlImage
                {
                    Src = $"{IMG_LINK_PREFIX}/{statuses[index].ToLower()}.svg"
                };
                cell3.Controls.Add(statusImage);
                row.Cells.Add(cell3);

                row.Cells.Add(new HtmlTableCell { Align = CENTER, InnerText = statuses[index] });
                row.Cells.Add(new HtmlTableCell { Align = CENTER, InnerText = nrOfTests[statuses[index]].ToString() });
                row.Cells.Add(new HtmlTableCell { Align = CENTER, InnerText = $"{(int)Math.Round((double)(100 * nrOfTests[statuses[index]]) / totalTests)}%" });

                row.Attributes.Add(STYLE, HEIGHT_30PX);
                table.Rows.Add(row);
            }

            //add table to file
            string html;
            var runStatusMessage = new StringBuilder();

            using (var sw = new StringWriter())
            {
                table.RenderControl(new HtmlTextWriter(sw));
                html = sw.ToString();
            }

            runStatusMessage.AppendFormat(html);

            File.WriteAllText($@"{uftWorkingFolder}\res\Report_{buildNumber}\Run status summary", runStatusMessage.ToString());
        }

        public static void CreateJUnitReport(IDictionary<string, IList<ReportMetaData>> reports, string uftWorkingFolder, string buildNumber)
        {
            var table = new HtmlTable();
            var header = new HtmlTableRow();

            var h1 = new HtmlTableCell
            {
                InnerText = TEST_NAME,
                Width = _150,
                Align = CENTER
            };
            h1.Attributes.Add(STYLE, FONT_WEIGHT_BOLD);
            header.Cells.Add(h1);

            var h2 = new HtmlTableCell
            {
                InnerText = FAILED_STEPS,
                Width = _300,
                Align = LEFT
            };
            h2.Attributes.Add(STYLE, FONT_WEIGHT_BOLD);
            header.Cells.Add(h2);

            var h3 = new HtmlTableCell
            {
                InnerText = DURATIONS,
                Width = _100,
                Align = LEFT
            };
            h3.Attributes.Add(STYLE, FONT_WEIGHT_BOLD);
            header.Cells.Add(h3);

            var h4 = new HtmlTableCell
            {
                InnerText = ERROR_DETAILS,
                Width = _600,
                Align = LEFT
            };
            h4.Attributes.Add(STYLE, FONT_WEIGHT_BOLD);
            header.Cells.Add(h4);

            header.BgColor = KnownColor.Azure.ToString();
            table.Rows.Add(header);

            var numberOfFailedSteps = GetNumberOfFailedSteps(reports);

            foreach (string testName in reports.Keys)
            {
                int index = 0;
                foreach (var item in reports[testName])
                {
                    if (item.getStatus() == FAIL)
                    {
                        var row = new HtmlTableRow();
                        if (index == 0)
                        {
                            var cell1 = new HtmlTableCell { InnerText = testName, Align = CENTER };
                            cell1.Attributes.Add(STYLE, FONT_WEIGHT_BOLD_UNDERLINE);
                            cell1.RowSpan = numberOfFailedSteps[testName];
                            row.Cells.Add(cell1);
                        }

                        row.Cells.Add(new HtmlTableCell { InnerText = item.getDisplayName(), Align = LEFT });
                        row.Cells.Add(new HtmlTableCell { InnerText = item.getDuration(), Align = LEFT });
                        row.Cells.Add(new HtmlTableCell { InnerText = item.getErrorMessage(), Align = LEFT });

                        row.Attributes.Add(STYLE, HEIGHT_30PX);
                        table.Rows.Add(row);

                        index++;
                    }
                }
            }

            //add table to file
            string html;
            var junitStatusMessage = new StringBuilder();

            using (var sw = new StringWriter())
            {
                table.RenderControl(new HtmlTextWriter(sw));
                html = sw.ToString();
            }

            junitStatusMessage.AppendFormat(html);

            File.WriteAllText($@"{uftWorkingFolder}\res\Report_{buildNumber}\Failed tests", junitStatusMessage.ToString());
        }

        private static IDictionary<string, int> GetNumberOfFailedSteps(IDictionary<string, IList<ReportMetaData>> reports)
        {
            IDictionary<string, int> numberOfFailedSteps = new Dictionary<string, int>();
            foreach (var test in reports.Keys)
            {
                int failedSteps = 0;
                foreach (var item in reports[test])
                {
                    if (item.getStatus() == FAIL)
                    {
                        failedSteps++;
                    }
                }
                numberOfFailedSteps[test] = failedSteps;
            }
            return numberOfFailedSteps;
        }

        private static string GetTestName(string testPath)
        {
            int pos = testPath.LastIndexOf(@"\", StringComparison.Ordinal) + 1;
            return testPath.Substring(pos, testPath.Length - pos);
        }

        private static string ImageToBase64(System.Drawing.Image _imagePath)
        {
            byte[] imageBytes = ImageToByteArray(_imagePath);
            string base64String = Convert.ToBase64String(imageBytes);
            return base64String;
        }

        private static byte[] ImageToByteArray(System.Drawing.Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

            return ms.ToArray();
        }
    }
}
