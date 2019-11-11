using System.Collections.Generic;
using Octopus.Server.Extensibility.HostServices.Web;
using Octopus.Server.Extensibility.IssueTracker.Jira.Configuration;

namespace Octopus.Server.Extensibility.IssueTracker.Jira
{
    class JiraIssueTrackerHomeLinksContributor : IHomeLinksContributor
    {
        private readonly IJiraConfigurationStore configurationStore;
        public const string ApiConnectAppCredentialsTestLinkName = "JiraConnectAppCredentialsTest";
        public const string ApiJiraCredentialsTestLinkName = "JiraCredentialsTest";

        public JiraIssueTrackerHomeLinksContributor(IJiraConfigurationStore configurationStore)
        {
            this.configurationStore = configurationStore;
        }
        
        public IDictionary<string, string> GetLinksToContribute()
        {
            var linksToContribute = new Dictionary<string, string>
            {
                {ApiConnectAppCredentialsTestLinkName, $"~{JiraIssueTrackerApi.ApiConnectAppCredentialsTest}"},
                {ApiJiraCredentialsTestLinkName, $"~{JiraIssueTrackerApi.ApiJiraCredentialsTest}"}
            };

            return linksToContribute;
        }
    }
}