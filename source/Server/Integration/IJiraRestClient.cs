using System.Collections.Generic;
using System.Threading.Tasks;

namespace Octopus.Server.Extensibility.IssueTracker.Jira.Integration
{
    public interface IJiraRestClient
    {
        Task<JiraIssue> GetIssue(string workItemId);
        Task<JiraIssueComments> GetIssueComments(string workItemId);
    }
}