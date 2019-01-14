using System.ComponentModel;
using Octopus.Client.Extensibility.Attributes;
using Octopus.Client.Extensibility.Extensions.Infrastructure.Configuration;
using Octopus.Client.Model;

namespace Octopus.Client.Extensibility.IssueTracker.Jira
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