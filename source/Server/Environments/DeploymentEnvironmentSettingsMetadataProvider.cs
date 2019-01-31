using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Octopus.Server.Extensibility.Extensions.Model.Environments;
using Octopus.Server.Extensibility.IssueTracker.Jira.Configuration;
using Octopus.Server.Extensibility.Metadata;

namespace Octopus.Server.Extensibility.IssueTracker.Jira.Environments
{
    public class DeploymentEnvironmentSettingsMetadataProvider : IContributeDeploymentEnvironmentSettingsMetadata
    {
        private readonly IJiraConfigurationStore store;

        public DeploymentEnvironmentSettingsMetadataProvider(IJiraConfigurationStore store)
        {
            this.store = store;
        }

        public string ExtensionId => JiraConfigurationStore.SingletonId;
        public string ExtensionName => JiraIssueTracker.Name;

        public List<PropertyMetadata> Properties => store.GetIsEnabled()
            ? new MetadataGenerator().GetMetadata<JiraDeploymentEnvironmentSettings>().Types.First().Properties
            : null;

        internal class JiraDeploymentEnvironmentSettings
        {
            [DisplayName("Jira Environment Type")]
            [Description("The Jira environment type of this Octopus deployment environment.")]
            [ReadOnly(false)]
            public JiraEnvironmentType JiraEnvironmentType { get; set; }
        }
    }

    public enum JiraEnvironmentType
    {
        unmapped,
        development,
        testing,
        staging,
        production
    }
}