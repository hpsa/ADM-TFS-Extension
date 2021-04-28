using System;

namespace PSModule.Models
{
    public class ReportMetaData
    {
        private string reportPath { get; set; } //slave path of report folder(only for html report format)

        private string displayName { get; set; }

        private string resourceURL { get; set; }

        private string dateTime { get; set; }

        private string status { get; set; }

        private string duration { get; set; }

        private string errorMessage { get; set; }

        public void setDisplayName(string value)
        {
            this.displayName = value;
        }

        public string getDisplayName()
        {
            return displayName;
        }

        public void setReportPath(string value)
        {
            this.reportPath = value;
        }

        internal string getReportPath()
        {
            return reportPath;
        }


        public void setDateTime(string value)
        {
            this.dateTime = value;
        }

        internal string getDateTime()
        {
            return dateTime;
        }

        public void setResourceUrl(string value)
        {
            this.resourceURL = value;
        }

        public void setStatus(string value)
        {
            this.status = value;
        }
        internal string getStatus()
        {
            return status;
        }

        public void setDuration(string value)
        {
            this.duration = value;
        }
        internal string getDuration()
        {
            return duration;
        }

        public void setErrorMessage(string value)
        {
            this.errorMessage = value;
        }

        internal string getErrorMessage()
        {
            return errorMessage;
        }
    }
}