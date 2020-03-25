using PSModule.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PSModule
{
    class Helper
    {
        public static List<ReportMetaData> readReportFromXMLFile(string reportPath)
        {
            List<ReportMetaData> listReport = new List<ReportMetaData>();
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
    }
}
