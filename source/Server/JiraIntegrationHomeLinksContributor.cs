using System.Collections.Generic;
using Octopus.Server.Extensibility.HostServices.Web;
using Octopus.Server.Extensibility.JiraIntegration.Configuration;

namespace Octopus.Server.Extensibility.JiraIntegration
{
    internal class JiraIntegrationHomeLinksContributor : IHomeLinksContributor
    {
        public const string ApiConnectAppCredentialsTestLinkName = "JiraConnectAppCredentialsTest";
        public const string ApiJiraCredentialsTestLinkName = "JiraCredentialsTest";
        private readonly IJiraConfigurationStore configurationStore;

        public JiraIntegrationHomeLinksContributor(IJiraConfigurationStore configurationStore)
        {
            this.configurationStore = configurationStore;
        }

        public IDictionary<string, string> GetLinksToContribute()
        {
            var linksToContribute = new Dictionary<string, string>
            {
                { ApiConnectAppCredentialsTestLinkName, $"~{JiraIntegrationApi.ApiConnectAppCredentialsTest}" },
                { ApiJiraCredentialsTestLinkName, $"~{JiraIntegrationApi.ApiJiraCredentialsTest}" }
            };

            return linksToContribute;
        }
    }
}