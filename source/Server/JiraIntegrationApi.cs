using Octopus.Server.Extensibility.Extensions.Infrastructure.Web.Api;
using Octopus.Server.Extensibility.JiraIntegration.Web;

namespace Octopus.Server.Extensibility.JiraIntegration
{
    internal class JiraIntegrationApi : RegistersEndpoints
    {
        public const string ApiConnectAppCredentialsTest = "/api/jiraintegration/connectivitycheck/connectapp";
        public const string ApiJiraCredentialsTest = "/api/jiraintegration/connectivitycheck/jira";

        public JiraIntegrationApi()
        {
            Add<JiraCredentialsConnectivityCheckAction>("POST", ApiJiraCredentialsTest, RouteCategory.Raw,
                new SecuredEndpointInvocation(), null, "JiraIntegration");
            Add<JiraConnectAppConnectivityCheckAction>("POST", ApiConnectAppCredentialsTest, RouteCategory.Raw,
                new SecuredEndpointInvocation(), null, "JiraIntegration");
        }
    }
}