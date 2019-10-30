using System.Collections.Generic;
using System.Threading.Tasks;
using Octopus.Server.Extensibility.IssueTracker.Jira.Web.Response;

namespace Octopus.Server.Extensibility.IssueTracker.Jira.Integration
{
    public interface IJiraRestClient
    {
        Task<ConnectivityCheckResponse> ConnectivityCheck();
        Task<JiraIssue> GetIssue(string workItemId);
        Task<JiraIssueComments> GetIssueComments(string workItemId);
    }
}