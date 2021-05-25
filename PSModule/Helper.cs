using PSModule.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Web.UI.HtmlControls;
using System.Xml;
using System.Web.UI.DataVisualization.Charting;
using Azure.Storage.Blobs;
using Azure.Storage;

namespace PSModule
{
    class Helper
    {
        public static List<ReportMetaData> readReportFromXMLFile(string reportPath, Dictionary<string, List<ReportMetaData>> testSteps, bool isJUnitReport)
        {
            List<ReportMetaData> listReport = new List<ReportMetaData>();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(reportPath);
          
            foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes) //inside <testsuite> node 
            {
                List<ReportMetaData> steps = new List<ReportMetaData>();
                string testName = "";
                if (isJUnitReport)
                {
                   
                    string currentTest = node.Attributes["name"].Value;
                    testName = currentTest.Substring(currentTest.LastIndexOf("-") + 1);
                }

                foreach (XmlNode currentNode in node) //inside <testcase> nodes
                {
                    ReportMetaData reportmetadata = new ReportMetaData();
                    XmlAttributeCollection attributesList = currentNode.Attributes;
                    foreach (XmlAttribute attribute in attributesList)
                    {
                        switch (attribute.Name)
                        {
                            case "name": reportmetadata.setDisplayName(attribute.Value); break;
                            case "report": reportmetadata.setReportPath(attribute.Value); break;
                            case "status": reportmetadata.setStatus(attribute.Value); break;
                            case "time": reportmetadata.setDuration(attribute.Value); break;
                            default: break;
                        }
                    }

                    if (isJUnitReport)
                    {
                        //remove the number in front of each step
                        string stepName = reportmetadata.getDisplayName();
                        if (!String.IsNullOrEmpty(stepName))
                        {
                            if (stepName.StartsWith("#"))
                            {
                                reportmetadata.setDisplayName(stepName.Substring(stepName.IndexOf(":") + 1));
                            }
                        }
                    }

                    XmlNodeList nodes = currentNode.ChildNodes;
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

        public static int getErrorCode(List<ReportMetaData> listReport)
        {
            int errorCode = 0;
            int passedTests = 0;
            int failedTests = 0;

            foreach (ReportMetaData report in listReport)
            {
                if (report.getStatus().Equals("pass"))
                {
                    passedTests++;
                }
                if (report.getStatus().Equals("fail"))
                {
                    failedTests++;
                }
                if (report.getStatus().Equals("error"))
                {
                    failedTests++;
                }
            }

            if (passedTests > 0 && failedTests > 0)
            {
                errorCode = -2;//job unstable
            }
            if (passedTests == 0 && failedTests > 0)
            {
                errorCode = -1;//job failed
            }

            return errorCode;
        }

        public static int getNumberOfTests(List<ReportMetaData> listReport, ref Dictionary<string, int> nrOfTests)
        {
            int totalTests = 0;
            foreach (ReportMetaData item in listReport)
            {
                switch (item.getStatus())
                {
                    case "pass": nrOfTests["Passed"] = (int)nrOfTests["Passed"] + 1; break;
                    case "fail": nrOfTests["Failed"] = (int)nrOfTests["Failed"] + 1; break;
                    case "error": nrOfTests["Error"] = (int)nrOfTests["Error"] + 1; break;
                    default: nrOfTests["Warning"] = (int)nrOfTests["Warning"] + 1; break;
                }
            
                totalTests++;
            }
            
            return totalTests;
        }

        public static void createSummaryReport(string uftWorkingFolder, ref List<ReportMetaData> reportList,
                                               string uploadArtifact, string artifactType,
                                               string storageAccount, string container, 
                                               string reportName, string archiveName, string buildNumber, string runType)
        {
            HtmlTable table = new HtmlTable(); 
            HtmlTableRow header = new HtmlTableRow();
            HtmlTableCell h1 = new HtmlTableCell();
            h1.InnerText = "Test name";
            h1.Width = "100";
            h1.Align = "center";
            h1.Attributes.Add("style", "font-weight: bold;");
            header.Cells.Add(h1);

            HtmlTableCell h2 = new HtmlTableCell();
            h2.InnerText = "Timestamp";
            h2.Width = "150";
            h2.Align = "center";
            h2.Attributes.Add("style", "font-weight: bold;");
            header.Cells.Add(h2);

            HtmlTableCell h3 = new HtmlTableCell();
            h3.InnerText = "Status";
            h3.Width = "50";
            h3.Align = "center";
            h3.Attributes.Add("style", "font-weight: bold;");
            header.Cells.Add(h3);

            if (runType.Equals(RunType.FileSystem.ToString()))
            {
                if (uploadArtifact.Equals("yes") && (artifactType.Equals(ArtifactType.onlyReport.ToString()) || artifactType.Equals(ArtifactType.bothReportArchive.ToString())))
                {
                    HtmlTableCell h4 = new HtmlTableCell();
                    h4.InnerText = "UFT report";
                    h4.Width = "100";
                    h4.Align = "center";
                    h4.Attributes.Add("style", "font-weight: bold;");
                    header.Cells.Add(h4);

                    if (artifactType.Equals(ArtifactType.bothReportArchive.ToString()))
                    {
                        HtmlTableCell h5 = new HtmlTableCell();
                        h5.InnerText = "UFT report archive";
                        h5.Width = "150";
                        h5.Align = "center";
                        h5.Attributes.Add("style", "font-weight: bold;");
                        header.Cells.Add(h5);
                    }
                }

                if (uploadArtifact.Equals("yes") && (artifactType.Equals(ArtifactType.onlyArchive.ToString())))
                {
                    HtmlTableCell h4 = new HtmlTableCell();
                    h4.InnerText = "UFT report archive";
                    h4.Width = "150";
                    h4.Align = "center";
                    header.Cells.Add(h4);
                }
            }

            header.BgColor = KnownColor.Azure.ToString();
            table.Rows.Add(header);

            //create table content
            int index = 1;
            foreach (ReportMetaData report in reportList)
            {
                HtmlTableRow row = new HtmlTableRow();

                HtmlTableCell cell1 = new HtmlTableCell();
                cell1.InnerText = getTestName(report.getDisplayName());
                cell1.Align = "center";
                row.Cells.Add(cell1);

                HtmlTableCell cell2 = new HtmlTableCell();
                cell2.InnerText = report.getDateTime();
                cell2.Align = "center";
                row.Cells.Add(cell2);

                HtmlTableCell cell3 = new HtmlTableCell();
                HtmlImage statusImage = new HtmlImage();

                if (report.getStatus().Equals("pass"))
                {
                    statusImage.Src = "https://extensionado.blob.core.windows.net/uft-extension-images/passed.png";
                }
                else
                {
                    statusImage.Src = "https://extensionado.blob.core.windows.net/uft-extension-images/failed.png";
                }

                cell3.Align = "center";
                cell3.Controls.Add(statusImage);
                row.Cells.Add(cell3);

                if (runType.Equals(RunType.FileSystem.ToString()))
                {
                    if (uploadArtifact.Equals("yes") && (artifactType.Equals(ArtifactType.onlyReport.ToString()) || artifactType.Equals(ArtifactType.bothReportArchive.ToString())))
                    {

                        HtmlTableCell cell4 = new HtmlTableCell();
                        HtmlAnchor reportLink = new HtmlAnchor();

                        reportLink.HRef = "https://" + storageAccount + ".blob.core.windows.net/" + container + "/" + reportName + "_" + index + ".html";
                        reportLink.InnerText = "View report";
                        cell4.Controls.Add(reportLink);
                        cell4.Align = "center";
                        row.Cells.Add(cell4);


                        if (artifactType.Equals(ArtifactType.bothReportArchive.ToString()))
                        {
                            HtmlTableCell cell5 = new HtmlTableCell();
                            HtmlAnchor archiveLink = new HtmlAnchor();

                            archiveLink.HRef = "https://" + storageAccount + ".blob.core.windows.net/" + container + "/" + archiveName + "_" + index + ".zip";
                            archiveLink.InnerText = "Download";
                            cell5.Controls.Add(archiveLink);
                            cell5.Align = "center";
                            row.Cells.Add(cell5);
                        }
                    }

                    if (uploadArtifact.Equals("yes") && artifactType.Equals(ArtifactType.onlyArchive.ToString()))
                    {
                        HtmlTableCell cell4 = new HtmlTableCell();
                        HtmlAnchor archiveLink = new HtmlAnchor();

                        archiveLink.HRef = "https://" + storageAccount + ".blob.core.windows.net/" + container + "/" + archiveName + "_" + index + ".zip";
                        archiveLink.InnerText = "Download";

                        cell4.Controls.Add(archiveLink);
                        cell4.Align = "center";
                        row.Cells.Add(cell4);
                    }
                }

                table.Rows.Add(row);
                index++;
            }

            //add table to file
            string html;
            var reportMessage = new System.Text.StringBuilder();

            using (var sw = new StringWriter())
            {
                table.RenderControl(new System.Web.UI.HtmlTextWriter(sw));
                html = sw.ToString();
            }

            reportMessage.AppendFormat(html);

            System.IO.File.WriteAllText(uftWorkingFolder + @"\\res\\Report_" + buildNumber + "\\UFT Report", reportMessage.ToString());
            
        }

        public static void createRunStatusSummary(string runStatus, int totalTests, Dictionary<string, int> nrOfTests, 
                                                  string uftWorkingFolder, string buildNumber,
                                                  string storageAccount, string container)
        {
            HtmlTable table = new HtmlTable();
            HtmlTableRow header = new HtmlTableRow();
          
            HtmlTableCell h1 = new HtmlTableCell();
            h1.InnerText = "Run status";
            h1.Width = "100";
            h1.Align = "center";
            h1.Attributes.Add("style", "font-weight: bold;");
            header.Cells.Add(h1);

            HtmlTableCell h2 = new HtmlTableCell();
            h2.InnerText = "Total tests";
            h2.Width = "150";
            h2.Align = "center";
            h2.Attributes.Add("style", "font-weight: bold;");
            header.Cells.Add(h2);

            HtmlTableCell h4 = new HtmlTableCell();
            h4.InnerText = "Status";
            h4.Width = "100";
            h4.Align = "center";
            h4.Attributes.Add("style", "font-weight: bold;");
            h4.ColSpan = 2;
            header.Cells.Add(h4);

            HtmlTableCell h5 = new HtmlTableCell();
            h5.InnerText = "No. of tests";
            h5.Width = "80";
            h5.Align = "center";
            h5.Attributes.Add("style", "font-weight: bold;");
            header.Cells.Add(h5);

            HtmlTableCell h6 = new HtmlTableCell();
            h6.InnerText = "Passing rate";
            h6.Width = "150";
            h6.Align = "center";
            h6.Attributes.Add("style", "font-weight: bold;");
            header.Cells.Add(h6);

            header.BgColor = KnownColor.Azure.ToString();
            table.Rows.Add(header);

            string[] statusArray = { "Passed", "Failed", "Error", "Warning" };
           
            //create table content
            for (int index = 0; index < 4; index++)
            {
                HtmlTableRow row = new HtmlTableRow();
                if (index == 0)
                {
                    HtmlTableCell cell1 = new HtmlTableCell();
                    cell1.InnerText = runStatus;
                    cell1.Align = "center";
                    cell1.RowSpan = 4;
                    cell1.Attributes.Add("style", "font-weight: bold;");
                    row.Cells.Add(cell1);


                    HtmlTableCell cell2 = new HtmlTableCell();
                    cell2.InnerText = totalTests.ToString();
                    cell2.Align = "center";
                    cell2.RowSpan = 4;
                    cell2.Attributes.Add("style", "font-weight: bold;");
                    row.Cells.Add(cell2);
                }
                
                HtmlTableCell cell3 = new HtmlTableCell();
                cell3.Align = "right";

                HtmlImage statusImage = new HtmlImage();
                statusImage.Src = "https://extensionado.blob.core.windows.net/uft-extension-images/" + statusArray[index].ToLower() + ".png";
                cell3.Controls.Add(statusImage);
                row.Cells.Add(cell3);

                HtmlTableCell cell4 = new HtmlTableCell();
                cell4.Align = "center";
                cell4.InnerText = statusArray[index];
                row.Cells.Add(cell4);

                HtmlTableCell cell5 = new HtmlTableCell();
                cell5.Align = "center";
                cell5.InnerText = nrOfTests[statusArray[index]].ToString();
                row.Cells.Add(cell5);

                HtmlTableCell cell6 = new HtmlTableCell();
                cell6.Align = "center";
                int percentage = (int)Math.Round((double)(100 * nrOfTests[statusArray[index]]) / totalTests);
                cell6.InnerText = percentage + "%";
                row.Cells.Add(cell6);

              
                row.Attributes.Add("style","height: 30px;");
                table.Rows.Add(row);
            }
                     
            //add table to file
            string html;
            var runStatusMessage = new System.Text.StringBuilder();

            using (var sw = new StringWriter())
            {
                table.RenderControl(new System.Web.UI.HtmlTextWriter(sw));
                html = sw.ToString();
            }

            runStatusMessage.AppendFormat(html);

            System.IO.File.WriteAllText(uftWorkingFolder + @"\\res\\Report_" + buildNumber + "\\Run status summary", runStatusMessage.ToString());
        }


        public static void createJUnitReport(Dictionary<string,List<ReportMetaData>> reports, string uftWorkingFolder, string buildNumber)
        {
            HtmlTable table = new HtmlTable();
            HtmlTableRow header = new HtmlTableRow();

            HtmlTableCell h1 = new HtmlTableCell();
            h1.InnerText = "Test name";
            h1.Width = "150";
            h1.Align = "center";
            h1.Attributes.Add("style", "font-weight: bold;");
            header.Cells.Add(h1);

            HtmlTableCell h2= new HtmlTableCell();
            h2.InnerText = "Failed steps";
            h2.Width = "300";
            h2.Align = "left";
            h2.Attributes.Add("style", "font-weight: bold;");
            header.Cells.Add(h2);

            HtmlTableCell h3 = new HtmlTableCell();
            h3.InnerText = "Duration(s)";
            h3.Width = "100";
            h3.Align = "left";
            h3.Attributes.Add("style", "font-weight: bold;");
            header.Cells.Add(h3);

            HtmlTableCell h4 = new HtmlTableCell();
            h4.InnerText = "Error details";
            h4.Width = "600";
            h4.Align = "left";
            h4.Attributes.Add("style", "font-weight: bold;");
            header.Cells.Add(h4);

            header.BgColor = KnownColor.Azure.ToString();
            table.Rows.Add(header);
           
            Dictionary<string, int> numberOfFailedSteps = getNumberOfFailedSteps(reports);

            foreach (string testName in reports.Keys) 
            {
                int index = 0;
                foreach (var item in reports[testName])
                {
                    if (!String.IsNullOrEmpty(item.getStatus()) && item.getStatus().Equals("fail"))
                    {
                        HtmlTableRow row = new HtmlTableRow();
                        if (index == 0)
                        {
                            HtmlTableCell cell1 = new HtmlTableCell();
                            cell1.InnerText = testName;
                            cell1.Align = "center";
                            cell1.Attributes.Add("style", "font-weight: bold; text-decoration: underline;");
                            cell1.RowSpan = numberOfFailedSteps[testName];
                            row.Cells.Add(cell1);
                        }

                        HtmlTableCell cell2 = new HtmlTableCell();
                        cell2.InnerText = item.getDisplayName();
                        cell2.Align = "left";
                        row.Cells.Add(cell2);

                        HtmlTableCell cell3 = new HtmlTableCell();
                        cell3.InnerText = item.getDuration();
                        cell3.Align = "left";
                        row.Cells.Add(cell3);

                        HtmlTableCell cell4 = new HtmlTableCell();
                        cell4.InnerText = item.getErrorMessage();
                        cell4.Align = "left";
                        row.Cells.Add(cell4);

                        row.Attributes.Add("style", "height: 30px;");
                        table.Rows.Add(row);

                        index++;
                    }
                }
            }

            //add table to file
            string html;
            var junitStatusMessage = new System.Text.StringBuilder();

            using (var sw = new StringWriter())
            {
                table.RenderControl(new System.Web.UI.HtmlTextWriter(sw));
                html = sw.ToString();
            }

            junitStatusMessage.AppendFormat(html);

            System.IO.File.WriteAllText(uftWorkingFolder + @"\\res\\Report_" + buildNumber + "\\Failed tests", junitStatusMessage.ToString());
        }

        private static Dictionary<string, int> getNumberOfFailedSteps(Dictionary<string, List<ReportMetaData>> reports)
        {
            Dictionary<string, int> numberOfFailedSteps = new Dictionary<string, int>();
            foreach(var test in reports.Keys)
            {
                int failedSteps = 0;
                foreach (var item in reports[test])
                {
                    if (!String.IsNullOrEmpty(item.getStatus()) && item.getStatus().Equals("fail"))
                    {
                        failedSteps++;
                    }
                }
                numberOfFailedSteps[test] = failedSteps;
            }
            return numberOfFailedSteps;
        }

        private static string getTestName(string testPath)
        {
            int pos = testPath.LastIndexOf("\\", StringComparison.Ordinal) + 1;
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
