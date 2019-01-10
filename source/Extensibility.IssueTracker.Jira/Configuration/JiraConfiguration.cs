using Octopus.Server.Extensibility.Extensions.Infrastructure.Configuration;
using Octopus.Shared.Model;

namespace Octopus.Server.Extensibility.IssueTracker.Jira.Configuration
{
    public class JiraConfiguration : ExtensionConfigurationDocument
    {
        public JiraConfiguration() : base("Jira", "Octopus Deploy", "1.0")
        {
            Id = JiraConfigurationStore.SingletonId;
        }

        public string BaseUrl { get; set; }
        [Encrypted]
        public string Password { get; set; }
        public string ConnectAppUrl { get; set; }
    }
}
