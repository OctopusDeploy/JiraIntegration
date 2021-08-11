using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Octopus.Server.Extensibility.Extensions.Model.Environments;
using Octopus.Server.Extensibility.JiraIntegration.Configuration;
using Octopus.Server.Extensibility.Metadata;
using Octopus.Server.MessageContracts.Attributes;

namespace Octopus.Server.Extensibility.JiraIntegration.Environments
{
    class DeploymentEnvironmentSettingsMetadataProvider : IContributeDeploymentEnvironmentSettingsMetadata
    {
        private readonly IJiraConfigurationStore store;

        public DeploymentEnvironmentSettingsMetadataProvider(IJiraConfigurationStore store)
        {
            this.store = store;
        }

        public string ExtensionId => JiraConfigurationStore.SingletonId;
        public string ExtensionName => JiraIntegration.Name;

        public Type Resource => typeof(JiraDeploymentEnvironmentResource);

        public Type Model => typeof(JiraDeploymentEnvironmentMapping);

        public List<PropertyMetadata> Properties => store.GetIsEnabled() && store.GetJiraInstanceType() == JiraInstanceType.Cloud
            ? new MetadataGenerator().GetMetadata<JiraDeploymentEnvironmentResource>().Types.First().Properties
            : new List<PropertyMetadata>();

        internal class JiraDeploymentEnvironmentResource
        {
            [DisplayName("Jira Environment Type")]
            [Description("The Jira environment type of this Octopus deployment environment.")]
            [ReadOnly(false)]
            [HasOptions(SelectMode.Single)]
            public JiraEnvironmentType JiraEnvironmentType { get; set; }
        }

        private class JiraDeploymentEnvironmentMapping
        {
            public JiraEnvironmentType JiraEnvironmentType { get; set; }
        }
    }

    enum JiraEnvironmentType
    {
        unmapped,
        development,
        testing,
        staging,
        production
    }
}