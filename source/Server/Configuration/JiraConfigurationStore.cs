using System.Threading;
using System.Threading.Tasks;
using Octopus.Data.Model;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Configuration;

namespace Octopus.Server.Extensibility.JiraIntegration.Configuration
{
    class JiraConfigurationStore : ExtensionConfigurationStoreAsync<JiraConfiguration>, IJiraConfigurationStore
    {
        public static string CommentParser = "Jira";
        public static string SingletonId = "jira-integration";

        public JiraConfigurationStore(IConfigurationStoreAsync configurationStore) : base(configurationStore)
        {
        }

        public override string Id => SingletonId;

        public async Task<JiraInstanceType> GetJiraInstanceType(CancellationToken cancellationToken)
        {
            return await GetProperty(doc => doc.JiraInstanceType, cancellationToken);
        }

        public async Task SetJiraInstanceType(JiraInstanceType jiraInstanceType, CancellationToken cancellationToken)
        {
            await SetProperty(doc => doc.JiraInstanceType = jiraInstanceType, cancellationToken);
        }

        public async Task<string?> GetBaseUrl(CancellationToken cancellationToken)
        {
            return await GetProperty(doc => doc.BaseUrl?.Trim('/'), cancellationToken);
        }

        public async Task SetBaseUrl(string? baseUrl, CancellationToken cancellationToken)
        {
            await SetProperty(doc => doc.BaseUrl = baseUrl?.Trim('/'), cancellationToken);
        }

        public async Task<SensitiveString?> GetConnectAppPassword(CancellationToken cancellationToken)
        {
            return await GetProperty(doc => doc.Password, cancellationToken);
        }

        public async Task SetConnectAppPassword(SensitiveString? password, CancellationToken cancellationToken)
        {
            await SetProperty(doc => doc.Password = password, cancellationToken);
        }

        public async Task<string?> GetConnectAppUrl(CancellationToken cancellationToken)
        {
            return await GetProperty(doc => doc.ConnectAppUrl?.Trim('/'), cancellationToken);
        }

        public async Task SetConnectAppUrl(string? url, CancellationToken cancellationToken)
        {
            await SetProperty(doc => doc.ConnectAppUrl = url?.Trim('/'), cancellationToken);
        }

        public async Task<string?> GetJiraUsername(CancellationToken cancellationToken)
        {
            return await GetProperty(doc => doc.ReleaseNoteOptions.Username, cancellationToken);
        }

        public async Task SetJiraUsername(string? username, CancellationToken cancellationToken)
        {
            await SetProperty(doc => doc.ReleaseNoteOptions.Username = username, cancellationToken);
        }

        public async Task<SensitiveString?> GetJiraPassword(CancellationToken cancellationToken)
        {
            return await GetProperty(doc => doc.ReleaseNoteOptions.Password, cancellationToken);
        }

        public async Task SetJiraPassword(SensitiveString? password, CancellationToken cancellationToken)
        {
            await SetProperty(doc => doc.ReleaseNoteOptions.Password = password, cancellationToken);
        }

        public async Task<string?> GetReleaseNotePrefix(CancellationToken cancellationToken)
        {
            return await GetProperty(doc => doc.ReleaseNoteOptions.ReleaseNotePrefix, cancellationToken);
        }

        public async Task SetReleaseNotePrefix(string? releaseNotePrefix, CancellationToken cancellationToken)
        {
            await SetProperty(doc => doc.ReleaseNoteOptions.ReleaseNotePrefix = releaseNotePrefix, cancellationToken);
        }
    }
}
