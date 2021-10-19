using System.Threading;
using System.Threading.Tasks;
using Octopus.Data.Model;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Configuration;

namespace Octopus.Server.Extensibility.JiraIntegration.Configuration
{
    interface IJiraConfigurationStore : IExtensionConfigurationStoreAsync<JiraConfiguration>
    {
        Task<JiraInstanceType> GetJiraInstanceType(CancellationToken cancellationToken);
        Task SetJiraInstanceType(JiraInstanceType jiraInstanceType, CancellationToken cancellationToken);
        Task<string?> GetBaseUrl(CancellationToken cancellationToken);
        Task SetBaseUrl(string? baseUrl, CancellationToken cancellationToken);

        Task<SensitiveString?> GetConnectAppPassword(CancellationToken cancellationToken);
        Task SetConnectAppPassword(SensitiveString? password, CancellationToken cancellationToken);

        Task<string?> GetConnectAppUrl(CancellationToken cancellationToken);
        Task SetConnectAppUrl(string? url, CancellationToken cancellationToken);

        Task<string?> GetJiraUsername(CancellationToken cancellationToken);
        Task SetJiraUsername(string? username, CancellationToken cancellationToken);

        Task<SensitiveString?> GetJiraPassword(CancellationToken cancellationToken);
        Task SetJiraPassword(SensitiveString? password, CancellationToken cancellationToken);

        Task<string?> GetReleaseNotePrefix(CancellationToken cancellationToken);
        Task SetReleaseNotePrefix(string? releaseNotePrefix, CancellationToken cancellationToken);
    }
}
