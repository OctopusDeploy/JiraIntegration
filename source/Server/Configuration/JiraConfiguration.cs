using Octopus.Server.Extensibility.Extensions.Infrastructure.Configuration;
using Octopus.Shared.Model;

namespace Octopus.Server.Extensibility.IssueTracker.Jira.Configuration
{
    public class JiraConfiguration : ExtensionConfigurationDocument
    {
        public JiraConfiguration() : base("Jira", "Octopus Deploy", "1.1")
        {
            Id = JiraConfigurationStore.SingletonId;
            ConnectAppUrl = "https://jiraconnectapp.octopus.com";
        }

        public string BaseUrl { get; set; }
        [Encrypted]
        public string Password { get; set; }
        public string ConnectAppUrl { get; set; }
        public ReleaseNoteOptions ReleaseNoteOptions { get; set; } = new ReleaseNoteOptions();
    }

    public class ReleaseNoteOptions
    {
        public string Username { get; set; }
        [Encrypted]
        public string Password { get; set; }
        public string ReleaseNotePrefix { get; set; }
    }
}
