using System.Threading.Tasks;
using Octopus.Server.Extensibility.Resources.Configuration;

namespace Octopus.Server.Extensibility.JiraIntegration.Integration
{
    internal interface IJiraRestClient
    {
        Task<ConnectivityCheckResponse> ConnectivityCheck();
        Task<JiraSearchResult?> GetIssues(string[] workItemId);
    }
}