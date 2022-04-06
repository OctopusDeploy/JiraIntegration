using System;
using System.Collections.Generic;
using Octopus.Data.Model;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Configuration;
using Octopus.Server.Extensibility.HostServices.Configuration;
using Octopus.Server.Extensibility.HostServices.Licensing;
using Octopus.Server.Extensibility.HostServices.Mapping;

namespace Octopus.Server.Extensibility.JiraIntegration.Configuration
{
    class JiraConfigurationSettings : ExtensionConfigurationSettings<JiraConfiguration, JiraConfigurationResource, IJiraConfigurationStore>, IJiraConfigurationSettings
    {
        readonly IInstallationIdProvider installationIdProvider;
        readonly IServerConfigurationStore serverConfigurationStore;

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

        public override IEnumerable<IConfigurationValue> GetConfigurationValues()
        {
            var isEnabled = ConfigurationDocumentStore.GetIsEnabled();

            yield return new ConfigurationValue<bool>("Octopus.JiraIntegration.IsEnabled", isEnabled, isEnabled, "Is Enabled");
            yield return new ConfigurationValue<string?>("Octopus.JiraIntegration.BaseUrl", ConfigurationDocumentStore.GetBaseUrl(), isEnabled && !string.IsNullOrWhiteSpace(ConfigurationDocumentStore.GetBaseUrl()), "Jira Base Url");
            yield return new ConfigurationValue<SensitiveString?>("Octopus.JiraIntegration.ConnectAppPassword", ConfigurationDocumentStore.GetConnectAppPassword(), isEnabled && !string.IsNullOrWhiteSpace(ConfigurationDocumentStore.GetConnectAppPassword()?.Value), "Jira Connect App Password");
            yield return new ConfigurationValue<string?>("Octopus.JiraIntegration.Username", ConfigurationDocumentStore.GetJiraUsername(), isEnabled && !string.IsNullOrWhiteSpace(ConfigurationDocumentStore.GetJiraUsername()), "Jira Username");
            yield return new ConfigurationValue<SensitiveString?>("Octopus.JiraIntegration.Password", ConfigurationDocumentStore.GetJiraPassword(), isEnabled && !string.IsNullOrWhiteSpace(ConfigurationDocumentStore.GetJiraPassword()?.Value), "Jira Password");
            yield return new ConfigurationValue<string?>("Octopus.JiraIntegration.IssueTracker.JiraReleaseNotePrefix", ConfigurationDocumentStore.GetReleaseNotePrefix(), isEnabled && !string.IsNullOrWhiteSpace(ConfigurationDocumentStore.GetReleaseNotePrefix()), "Jira Release Note Prefix");
        }

        public override void BuildMappings(IResourceMappingsBuilder builder)
        {
            builder.Map<JiraConfigurationResource, JiraConfiguration>()
                .EnrichResource(
                    (model, resource) =>
                    {
                        resource.OctopusInstallationId = installationIdProvider.GetInstallationId().ToString();
                        resource.OctopusServerUrl = serverConfigurationStore.GetServerUri();
                    });
            builder.Map<ReleaseNoteOptionsResource, ReleaseNoteOptions>();
        }
    }
}
