using PSModule.Models;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Xml;
using System.Web.UI.WebControls;
using System.IO;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Drawing;
using PSModule.Properties;

namespace TestReport
{
    [Cmdlet(VerbsLifecycle.Invoke, "ReportTask")]
    public class InvokeReportTask : Cmdlet
    {
        [Parameter(Position = 0)]
        public string ResultsFilename
        {
            get { return resultsFileName; }
            set { resultsFileName = value; }
        }

        [Parameter(Position = 1)]
        public string UftWorkingFolder
        {
            get { return uftWorkingFolder; }
            set { uftWorkingFolder = value; }
        }


        private string resultsFileName;
        private string uftWorkingFolder;
        private List<ReportMetaData> listReport;

        protected override void ProcessRecord()
        {
            listReport = new List<ReportMetaData>();
            readReportFromXMLFile(resultsFileName, ref listReport);
            /*if (listReport != null)
            {
                foreach (ReportMetaData dataReport in listReport)
                {
                    WriteObject("Test: " + getTestName(dataReport.getDisplayName()) + ", " + dataReport.getDateTime() + ", " + dataReport.getStatus());
                }
            }*/

            //create html report
            createSummaryReport(ref listReport);
        }

        private void readReportFromXMLFile(string reportPath, ref List<ReportMetaData> listReport)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(reportPath);

            foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
            {
                foreach (XmlNode currentNode in node)
                {
                    ReportMetaData reportmetadata = new ReportMetaData();
                    XmlAttributeCollection attributesList = currentNode.Attributes;
                    foreach (XmlAttribute attribute in attributesList)
                    {
                        switch (attribute.Name)
                        {
                            case "name": reportmetadata.setDisplayName(attribute.Value); break;
                            case "status": reportmetadata.setStatus(attribute.Value); break;
                            default: break;
                        }
                    }

                    XmlNodeList nodes = currentNode.ChildNodes;
                    foreach (XmlNode xmlNode in nodes)
                    {
                        if (xmlNode.Name.Equals("system-out"))
                        {
                            reportmetadata.setDateTime(xmlNode.InnerText.Substring(0, 19));
                        }
                    }

                    listReport.Add(reportmetadata);
                }
            }
        }

        private string getTestName(string testPath)
        {
            int pos = testPath.LastIndexOf("\\", StringComparison.Ordinal) + 1;
            return testPath.Substring(pos, testPath.Length - pos);
        }

        private void createSummaryReport(ref List<ReportMetaData> reportList)
        {
            /*string imagePath = uftWorkingFolder + "\\bin";
            string path = imagePath + "\\passed.png";
            WriteObject("Imaae path: " + path);

            Assembly myAssembly = Assembly.GetExecutingAssembly();
            string[] names = myAssembly.GetManifestResourceNames();
            foreach (string name in names)
            {
                Console.WriteLine(name);
            }

            Image passedImage = new Image();
            passedImage.ImageUrl = "/PSModule;component/Resource/passed.png";
            passedImage.ImageAlign = ImageAlign.Middle;
            //passedImage.ImageUrl = imagePath + "\\passed.png"; // PSModule.Properties.Resources.passed.;

            Image failedImage = new Image();
            failedImage.ImageUrl = "~" + imagePath + "\\failed.png";
            failedImage.ImageAlign = ImageAlign.Middle;

            //var table = new HtmlTable();
            var table = new Table();
               
            //create header
            TableRow header = new TableHeaderRow();
           
            var cellTestName = new TableCell();
            cellTestName.Text = "Test name";
            cellTestName.HorizontalAlign = HorizontalAlign.Center;
            cellTestName.Width = 100;

            var cellTimestamp = new TableCell();
            cellTimestamp.Text = "Timestamp";
            cellTimestamp.HorizontalAlign = HorizontalAlign.Center;
            cellTimestamp.Width = 150;

            var cellStatus = new TableCell();
            cellStatus.Text = "Status";
            cellStatus.HorizontalAlign = HorizontalAlign.Center;
            cellStatus.Width = 50;

            /*var cellXmlReport = new TableCell();
            cellXmlReport.Text = "Xml report";
            cellXmlReport.HorizontalAlign = HorizontalAlign.Center;
            cellXmlReport.Width = 100;*/

            /* var cellHtmlReport = new TableCell();
             cellHtmlReport.Text = "Html report";
             cellHtmlReport.HorizontalAlign = HorizontalAlign.Center;
             cellHtmlReport.Width = 100;

             header.Cells.Add(cellTestName);
             header.Cells.Add(cellTimestamp);
             header.Cells.Add(cellStatus);
            // header.Cells.Add(cellXmlReport);
             header.Cells.Add(cellHtmlReport);

             header.BackColor = System.Drawing.Color.LightBlue;
             table.Rows.Add(header);*/
            HtmlTable table = new HtmlTable();
            HtmlTableRow header = new HtmlTableRow();
            HtmlTableCell h1 = new HtmlTableCell();
            h1.InnerText = "Test name";
            h1.Width = "100";
            h1.Align = "center";
            header.Cells.Add(h1);

            HtmlTableCell h2 = new HtmlTableCell();
            h2.InnerText = "Timestamp";
            h2.Width = "150";
            h2.Align = "center";
            header.Cells.Add(h2);

            HtmlTableCell h3 = new HtmlTableCell();
            h3.InnerText = "Status";
            h3.Width = "50";
            h3.Align = "center";
            header.Cells.Add(h3);

            HtmlTableCell h4 = new HtmlTableCell();
            h4.InnerText = "HTML report";
            h4.Width = "100";
            h4.Align = "center";
            header.Cells.Add(h4);

            header.BgColor = KnownColor.Azure.ToString();
            table.Rows.Add(header);

            //create table content
            foreach (ReportMetaData report in reportList)
            {
                /*TableRow row = new TableRow();
                var cell1 = new TableCell();
                cell1.Text = getTestName(report.getDisplayName());
                cell1.HorizontalAlign = HorizontalAlign.Center;
                var cell2 = new TableCell();
                cell2.Text = report.getDateTime();
                cell2.HorizontalAlign = HorizontalAlign.Center;
                var cell3 = new TableCell();
                if (report.getStatus().Equals("pass"))
                {
                    cell3.Controls.Add(passedImage);
                } else
                {
                    cell3.Controls.Add(failedImage);
                }
                //cell3.Text = report.getStatus();
               /* cell3.HorizontalAlign = HorizontalAlign.Center;
                var cell4 = new TableCell();
                HyperLink xmlReportLink = new HyperLink();
                xmlReportLink.Text = "run_results.xml";
                xmlReportLink.NavigateUrl = "PSModule/Reports/run_results.xml";
                cell4.Controls.Add(xmlReportLink);
                cell4.HorizontalAlign = HorizontalAlign.Center;*/

                /*var cell5 = new TableCell();
                HyperLink htmlReportLink = new HyperLink();
                htmlReportLink.Text = "run_results.html";
                //Console.WriteLine(PSModule.Properties.Resources.run_results);

                htmlReportLink.NavigateUrl = imagePath + "\\run_results.html";
                cell5.Controls.Add(htmlReportLink);
                cell5.HorizontalAlign = HorizontalAlign.Center;

                row.Cells.Add(cell1);
                row.Cells.Add(cell2);
                row.Cells.Add(cell3);
                //row.Cells.Add(cell4);
                row.Cells.Add(cell5);

                table.Rows.Add(row);*/

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
                    statusImage.Src = "data:image/png;base64," + ImageToBase64(Resources.passed);
                }
                else
                {
                    statusImage.Src = "data:image/png;base64," + ImageToBase64(Resources.failed);
                }

                cell3.Align = "center";
                cell3.Controls.Add(statusImage);
                row.Cells.Add(cell3);

                HtmlTableCell cell4 = new HtmlTableCell();
                HtmlAnchor reportLink = new HtmlAnchor();
             
                //Console.WriteLine("path: " + Resources.run_results);
                reportLink.HRef = "C:\\Users\\laakso.CORPDOM\\TFS\\TFS_project\\UFTWorking\\res\\run_results.html"; //Path.GetFullPath(Resources.run_results); 
                reportLink.InnerText = "report";
         
                cell4.Controls.Add(reportLink);
                cell4.Align = "center";
                row.Cells.Add(cell4);

                table.Rows.Add(row);
            }

            //add table to file
            string html;
            var reportMessage = new System.Text.StringBuilder();
            reportMessage.Append("<!DOCTYPE html>< html >< head /> <body>");
            using (var sw = new StringWriter())
            {
                table.RenderControl(new HtmlTextWriter(sw));
                html = sw.ToString();
            }

            reportMessage.AppendFormat(html);
            reportMessage.Append("</body></html>");

            System.IO.File.WriteAllText(uftWorkingFolder + @"\res\UFT Report", reportMessage.ToString());
        }


        public static string ImageToBase64(System.Drawing.Image _imagePath)
        {
            byte[] imageBytes = ImageToByteArray(_imagePath);
            string base64String = Convert.ToBase64String(imageBytes);
            return base64String;
        }

        public static byte[] ImageToByteArray(System.Drawing.Image imageIn)
        {
            using (var ms = new MemoryStream())
            {
                imageIn.Save(ms, imageIn.RawFormat);
                return ms.ToArray();
            }
        }
    }
}