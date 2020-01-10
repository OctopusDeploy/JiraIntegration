using System.Threading.Tasks;
using Octopus.Server.Extensibility.Resources.Configuration;

namespace Octopus.Server.Extensibility.IssueTracker.Jira.Integration
{
    interface IJiraRestClient
    {
        Task<ConnectivityCheckResponse> ConnectivityCheck();
        Task<JiraIssue> GetIssue(string workItemId);
        Task<JiraIssueComments> GetIssueComments(string workItemId);
    }
}