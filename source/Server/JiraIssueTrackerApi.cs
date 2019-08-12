using System;
using System.Threading.Tasks;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Web.Api;
using Octopus.Server.Extensibility.IssueTracker.Jira.Configuration;
using Octopus.Server.Extensibility.IssueTracker.Jira.Web;

namespace Octopus.Server.Extensibility.IssueTracker.Jira
{
    public class JiraIssueTrackerApi : RegisterEndpoint
    {
        public const string ApiConnectAppCredentialsTest = "/api/jiraissuetracker/connectivitycheck/connectapp";
        public const string ApiJiraCredentialsTest = "/api/jiraissuetracker/connectivitycheck/jira";
        
        public JiraIssueTrackerApi(
            Func<SecuredAsyncActionInvoker<JiraConnectAppConnectivityCheckAction, IJiraConfigurationStore>> jiraConnectAppConnectivityCheckInvokerFactory,
            Func<SecuredAsyncActionInvoker<JiraCredentialsConnectivityCheckAction, IJiraConfigurationStore>> jiraCredentialsConnectivityCheckInvokerFactory)
        {
            Add("GET", ApiJiraCredentialsTest, jiraCredentialsConnectivityCheckInvokerFactory().ExecuteAsync);
            Add("GET", ApiConnectAppCredentialsTest, jiraConnectAppConnectivityCheckInvokerFactory().ExecuteAsync);
        }
    }
}