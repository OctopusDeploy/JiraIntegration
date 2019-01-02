using System.Collections.Generic;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Configuration;
using Octopus.Server.Extensibility.HostServices.Mapping;

namespace Octopus.Server.Extensibility.IssueTracker.Jira.Configuration
{
    public class JiraConfigurationSettings : ExtensionConfigurationSettings<JiraConfiguration, JiraConfigurationResource, IJiraConfigurationStore>, IJiraConfigurationSettings
    {
        public JiraConfigurationSettings(IJiraConfigurationStore configurationDocumentStore) : base(configurationDocumentStore)
        {
        }

        public override string Id => JiraConfigurationStore.SingletonId;

        public override string ConfigurationSetName => "Jira Issue Tracker";

        public override string Description => "Jira Issue Tracker settings";

        public override IEnumerable<ConfigurationValue> GetConfigurationValues()
        {
            var isEnabled = ConfigurationDocumentStore.GetIsEnabled();

            yield return new ConfigurationValue("Octopus.WebPortal.JiraIssueTracker", isEnabled.ToString(), isEnabled, "Is Enabled");
            yield return new ConfigurationValue("Octopus.WebPortal.JiraBaseUrl", ConfigurationDocumentStore.GetBaseUrl(), isEnabled && !string.IsNullOrWhiteSpace(ConfigurationDocumentStore.GetBaseUrl()), "Jira Base Url");
        }

        public override void BuildMappings(IResourceMappingsBuilder builder)
        {
            builder.Map<JiraConfigurationResource, JiraConfiguration>();
        }
    }
}