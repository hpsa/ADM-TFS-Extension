using PSModule.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Xml;

namespace PSModule
{
    static class Helper
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
        private const string PASSING_RATE = "Percentage of tests";
        private const string STYLE = "style";
        private const string UFT_REPORT_COL_CAPTION = "UFT report";
        private const string UFT_REPORT_ARCHIVE = "UFT report archive";
        private const string VIEW_REPORT = "View report";
        private const string DOWNLOAD = "Download";

        private const string LEFT = "left";
        private const string _200 = "200";
        private const string _800 = "800";
        private const string HEIGHT_30PX_AZURE = "height:30px;background-color:azure";
        private const string HEIGHT_30PX = "height:30px;";
        private const string HDR_FONT_WEIGHT_BOLD_MIN_WIDTH_200 = "font-weight:bold;min-width:200px";
        private const string HDR_FONT_WEIGHT_BOLD_MIN_WIDTH_800 = "font-weight:bold;min-width:800px";
        private const string FONT_WEIGHT_BOLD = "font-weight:bold;";
        private const string FONT_WEIGHT_BOLD_UNDERLINE = "font-weight:bold; text-decoration:underline;";

        private const string UFT_REPORT_CAPTION = "UFT Report";
        private const string RUN_SUMMARY = "Run Summary";
        private const string FAILED_TESTS = "Failed Tests";
        #endregion

        public static IList<ReportMetaData> ReadReportFromXMLFile(string reportPath, bool isJUnitReport, out IDictionary<string, IList<ReportMetaData>> failedSteps)
        {
            failedSteps = new Dictionary<string, IList<ReportMetaData>>();
            var listReport = new List<ReportMetaData>();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(reportPath);

            foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes) //inside <testsuite> node 
            {
                var failedTestSteps = new List<ReportMetaData>();
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
                            case NAME  : reportmetadata.DisplayName = attribute.Value; break;
                            case REPORT: reportmetadata.ReportPath  = attribute.Value; break;
                            case STATUS: reportmetadata.Status      = attribute.Value; break;
                            case TIME  : reportmetadata.Duration    = attribute.Value; break;
                            default    : break;
                        }
                    }

                    if (isJUnitReport)
                    {
                        //remove the number in front of each step
                        string stepName = reportmetadata.DisplayName;
                        if (stepName?.StartsWith(DIEZ) == true)
                        {
                            reportmetadata.DisplayName = stepName.Substring(stepName.IndexOf(COLON) + 1);
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
                                    reportmetadata.ErrorMessage = attribute.Value;
                                    reportmetadata.Status = FAIL;
                                }
                            }
                        }
                        if (xmlNode.Name == SYSTEM_OUT)
                        {
                            reportmetadata.DateTime = xmlNode.InnerText.Substring(0, 19);
                        }
                    }
                    if (isJUnitReport && reportmetadata.Status == FAIL)
                    {
                        failedTestSteps.Add(reportmetadata);
                    }
                    listReport.Add(reportmetadata);
                }
                if (isJUnitReport && failedTestSteps.Any())
                {
                    failedSteps.Add(testName, failedTestSteps);
                }
            }

            return listReport;
        }

        public static RunStatus GetRunStatus(IList<ReportMetaData> listReport)
        {
            var errorCode = RunStatus.PASSED;
            int passedTests = 0, failedTests = 0;

            foreach (ReportMetaData report in listReport)
            {
                if (report.Status == PASS)
                {
                    passedTests++;
                }
                else if (report.Status.In(ERROR, FAIL))
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
                nrOfTests[item.Status]++;
            }

            return listReport.Count;
        }

        public static void CreateSummaryReport(string rptPath, RunType runType, IList<ReportMetaData> reportList,
                                               bool uploadArtifact = false, ArtifactType artifactType = ArtifactType.None,
                                               string storageAccount = "", string container = "", string reportName = "", string archiveName = "")
        {
            var table = new HtmlTable { ClientIDMode = ClientIDMode.Static, ID = "tblSummaryReportId" };
            var header = new HtmlTableRow();
            var h1 = new HtmlTableCell { InnerText = TEST_NAME, Width = _200, Align = LEFT };
            h1.Attributes.Add(STYLE, HDR_FONT_WEIGHT_BOLD_MIN_WIDTH_200);
            header.Cells.Add(h1);

            var h2 = new HtmlTableCell { InnerText = TIMESTAMP, Width = _200, Align = LEFT };
            h2.Attributes.Add(STYLE, HDR_FONT_WEIGHT_BOLD_MIN_WIDTH_200);
            header.Cells.Add(h2);

            var h3 = new HtmlTableCell { InnerText = _STATUS, Width = _200, Align = LEFT };
            h3.Attributes.Add(STYLE, HDR_FONT_WEIGHT_BOLD_MIN_WIDTH_200);
            header.Cells.Add(h3);

            if (runType == RunType.FileSystem && uploadArtifact)
            {
                if (artifactType.In(ArtifactType.onlyReport, ArtifactType.bothReportArchive))
                {
                    var h4 = new HtmlTableCell { InnerText = UFT_REPORT_COL_CAPTION, Width = _200, Align = LEFT };
                    h4.Attributes.Add(STYLE, HDR_FONT_WEIGHT_BOLD_MIN_WIDTH_200);
                    header.Cells.Add(h4);

                    if (artifactType == ArtifactType.bothReportArchive)
                    {
                        var h5 = new HtmlTableCell { InnerText = UFT_REPORT_ARCHIVE, Width = _200, Align = LEFT };
                        h5.Attributes.Add(STYLE, HDR_FONT_WEIGHT_BOLD_MIN_WIDTH_200);
                        header.Cells.Add(h5);
                    }
                }
                else if (artifactType == ArtifactType.onlyArchive)
                {
                    var h4 = new HtmlTableCell { InnerText = UFT_REPORT_ARCHIVE, Width = _200, Align = LEFT };
                    h4.Attributes.Add(STYLE, HDR_FONT_WEIGHT_BOLD_MIN_WIDTH_200);
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
                var cell1 = new HtmlTableCell { InnerText = GetTestName(report.DisplayName), Align = LEFT };
                row.Cells.Add(cell1);

                var cell2 = new HtmlTableCell { InnerText = report.DateTime, Align = LEFT };
                row.Cells.Add(cell2);

                var cell3 = new HtmlTableCell { Align = LEFT };
                cell3.Controls.Add(new HtmlImage { Src = $"{IMG_LINK_PREFIX}/{report.Status}.svg" });
                row.Cells.Add(cell3);

                if (runType == RunType.FileSystem && uploadArtifact && !report.ReportPath.IsNullOrWhiteSpace())
                {
                    string htmlLink = $"{htmlLinkPrefix}_{index}.html";
                    string zipLink = $"{zipLinkPrefix}_{index}.zip";
                    if (artifactType.In(ArtifactType.onlyReport, ArtifactType.bothReportArchive))
                    {
                        var cell4 = new HtmlTableCell { Align = LEFT };
                        var reportLink = new HtmlAnchor { HRef = htmlLink, InnerText = VIEW_REPORT };
                        cell4.Controls.Add(reportLink);
                        row.Cells.Add(cell4);

                        if (artifactType == ArtifactType.bothReportArchive)
                        {
                            var cell5 = new HtmlTableCell { Align = LEFT };
                            cell5.Controls.Add(new HtmlAnchor { HRef = zipLink, InnerText = DOWNLOAD });
                            row.Cells.Add(cell5);
                        }
                    }
                    else if (artifactType == ArtifactType.onlyArchive)
                    {
                        var cell4 = new HtmlTableCell { Align = LEFT };
                        cell4.Controls.Add(new HtmlAnchor { HRef = zipLink, InnerText = DOWNLOAD });
                        row.Cells.Add(cell4);
                    }
                    index++;
                }
                table.Rows.Add(row);
            }

            //add table to file
            string html;
            using (var sw = new StringWriter())
            {
                table.RenderControl(new HtmlTextWriter(sw));
                html = sw.ToString();
            }
            File.WriteAllText(Path.Combine(rptPath, UFT_REPORT_CAPTION), html);
        }

        public static void CreateRunSummary(RunStatus runStatus, int totalTests, IDictionary<string, int> nrOfTests, string rptPath)
        {
            var table = new HtmlTable { ClientIDMode = ClientIDMode.Static, ID = "tblRunSummaryId"};
            var header = new HtmlTableRow();

            var h1 = new HtmlTableCell { InnerText = RUN_STATUS, Width = _200, Align = LEFT };
            h1.Attributes.Add(STYLE, HDR_FONT_WEIGHT_BOLD_MIN_WIDTH_200);
            header.Cells.Add(h1);

            var h2 = new HtmlTableCell { InnerText = TOTAL_TESTS, Width = _200, Align = LEFT };
            h2.Attributes.Add(STYLE, HDR_FONT_WEIGHT_BOLD_MIN_WIDTH_200);
            header.Cells.Add(h2);

            var h3 = new HtmlTableCell { InnerText = _STATUS, Width = _200, Align = LEFT, ColSpan = 2 };
            h3.Attributes.Add(STYLE, HDR_FONT_WEIGHT_BOLD_MIN_WIDTH_200);
            header.Cells.Add(h3);

            var h4 = new HtmlTableCell { InnerText = NO_OF_TESTS, Width = _200, Align = LEFT };
            h4.Attributes.Add(STYLE, HDR_FONT_WEIGHT_BOLD_MIN_WIDTH_200);
            header.Cells.Add(h4);

            var h5 = new HtmlTableCell { InnerText = PASSING_RATE, Width = _200, Align = LEFT };
            h5.Attributes.Add(STYLE, HDR_FONT_WEIGHT_BOLD_MIN_WIDTH_200);
            header.Cells.Add(h5);

            header.BgColor = KnownColor.Azure.ToString();
            table.Rows.Add(header);

            string[] statuses = nrOfTests.Keys.ToArray();
            int length = statuses.Length;

            var percentages = new decimal[length];
            for (int index = 0; index < length; index++)
            {
                percentages[index] = (decimal)(100 * nrOfTests[statuses[index]]) / totalTests;
            }
            var roundedPercentages = GetPerfectRounding(percentages);
            //create table content
            for (int index = 0; index < length; index++)
            {
                var row = new HtmlTableRow();
                if (index == 0)
                {
                    var cell1 = new HtmlTableCell { InnerText = runStatus.ToString(), Align = LEFT, RowSpan = 4 };
                    cell1.Attributes.Add(STYLE, FONT_WEIGHT_BOLD);
                    row.Cells.Add(cell1);

                    var cell2 = new HtmlTableCell { InnerText = $"{totalTests}", Align = LEFT, RowSpan = 4 };
                    cell2.Attributes.Add(STYLE, FONT_WEIGHT_BOLD);
                    row.Cells.Add(cell2);
                }

                var cell3 = new HtmlTableCell { Align = LEFT };
                var statusImage = new HtmlImage
                {
                    Src = $"{IMG_LINK_PREFIX}/{statuses[index].ToLower()}.svg"
                };
                cell3.Controls.Add(statusImage);
                row.Cells.Add(cell3);

                row.Cells.Add(new HtmlTableCell { Align = LEFT, InnerText = statuses[index] });
                row.Cells.Add(new HtmlTableCell { Align = LEFT, InnerText = nrOfTests[statuses[index]].ToString() });
                row.Cells.Add(new HtmlTableCell { Align = LEFT, InnerText = $"{roundedPercentages[index]:00.00}%" });

                row.Attributes.Add(STYLE, HEIGHT_30PX);
                table.Rows.Add(row);
            }

            //add table to file
            string html;
            using (var sw = new StringWriter())
            {
                table.RenderControl(new HtmlTextWriter(sw));
                html = sw.ToString();
            }
            File.WriteAllText(Path.Combine(rptPath,RUN_SUMMARY), html);
        }

        public static void CreateFailedStepsReport(IDictionary<string, IList<ReportMetaData>> failedSteps, string rptPath)
        {
            if (failedSteps.IsNullOrEmpty())
                return;

            var table = new HtmlTable { ClientIDMode = ClientIDMode.Static, ID = "tblFailedStepsId" };
            var header = new HtmlTableRow();

            var h1 = new HtmlTableCell { InnerText = TEST_NAME, Width = _200, Align = LEFT };
            h1.Attributes.Add(STYLE, HDR_FONT_WEIGHT_BOLD_MIN_WIDTH_200);
            header.Cells.Add(h1);

            var h2 = new HtmlTableCell { InnerText = FAILED_STEPS, Width = _200, Align = LEFT };
            h2.Attributes.Add(STYLE, HDR_FONT_WEIGHT_BOLD_MIN_WIDTH_200);
            header.Cells.Add(h2);

            var h3 = new HtmlTableCell { InnerText = DURATIONS, Width = _200, Align = LEFT };
            h3.Attributes.Add(STYLE, HDR_FONT_WEIGHT_BOLD_MIN_WIDTH_200);
            header.Cells.Add(h3);

            var h4 = new HtmlTableCell { InnerText = ERROR_DETAILS, Width = _800, Align = LEFT };
            h4.Attributes.Add(STYLE, HDR_FONT_WEIGHT_BOLD_MIN_WIDTH_800);
            header.Cells.Add(h4);

            header.Attributes.Add(STYLE, HEIGHT_30PX);
            table.Rows.Add(header);

            bool isOddRow = true;
            foreach (string testName in failedSteps.Keys)
            {
                int index = 0;
                string style = isOddRow ? HEIGHT_30PX_AZURE : HEIGHT_30PX;
                var failedTestSteps = failedSteps[testName];
                foreach (var item in failedTestSteps)
                {
                    var row = new HtmlTableRow();
                    if (index == 0)
                    {
                        var cell1 = new HtmlTableCell { InnerText = testName, Align = LEFT };
                        cell1.Attributes.Add(STYLE, FONT_WEIGHT_BOLD_UNDERLINE);
                        cell1.RowSpan = failedTestSteps.Count;
                        row.Cells.Add(cell1);
                    }

                    row.Cells.Add(new HtmlTableCell { InnerText = item.DisplayName, Align = LEFT });
                    row.Cells.Add(new HtmlTableCell { InnerText = item.Duration, Align = LEFT });
                    row.Cells.Add(new HtmlTableCell { InnerText = item.ErrorMessage, Align = LEFT });

                    row.Attributes.Add(STYLE, style);
                    table.Rows.Add(row);

                    index++;
                }
                if (failedTestSteps.Any())
                    isOddRow = !isOddRow;
            }

            //add table to file
            string html;
            using (var sw = new StringWriter())
            {
                table.RenderControl(new HtmlTextWriter(sw));
                html = sw.ToString();
            }
            File.WriteAllText(Path.Combine(rptPath, FAILED_TESTS), html);
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

        private static decimal[] GetPerfectRounding(decimal[] original, decimal expectedSum = 100, int decimals = 1)
        {
            var rounded = original.Select(x => Math.Round(x, decimals)).ToArray();
            var delta = expectedSum - rounded.Sum();
            if (delta == 0) return rounded;
            var deltaUnit = Convert.ToDecimal(Math.Pow(0.1, decimals)) * Math.Sign(delta);

            IList<int> applyDeltaSequence;
            if (delta < 0)
            {
                applyDeltaSequence = original
                    .Zip(Enumerable.Range(0, int.MaxValue), (x, index) => new { x, index })
                    .OrderBy(a => original[a.index] - rounded[a.index])
                    .ThenByDescending(a => a.index)
                    .Select(a => a.index).ToList();
            }
            else
            {
                applyDeltaSequence = original
                    .Zip(Enumerable.Range(0, int.MaxValue), (x, index) => new { x, index })
                    .OrderByDescending(a => original[a.index] - rounded[a.index])
                    .Select(a => a.index).ToList();
            }

            Enumerable.Repeat(applyDeltaSequence, int.MaxValue)
                .SelectMany(x => x)
                .Take(Convert.ToInt32(delta / deltaUnit))
                .ForEach(index => rounded[index] += deltaUnit);

            return rounded;
        }
    }
}
