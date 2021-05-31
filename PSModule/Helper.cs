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
                    string currentTest = node.Attributes["name"].Value;
                    testName = currentTest.Substring(currentTest.LastIndexOf('-') + 1);
                }

                foreach (XmlNode currentNode in node) //inside <testcase> nodes
                {
                    var reportmetadata = new ReportMetaData();
                    var attributesList = currentNode.Attributes;
                    foreach (XmlAttribute attribute in attributesList)
                    {
                        switch (attribute.Name)
                        {
                            case "name"  : reportmetadata.setDisplayName(attribute.Value); break;
                            case "report": reportmetadata.setReportPath(attribute.Value); break;
                            case "status": reportmetadata.setStatus(attribute.Value); break;
                            case "time"  : reportmetadata.setDuration(attribute.Value); break;
                            default      : break;
                        }
                    }

                    if (isJUnitReport)
                    {
                        //remove the number in front of each step
                        string stepName = reportmetadata.getDisplayName();
                        if (stepName?.StartsWith("#") == true)
                        {
                            reportmetadata.setDisplayName(stepName.Substring(stepName.IndexOf(':') + 1));
                        }
                    }

                    var nodes = currentNode.ChildNodes;
                    foreach (XmlNode xmlNode in nodes)//inside nodes in <testcase> nodes
                    {
                        if (xmlNode.Name.Equals("failure"))
                        {
                            foreach (XmlAttribute attribute in xmlNode.Attributes)
                            {
                                if (attribute.Name.Equals("message"))
                                {
                                    reportmetadata.setErrorMessage(attribute.Value);
                                    reportmetadata.setStatus("fail");
                                }
                            }
                        }
                        if (xmlNode.Name.Equals("system-out"))
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
                if (report.getStatus().EqualsIgnoreCase("pass"))
                {
                    passedTests++;
                }
                else if (report.getStatus().EqualsIgnoreCase("fail"))
                {
                    failedTests++;
                }
                else if (report.getStatus().EqualsIgnoreCase("error"))
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
            int totalTests = 0;
            nrOfTests = new Dictionary<string, int>
            {
                { "Passed", 0 },
                { "Failed", 0 },
                { "Error", 0 },
                { "Warning", 0 }
            };

            foreach (ReportMetaData item in listReport)
            {
                _ = item.getStatus() switch
                {
                    "pass"  => nrOfTests["Passed"]++,
                    "fail"  => nrOfTests["Failed"]++,
                    "error" => nrOfTests["Error"]++,
                    _       => nrOfTests["Warning"]++
                };

                totalTests++;
            }

            return totalTests;
        }

        public static void CreateSummaryReport(string uftWorkingFolder, string buildNumber, RunType runType, ref IList<ReportMetaData> reportList,
                                               bool uploadArtifact = false, ArtifactType artifactType = ArtifactType.None,
                                               string storageAccount = "", string container = "", string reportName = "", string archiveName = "")
        {
            var table = new HtmlTable();
            var header = new HtmlTableRow();
            var h1 = new HtmlTableCell { InnerText = "Test name", Width = "100", Align = "center" };
            h1.Attributes.Add("style", "font-weight: bold;");
            header.Cells.Add(h1);

            var h2 = new HtmlTableCell { InnerText = "Timestamp", Width = "150", Align = "center" };
            h2.Attributes.Add("style", "font-weight: bold;");
            header.Cells.Add(h2);

            var h3 = new HtmlTableCell { InnerText = "Status", Width = "50", Align = "center" };
            h3.Attributes.Add("style", "font-weight: bold;");
            header.Cells.Add(h3);

            if (runType == RunType.FileSystem && uploadArtifact)
            {
                if (artifactType.In(ArtifactType.onlyReport, ArtifactType.bothReportArchive))
                {
                    var h4 = new HtmlTableCell { InnerText = "UFT report", Width = "100", Align = "center" };
                    h4.Attributes.Add("style", "font-weight: bold;");
                    header.Cells.Add(h4);

                    if (artifactType == ArtifactType.bothReportArchive)
                    {
                        var h5 = new HtmlTableCell { InnerText = "UFT report archive", Width = "150", Align = "center" };
                        h5.Attributes.Add("style", "font-weight: bold;");
                        header.Cells.Add(h5);
                    }
                }
                else if (artifactType == ArtifactType.onlyArchive)
                {
                    var h4 = new HtmlTableCell { InnerText = "UFT report archive", Width = "150", Align = "center" };
                    h4.Attributes.Add("style", "font-weight: bold;");
                    header.Cells.Add(h4);
                }
            }

            header.BgColor = KnownColor.Azure.ToString();
            table.Rows.Add(header);

            //create table content
            int index = 1;
            var zipLinkPrefix = $"https://{storageAccount}.blob.core.windows.net/{container}/{archiveName}";
            var htmlLinkPrefix = $"https://{storageAccount}.blob.core.windows.net/{container}/{reportName}";
            foreach (ReportMetaData report in reportList)
            {
                var row = new HtmlTableRow();
                var cell1 = new HtmlTableCell { InnerText = GetTestName(report.getDisplayName()), Align = "center" };
                row.Cells.Add(cell1);

                var cell2 = new HtmlTableCell { InnerText = report.getDateTime(), Align = "center" };
                row.Cells.Add(cell2);

                var cell3 = new HtmlTableCell { Align = "center" };
                var imgName = report.getStatus() switch
                {
                    "pass"  => "passed.png",
                    "error" => "error.png",
                    _       => "failed.png"
                };

                cell3.Controls.Add(new HtmlImage { Src = $"https://extensionado.blob.core.windows.net/uft-extension-images/{imgName}" });
                row.Cells.Add(cell3);

                if (runType == RunType.FileSystem && uploadArtifact && !report.getReportPath().IsNullOrWhiteSpace())
                {
                    if (artifactType.In(ArtifactType.onlyReport, ArtifactType.bothReportArchive))
                    {
                        var cell4 = new HtmlTableCell { Align = "center" };
                        var reportLink = new HtmlAnchor { HRef = $"{htmlLinkPrefix}/{reportName}_{index}.html", InnerText = "View report" };
                        cell4.Controls.Add(reportLink);
                        row.Cells.Add(cell4);

                        if (artifactType == ArtifactType.bothReportArchive)
                        {
                            var cell5 = new HtmlTableCell { Align = "center" };
                            cell5.Controls.Add(new HtmlAnchor { HRef = $"{zipLinkPrefix}_{index}.zip", InnerText = "Download" });
                            row.Cells.Add(cell5);
                        }
                    }
                    else if (artifactType == ArtifactType.onlyArchive)
                    {
                        var cell4 = new HtmlTableCell { Align = "center" };
                        cell4.Controls.Add(new HtmlAnchor { HRef = $"{zipLinkPrefix}_{index}.zip", InnerText = "Download" });
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

            var h1 = new HtmlTableCell { InnerText = "Run status", Width = "100", Align = "center" };
            h1.Attributes.Add("style", "font-weight: bold;");
            header.Cells.Add(h1);

            var h2 = new HtmlTableCell { InnerText = "Total tests", Width = "150", Align = "center" };
            h2.Attributes.Add("style", "font-weight: bold;");
            header.Cells.Add(h2);

            var h3 = new HtmlTableCell { InnerText = "Status", Width = "100", Align = "center", ColSpan = 2 };
            h3.Attributes.Add("style", "font-weight: bold;");
            header.Cells.Add(h3);

            var h4 = new HtmlTableCell { InnerText = "No. of tests", Width = "80", Align = "center" };
            h4.Attributes.Add("style", "font-weight: bold;");
            header.Cells.Add(h4);

            var h5 = new HtmlTableCell { InnerText = "Passing rate", Width = "150", Align = "center" };
            h5.Attributes.Add("style", "font-weight: bold;");
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
                    var cell1 = new HtmlTableCell { InnerText = runStatus.ToString(), Align = "center", RowSpan = 4 };
                    cell1.Attributes.Add("style", "font-weight: bold;");
                    row.Cells.Add(cell1);

                    var cell2 = new HtmlTableCell { InnerText = $"{totalTests}", Align = "center", RowSpan = 4 };
                    cell2.Attributes.Add("style", "font-weight: bold;");
                    row.Cells.Add(cell2);
                }

                var cell3 = new HtmlTableCell { Align = "right" };
                var statusImage = new HtmlImage
                {
                    Src = $"https://extensionado.blob.core.windows.net/uft-extension-images/{statuses[index].ToLower()}.png"
                };
                cell3.Controls.Add(statusImage);
                row.Cells.Add(cell3);

                row.Cells.Add(new HtmlTableCell { Align = "center", InnerText = statuses[index] });
                row.Cells.Add(new HtmlTableCell { Align = "center", InnerText = nrOfTests[statuses[index]].ToString() });
                row.Cells.Add(new HtmlTableCell { Align = "center", InnerText = $"{(int)Math.Round((double)(100 * nrOfTests[statuses[index]]) / totalTests)}%" });

                row.Attributes.Add("style", "height: 30px;");
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
                InnerText = "Test name",
                Width = "150",
                Align = "center"
            };
            h1.Attributes.Add("style", "font-weight: bold;");
            header.Cells.Add(h1);

            var h2 = new HtmlTableCell
            {
                InnerText = "Failed steps",
                Width = "300",
                Align = "left"
            };
            h2.Attributes.Add("style", "font-weight: bold;");
            header.Cells.Add(h2);

            var h3 = new HtmlTableCell
            {
                InnerText = "Duration(s)",
                Width = "100",
                Align = "left"
            };
            h3.Attributes.Add("style", "font-weight: bold;");
            header.Cells.Add(h3);

            var h4 = new HtmlTableCell
            {
                InnerText = "Error details",
                Width = "600",
                Align = "left"
            };
            h4.Attributes.Add("style", "font-weight: bold;");
            header.Cells.Add(h4);

            header.BgColor = KnownColor.Azure.ToString();
            table.Rows.Add(header);

            var numberOfFailedSteps = GetNumberOfFailedSteps(reports);

            foreach (string testName in reports.Keys)
            {
                int index = 0;
                foreach (var item in reports[testName])
                {
                    if (item.getStatus().EqualsIgnoreCase("fail"))
                    {
                        var row = new HtmlTableRow();
                        if (index == 0)
                        {
                            var cell1 = new HtmlTableCell { InnerText = testName, Align = "center" };
                            cell1.Attributes.Add("style", "font-weight: bold; text-decoration: underline;");
                            cell1.RowSpan = numberOfFailedSteps[testName];
                            row.Cells.Add(cell1);
                        }

                        row.Cells.Add(new HtmlTableCell { InnerText = item.getDisplayName(), Align = "left" });
                        row.Cells.Add(new HtmlTableCell { InnerText = item.getDuration(), Align = "left" });
                        row.Cells.Add(new HtmlTableCell { InnerText = item.getErrorMessage(), Align = "left" });

                        row.Attributes.Add("style", "height: 30px;");
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
                    if (item.getStatus().EqualsIgnoreCase("fail"))
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
