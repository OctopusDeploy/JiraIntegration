using System.Threading.Tasks;
using Octopus.Server.Extensibility.Resources.Configuration;

namespace Octopus.Server.Extensibility.JiraIntegration.Integration
{
    interface IJiraRestClient
    {
        Task<ConnectivityCheckResponse> ConnectivityCheck();
        Task<JiraSearchResponse> GetIssues(string[] workItemId);
    }
}