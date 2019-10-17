using Octopus.Data.Model;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Configuration;

namespace Octopus.Server.Extensibility.IssueTracker.Jira.Configuration
{
    class JiraConfiguration : ExtensionConfigurationDocument
    {
        public JiraConfiguration() : base("Jira", "Octopus Deploy", "1.1")
        {
            Id = JiraConfigurationStore.SingletonId;
            ConnectAppUrl = "https://jiraconnectapp.octopus.com";
            JiraInstanceType = JiraInstanceType.Cloud;
        }

        public JiraInstanceType JiraInstanceType { get; set; }
        
        public string BaseUrl { get; set; }
        
        [Encrypted]
        public string Password { get; set; }

        public string ConnectAppUrl { get; set; }
        
        public ReleaseNoteOptions ReleaseNoteOptions { get; set; } = new ReleaseNoteOptions();
    }

    enum JiraInstanceType
    {
        Cloud,
        Server
    }

    class ReleaseNoteOptions
    {
        public string Username { get; set; }
        [Encrypted]
        public string Password { get; set; }
        public string ReleaseNotePrefix { get; set; }
    }
}
