using System.ComponentModel;
using Octopus.Client.Model;

namespace Octopus.Client.Extensibility.IssueTracker.Jira
{
    public class JiraProjectSettings
    {
        [DisplayName("Work Item Package")]
        [Description("Include the work items from an included package")]
        [ReadOnly(false)]
        public DeploymentActionPackageResource WorkItemPackage { get; set; }

    }
}