using System.Threading;
using System.Threading.Tasks;
using Octopus.Server.Extensibility.Results;

namespace Octopus.Server.Extensibility.JiraIntegration.Integration
{
    internal interface IJiraRestClient
    {
        Task<IResultFromExtension<JiraIssue[]>> GetIssues(string[] workItemId, CancellationToken cancellationToken);
    }
}