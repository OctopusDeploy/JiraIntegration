using System;
using System.Collections.Generic;
using System.Text;
using Octopus.Server.Extensibility.IssueTracker.Extensions;
using Octopus.Server.Extensibility.IssueTracker.Jira.Configuration;

namespace Octopus.Server.Extensibility.IssueTracker.Jira
{
    public class JiraIssueTracker : IIssueTracker
    {
        readonly IJiraConfigurationStore ConfigurationStore;

        public JiraIssueTracker(IJiraConfigurationStore configurationStore)
        {
            this.ConfigurationStore = configurationStore;
        }

        public string IssueTrackerName => "Jira";
        public bool IsEnabled => ConfigurationStore.GetIsEnabled();
    }
}
