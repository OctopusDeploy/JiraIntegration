using System;
using Octopus.Server.Extensibility.Extensions.WorkItems;
using Octopus.Server.Extensibility.IssueTracker.Jira.Configuration;

namespace Octopus.Server.Extensibility.IssueTracker.Jira
{
    public class JiraIssueTracker : IIssueTracker
    {
        internal static string Name = "Jira";

        readonly IJiraConfigurationStore configurationStore;

        public JiraIssueTracker(IJiraConfigurationStore configurationStore)
        {
            this.configurationStore = configurationStore;
        }

        public string IssueTrackerId => JiraConfigurationStore.SingletonId;
        public string IssueTrackerName => Name;

        public bool IsEnabled => configurationStore.GetIsEnabled();

        public string BaseUrl => configurationStore.GetIsEnabled() ? configurationStore.GetBaseUrl() : null;
    }
}
