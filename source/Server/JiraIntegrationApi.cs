using System;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Web.Api;
using Octopus.Server.Extensibility.JiraIntegration.Web;

namespace Octopus.Server.Extensibility.JiraIntegration
{
    class JiraIntegrationApi : RegisterEndpoint
    {
        public const string ApiConnectAppCredentialsTest = "/api/jiraintegration/connectivitycheck/connectapp";
        public const string ApiJiraCredentialsTest = "/api/jiraintegration/connectivitycheck/jira";
        
        public JiraIntegrationApi(
            Func<SecuredAsyncActionInvoker<JiraConnectAppConnectivityCheckAction>> jiraConnectAppConnectivityCheckInvokerFactory,
            Func<SecuredAsyncActionInvoker<JiraCredentialsConnectivityCheckAction>> jiraCredentialsConnectivityCheckInvokerFactory)
        {
            Add("POST", ApiJiraCredentialsTest, jiraCredentialsConnectivityCheckInvokerFactory().ExecuteAsync);
            Add("POST", ApiConnectAppCredentialsTest, jiraConnectAppConnectivityCheckInvokerFactory().ExecuteAsync);
        }
    }
}