using System.ComponentModel;
using Octopus.Data.Resources;
using Octopus.Data.Resources.Attributes;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Configuration;

namespace Octopus.Server.Extensibility.IssueTracker.Jira.Configuration
{
    public class JiraConfigurationResource : ExtensionConfigurationResource
    {
        public const string JiraBaseUrlDescription = "Set the base url for the Jira instance.";

        [DisplayName("Jira Base Url")]
        [Description(JiraBaseUrlDescription)]
        [Writeable]
        public string BaseUrl { get; set; }
        
        [DisplayName("Jira Connect App Password")]
        [Description("Set the password for authenticating with the Jira Connect App")]
        [Writeable]
        public SensitiveValue Password { get; set; }

        [DisplayName("Octopus Installation Id")]
        [Description("Use this Id when configuring the Jira connect application")]
        [ReadOnly(true)]
        public string OctopusInstallationId { get; set; }
    }
}