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

        [Parameter(Position = 2)]
        public string ALMPassword;

        [Parameter(Position = 3, Mandatory = true)]
        public string ALMDomain;

        [Parameter(Position = 4, Mandatory = true)]
        public string ALMProject;

        [Parameter(Position = 5)]
        public string RunType;

        [Parameter(Position = 6)]
        public string TestSet;

        [Parameter(Position = 7)]
        public string Description;

        [Parameter(Position = 8, Mandatory = true)]
        public string TimeSlotDuration;

        [Parameter(Position = 9)]
        public string EnvironmentConfigurationID;

        [Parameter(Position = 10)]
        public string UseCDA;

        [Parameter(Position = 11)]
        public string DeploymentAction;

        [Parameter(Position = 12)]
        public string DeploymentEnvironmentName;

        [Parameter(Position = 13)]
        public string DeprovisioningAction;

        protected override void ProcessRecord()
        {

        }
    }
}
