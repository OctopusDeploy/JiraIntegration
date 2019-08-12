using System.ComponentModel;
using Octopus.Client.Extensibility.Attributes;
using Octopus.Client.Extensibility.Extensions.Infrastructure.Configuration;
using Octopus.Client.Model;

namespace Octopus.Client.Extensibility.IssueTracker.Jira
{
    public class JiraConfigurationResource : ExtensionConfigurationResource
    {
        public const string JiraBaseUrlDescription = "Set the base url for the Jira instance.";
        public const string ApiConnectAppCredentialsTest = "/api/jiraissuetracker/connectivitycheck/connectapp";

        public JiraConfigurationResource()
        {
            Id = "issuetracker-jira";
        }

        [DisplayName("Jira Base Url")]
        [Description(JiraBaseUrlDescription)]
        [Writeable]
        public string BaseUrl { get; set; }

        [DisplayName("Jira Connect App Password")]
        [Description("Set the password for authenticating with the Jira Connect App")]
        [Writeable]
        [AllowConnectivityCheck("Jira Connect App credentials", ApiConnectAppCredentialsTest, nameof(BaseUrl), nameof(Password))]
        public SensitiveValue Password { get; set; }

        [DisplayName("Octopus Installation Id")]
        [Description("Use this Id when configuring the Jira connect application")]
        [ReadOnly(true)]
        public string OctopusInstallationId { get; set; }

        [DisplayName("Release Note Options")]
        public ReleaseNoteOptionsResource ReleaseNoteOptions { get; set; } = new ReleaseNoteOptionsResource();
    }

    public class ReleaseNoteOptionsResource
    {
        public const string UsernameDescription = "Set the username to authenticate with against your Jira instance.";
        public const string PasswordDescription = "Set the password to authenticate with against your Jira instance.";
        public const string ReleaseNotePrefixDescription = "Set the prefix to look for when finding release notes for Jira issues. For example `Release note:`.";
        public const string ApiJiraCredentialsTest = "/api/jiraissuetracker/connectivitycheck/jira";

        [DisplayName("Jira Username")]
        [Description(UsernameDescription)]
        [Writeable]
        public string Username { get; set; }
        
        [DisplayName("Jira Password")]
        [Description(PasswordDescription)]
        [Writeable]
        [AllowConnectivityCheck("Jira credentials", ApiJiraCredentialsTest, nameof(JiraConfigurationResource.BaseUrl), nameof(Username), nameof(Password))]
        public SensitiveValue Password { get; set; }

        [DisplayName("Release Note Prefix")]
        [Description(ReleaseNotePrefixDescription)]
        [Writeable]
        public string ReleaseNotePrefix { get; set; }
    }
}