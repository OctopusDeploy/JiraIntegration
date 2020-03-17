using System;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Web.Api;
using Octopus.Server.Extensibility.JiraIntegration.Web;

namespace Octopus.Server.Extensibility.JiraIntegration
{
    class JiraIssueTrackerApi : RegisterEndpoint
    {
        public const string ApiConnectAppCredentialsTest = "/api/jiraissuetracker/connectivitycheck/connectapp";
        public const string ApiJiraCredentialsTest = "/api/jiraissuetracker/connectivitycheck/jira";
        
        public JiraIssueTrackerApi(
            Func<SecuredAsyncActionInvoker<JiraConnectAppConnectivityCheckAction>> jiraConnectAppConnectivityCheckInvokerFactory,
            Func<SecuredAsyncActionInvoker<JiraCredentialsConnectivityCheckAction>> jiraCredentialsConnectivityCheckInvokerFactory)
        {
            Add("POST", ApiJiraCredentialsTest, jiraCredentialsConnectivityCheckInvokerFactory().ExecuteAsync);
            Add("POST", ApiConnectAppCredentialsTest, jiraConnectAppConnectivityCheckInvokerFactory().ExecuteAsync);
        }
    }
}