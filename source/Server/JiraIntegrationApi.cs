using Octopus.Server.Extensibility.Extensions.Infrastructure.Web.Api;
using Octopus.Server.Extensibility.JiraIntegration.Web;

namespace Octopus.Server.Extensibility.JiraIntegration
{
    class JiraIntegrationApi
    {
        public const string ApiConnectAppCredentialsTest = "/api/jiraintegration/connectivitycheck/connectapp";
        public const string ApiJiraCredentialsTest = "/api/jiraintegration/connectivitycheck/jira";
    }
}