using System.Threading.Tasks;
using Octopus.Server.Extensibility.Resources.Configuration;
using Octopus.Server.Extensibility.Results;

namespace Octopus.Server.Extensibility.JiraIntegration.Integration
{
    internal interface IJiraRestClient
    {
        Task<ConnectivityCheckResponse> ConnectivityCheck();
        Task<IResultFromExtension<JiraIssue[]>> GetIssues(string[] workItemId);
    }
}