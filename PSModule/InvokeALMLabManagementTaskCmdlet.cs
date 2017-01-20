using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PSModule
{
    [Cmdlet(VerbsLifecycle.Invoke, "ALMLabManagementTask")]
    public class InvokeALMLabManagementTaskCmdlet : PSCmdlet
    {
        [Parameter(Position = 0, Mandatory = true)]
        public string ALMServerPath;

        [Parameter(Position = 1, Mandatory = true)]
        public string ALMUserName;

        [Parameter(Position = 2, Mandatory = false)]
        public string ALMPassword;

        [Parameter(Position = 3, Mandatory = true)]
        public string ALMDomain;

        [Parameter(Position = 4, Mandatory = true)]
        public string ALMProject;

        [Parameter(Position = 5, Mandatory = false)]
        public string RunType;

        [Parameter(Position = 6, Mandatory = false)]
        public string TestSet;

        [Parameter(Position = 7, Mandatory = false)]
        public string Description;

        [Parameter(Position = 8, Mandatory = true)]
        public string TimeSlotDuration;

        [Parameter(Position = 9, Mandatory = false)]
        public string EnvironmentConfigurationID;

        [Parameter(Position = 10, Mandatory = false)]
        public string UseCDA;

        [Parameter(Position = 11, Mandatory = false)]
        public string DeploymentAction;

        [Parameter(Position = 12, Mandatory = false)]
        public string DeploymentEnvironmentName;

        [Parameter(Position = 13, Mandatory = false)]
        public string DeprovisioningAction;

        protected override void ProcessRecord()
        {

        }
    }
}
