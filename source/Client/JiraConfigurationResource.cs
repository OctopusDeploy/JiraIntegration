using System.ComponentModel;
using Octopus.Client.Extensibility.Extensions.Infrastructure.Configuration;
using Octopus.Client.Model;
using Octopus.Data.Resources.Attributes;

namespace Octopus.Client.Extensibility.IssueTracker.Jira
{
    public class JiraConfigurationResource : ExtensionConfigurationResource
    {
        public const string JiraBaseUrlDescription = "Set the base url for the Jira instance.";

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
        public SensitiveValue Password { get; set; }

        [DisplayName("Octopus Installation Id")]
        [Description("Use this Id when configuring the Jira connect application")]
        [ReadOnly(true)]
        [AllowCopyToClipboard]
        public string OctopusInstallationId { get; set; }

        [DisplayName("Release Note Options")]
        public ReleaseNoteOptionsResource ReleaseNoteOptions { get; set; } = new ReleaseNoteOptionsResource();
    }

    public class ReleaseNoteOptionsResource
    {
        public const string UsernameDescription = "Set the username to authenticate with against your Jira instance.";
        public const string PasswordDescription = "Set the password to authenticate with against your Jira instance.";
        public const string ReleaseNotePrefixDescription = "Set the prefix to look for when finding release notes for Jira issues. For example `Release note:`.";

        [DisplayName("Jira Username")]
        [Description(UsernameDescription)]
        [Writeable]
        public string Username { get; set; }
        
        [DisplayName("Jira Password")]
        [Description(PasswordDescription)]
        [Writeable]
        public SensitiveValue Password { get; set; }

        [DisplayName("Release Note Prefix")]
        [Description(ReleaseNotePrefixDescription)]
        [Writeable]
        public string ReleaseNotePrefix { get; set; }
    }
}