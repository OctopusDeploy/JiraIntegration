using System;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Web.Api;
using Octopus.Server.Extensibility.JiraIntegration.Web;

namespace Octopus.Server.Extensibility.JiraIntegration
{
    class JiraIntegrationApi : RegistersEndpoints
    {
        public const string ApiConnectAppCredentialsTest = "/jiraintegration/connectivitycheck/connectapp";
        public const string ApiJiraCredentialsTest = "/jiraintegration/connectivitycheck/jira";

        public JiraIntegrationApi()
        {
            Add<JiraCredentialsConnectivityCheckAction>("POST", ApiJiraCredentialsTest, new SecuredEndpointInvocation());
            Add<JiraConnectAppConnectivityCheckAction>("POST", ApiConnectAppCredentialsTest, new SecuredEndpointInvocation());
        }
    }
}