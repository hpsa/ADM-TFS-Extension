using System;

namespace PSModule.Models
{
    public class ReportMetaData
    {
        private string folderPath { get; set; } //slave path of report folder(only for html report format)
        private string displayName { get; set; }

        //private string urlName { get; set; }

        //private string resourceURL { get; set; }

        private string dateTime { get; set; }

        private string status { get; set; }
    

        public void setDisplayName(string value)
        {
            this.displayName = value;
        }

        public string getDisplayName()
        {
            return displayName;
        }

       /* public void setUrlName(string value)
        {
            this.urlName = value;
        }*/

        public void setDateTime(string value)
        {
            this.dateTime = value;
        }

        internal string getDateTime()
        {
            return dateTime;
        }

       /* public void setResourceUrl(string value)
        {
            this.resourceURL = value;
        }*/

        public void setStatus(string value)
        {
            this.status = value;
        }
        internal string getStatus()
        {
            return status;
        }
    }
}