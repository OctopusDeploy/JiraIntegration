using System;
using Octopus.Server.Extensibility.Extensions.WorkItems;
using Octopus.Server.Extensibility.JiraIntegration.Configuration;

namespace Octopus.Server.Extensibility.JiraIntegration
{
    class JiraIntegration : IIssueTracker
    {
        internal static string Name = "Jira";

        readonly IJiraConfigurationStore configurationStore;

        public JiraIntegration(IJiraConfigurationStore configurationStore)
        {
            this.configurationStore = configurationStore;
        }

        public string CommentParser => JiraConfigurationStore.CommentParser;
        public string IssueTrackerName => Name;

        public bool IsEnabled => configurationStore.GetIsEnabled();

        public string? BaseUrl => configurationStore.GetIsEnabled() ? configurationStore.GetBaseUrl() : null;
    }
}
