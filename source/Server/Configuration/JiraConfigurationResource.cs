using System.ComponentModel;
using Octopus.Data.Resources;
using Octopus.Data.Resources.Attributes;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Configuration;

namespace Octopus.Server.Extensibility.JiraIntegration.Configuration
{
    [Description("Configure the Jira Integration. [Learn more](https://g.octopushq.com/JiraIntegration).")]
    class JiraConfigurationResource : ExtensionConfigurationResource
    {
        public const string JiraBaseUrlDescription = "Enter the base url of your Jira instance. Once set, work item references will render as links.";

        [DisplayName("Jira Instance Type")]
        [Description("Set whether you are using a cloud or server instance of Jira")]
        [Writeable]
        [HasOptions(SelectMode.Single)]
        public JiraInstanceType JiraInstanceType { get; set; }

        [DisplayName("Jira Base Url")]
        [Description(JiraBaseUrlDescription)]
        [Writeable]
        public string? BaseUrl { get; set; }

        [DisplayName("Jira Connect App Password")]
        [Description("Set the password for authenticating with Jira Connect App to allow deployment data to be sent to Jira. [Learn more](https://g.octopushq.com/JiraIntegration#connecting-jira-cloud-and-octopus).")]
        [Writeable]
        [ApplicableWhenSpecificValue(nameof(JiraInstanceType), "Cloud")]
        [AllowConnectivityCheck("Jira Connect App configuration", JiraIntegrationApi.ApiConnectAppCredentialsTest, nameof(BaseUrl), nameof(Password))]
        public SensitiveValue? Password { get; set; }

        [DisplayName("Octopus Installation Id")]
        [Description("Copy and paste this Id when configuring the Jira Connect application")]
        [ReadOnly(true)]
        [AllowCopyToClipboard]
        [ApplicableWhenSpecificValue(nameof(JiraInstanceType), "Cloud")]
        public string OctopusInstallationId { get; set; } = string.Empty;

        [DisplayName("Octopus Server Url")]
        [Description("This Url is required in order to push deployment data to Jira. If it is blank please set it in [Configuration/Nodes](/configuration/nodes)")]
        [ReadOnly(true)]
        [ApplicableWhenSpecificValue(nameof(JiraInstanceType), "Cloud")]
        public string? OctopusServerUrl { get; set; }

        [DisplayName("Release Note Options")]
        public ReleaseNoteOptionsResource ReleaseNoteOptions { get; set; } = new ReleaseNoteOptionsResource();
    }

    class ReleaseNoteOptionsResource
    {
        public const string UsernameDescription = "Set the username to authenticate with against your Jira instance.";
        public const string PasswordDescription = "Set the password to authenticate with against your Jira instance.";
        public const string ReleaseNotePrefixDescription = "Set the prefix to look for when finding release notes for Jira issues. For example `Release note:`.";

        [DisplayName("Jira Username")]
        [Description(UsernameDescription)]
        [Writeable]
        public string? Username { get; set; }

        [DisplayName("Jira Password")]
        [Description(PasswordDescription)]
        [Writeable]
        [AllowConnectivityCheck("Jira credentials", JiraIntegrationApi.ApiJiraCredentialsTest, nameof(JiraConfigurationResource.BaseUrl), nameof(Username), nameof(Password))]
        public SensitiveValue? Password { get; set; }

        [DisplayName("Release Note Prefix")]
        [Description(ReleaseNotePrefixDescription)]
        [Writeable]
        public string? ReleaseNotePrefix { get; set; }
    }
}
