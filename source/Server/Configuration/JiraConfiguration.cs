using Octopus.Data.Model;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Configuration;

namespace Octopus.Server.Extensibility.JiraIntegration.Configuration
{
    class JiraConfiguration : ExtensionConfigurationDocument
    {
        public JiraConfiguration() : base(JiraConfigurationStore.SingletonId, "Jira", "Octopus Deploy", "1.1")
        {
            ConnectAppUrl = "https://jiraconnectapp.octopus.com";
            JiraInstanceType = JiraInstanceType.Cloud;
        }

        public JiraInstanceType JiraInstanceType { get; set; }

        public string? BaseUrl { get; set; }
        
        public SensitiveString? Password { get; set; }

        public string? ConnectAppUrl { get; set; }
        
        public ReleaseNoteOptions ReleaseNoteOptions { get; set; } = new ReleaseNoteOptions();
    }

    enum JiraInstanceType
    {
        Cloud,
        Server
    }

    class ReleaseNoteOptions
    {
        public string? Username { get; set; }
        public SensitiveString? Password { get; set; }
        public string? ReleaseNotePrefix { get; set; }
    }
}
