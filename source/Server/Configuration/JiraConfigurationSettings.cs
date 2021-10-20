using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Octopus.Data.Model;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Configuration;
using Octopus.Server.Extensibility.HostServices.Configuration;
using Octopus.Server.Extensibility.HostServices.Licensing;
using Octopus.Server.Extensibility.HostServices.Mapping;

namespace Octopus.Server.Extensibility.JiraIntegration.Configuration
{
    internal class JiraConfigurationSettings :
        ExtensionConfigurationSettingsAsync<JiraConfiguration, JiraConfigurationResource, IJiraConfigurationStore>,
        IJiraConfigurationSettings
    {
        private readonly IInstallationIdProvider installationIdProvider;
        private readonly IServerConfigurationStore serverConfigurationStore;

        public JiraConfigurationSettings(IJiraConfigurationStore configurationDocumentStore,
            IInstallationIdProvider installationIdProvider,
            IServerConfigurationStore serverConfigurationStore) : base(configurationDocumentStore)
        {
            this.installationIdProvider = installationIdProvider;
            this.serverConfigurationStore = serverConfigurationStore;
        }

        public override string Id => JiraConfigurationStore.SingletonId;

        public override string ConfigurationSetName => "Jira Integration";

        public override string Description => "Jira Integration settings";

        public override async IAsyncEnumerable<IConfigurationValue> GetConfigurationValues(
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var isEnabled = await ConfigurationDocumentStore.GetIsEnabled(cancellationToken);
            yield return new ConfigurationValue<bool>("Octopus.JiraIntegration.IsEnabled", isEnabled, isEnabled,
                "Is Enabled");
            var baseUrl = await ConfigurationDocumentStore.GetBaseUrl(cancellationToken);
            yield return new ConfigurationValue<string?>("Octopus.JiraIntegration.BaseUrl", baseUrl,
                isEnabled && !string.IsNullOrWhiteSpace(baseUrl), "Jira Base Url");
            var connectAppPassword = await ConfigurationDocumentStore.GetConnectAppPassword(cancellationToken);
            yield return new ConfigurationValue<SensitiveString?>("Octopus.JiraIntegration.ConnectAppPassword",
                connectAppPassword, isEnabled && !string.IsNullOrWhiteSpace(connectAppPassword?.Value),
                "Jira Connect App Password");
            var jiraUsername = await ConfigurationDocumentStore.GetJiraUsername(cancellationToken);
            yield return new ConfigurationValue<string?>("Octopus.JiraIntegration.Username", jiraUsername,
                isEnabled && !string.IsNullOrWhiteSpace(jiraUsername), "Jira Username");
            var jiraPassword = await ConfigurationDocumentStore.GetJiraPassword(cancellationToken);
            yield return new ConfigurationValue<SensitiveString?>("Octopus.JiraIntegration.Password", jiraPassword,
                isEnabled && !string.IsNullOrWhiteSpace(jiraPassword?.Value), "Jira Password");
            var releaseNotePrefix = await ConfigurationDocumentStore.GetReleaseNotePrefix(cancellationToken);
            yield return new ConfigurationValue<string?>("Octopus.JiraIntegration.IssueTracker.JiraReleaseNotePrefix",
                releaseNotePrefix, isEnabled && !string.IsNullOrWhiteSpace(releaseNotePrefix),
                "Jira Release Note Prefix");
        }

        public override void BuildMappings(IResourceMappingsBuilder builder)
        {
            builder.Map<JiraConfigurationResource, JiraConfiguration>()
                .EnrichResource((model, resource) =>
                {
                    resource.OctopusInstallationId = installationIdProvider.GetInstallationId().ToString();
                    resource.OctopusServerUrl = serverConfigurationStore.GetServerUri();
                });
            builder.Map<ReleaseNoteOptionsResource, ReleaseNoteOptions>();
        }
    }
}