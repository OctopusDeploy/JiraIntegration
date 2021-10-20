using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Octopus.Server.Extensibility.Extensions.Model.Environments;
using Octopus.Server.Extensibility.JiraIntegration.Configuration;
using Octopus.Server.Extensibility.Metadata;
using Octopus.Server.MessageContracts.Attributes;

namespace Octopus.Server.Extensibility.JiraIntegration.Environments
{
    internal class DeploymentEnvironmentSettingsMetadataProvider : IContributeDeploymentEnvironmentSettingsMetadata
    {
        private readonly IJiraConfigurationStore store;

        public DeploymentEnvironmentSettingsMetadataProvider(IJiraConfigurationStore store)
        {
            this.store = store;
        }

        public string ExtensionId => JiraConfigurationStore.SingletonId;
        public string ExtensionName => JiraIntegration.Name;

        public async IAsyncEnumerable<PropertyMetadata> Properties(
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var enabled = await store.GetIsEnabled(cancellationToken) &&
                          await store.GetJiraInstanceType(cancellationToken) == JiraInstanceType.Cloud;
            if (!enabled) yield break;

            foreach (var propertyMetadata in new MetadataGenerator().GetMetadata<JiraDeploymentEnvironmentSettings>()
                .Types.First().Properties)
                yield return propertyMetadata;
        }

        internal class JiraDeploymentEnvironmentSettings
        {
            [DisplayName("Jira Environment Type")]
            [Description("The Jira environment type of this Octopus deployment environment.")]
            [ReadOnly(false)]
            [HasOptions(SelectMode.Single)]
            public JiraEnvironmentType JiraEnvironmentType { get; set; }
        }
    }

    internal enum JiraEnvironmentType
    {
        unmapped,
        development,
        testing,
        staging,
        production
    }
}