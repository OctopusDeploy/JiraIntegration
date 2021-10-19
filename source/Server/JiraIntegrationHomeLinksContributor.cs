using System.Collections.Generic;
using Octopus.Server.Extensibility.HostServices.Web;

namespace Octopus.Server.Extensibility.JiraIntegration
{
    class JiraIntegrationHomeLinksContributor : IHomeLinksContributor
    {
        public const string ApiConnectAppCredentialsTestLinkName = "JiraConnectAppCredentialsTest";
        public const string ApiJiraCredentialsTestLinkName = "JiraCredentialsTest";

        public IDictionary<string, string> GetLinksToContribute()
        {
            var linksToContribute = new Dictionary<string, string>
            {
                {ApiConnectAppCredentialsTestLinkName, $"~{JiraIntegrationApi.ApiConnectAppCredentialsTest}"},
                {ApiJiraCredentialsTestLinkName, $"~{JiraIntegrationApi.ApiJiraCredentialsTest}"}
            };

            return linksToContribute;
        }
    }
}