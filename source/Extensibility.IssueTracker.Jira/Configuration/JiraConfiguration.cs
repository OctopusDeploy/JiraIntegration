using System;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Configuration;

namespace Octopus.Server.Extensibility.IssueTracker.Jira.Configuration
{
    public class JiraConfiguration : ExtensionConfigurationDocument
    {
        public JiraConfiguration() : base("Jira", "Octopus Deploy", "1.0")
        {
            Id = JiraConfigurationStore.SingletonId;
        }

        public string BaseUrl { get; set; }
        public string ConnectAppUrl { get; set; }
    }
}
