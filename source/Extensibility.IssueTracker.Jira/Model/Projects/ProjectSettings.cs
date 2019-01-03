using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Octopus.Server.Extensibility.Extensions.Model;
using Octopus.Server.Extensibility.HostServices.Model.Projects;
using Octopus.Server.Extensibility.IssueTracker.Jira.Configuration;
using Octopus.Server.Extensibility.Metadata;

namespace Octopus.Server.Extensibility.IssueTracker.Jira.Model.Projects
{
    public class ProjectSettings : IContributeProjectSettingsMetadata
    {
        private readonly IJiraConfigurationStore configurationStore;

        public ProjectSettings(IJiraConfigurationStore configurationStore)
        {
            this.configurationStore = configurationStore;
        }

        public string ExtensionId => JiraConfigurationStore.SingletonId;
        public string ExtensionName => JiraIssueTracker.Name;

        public List<PropertyMetadata> Properties => configurationStore.GetIsEnabled() ? new MetadataGenerator().GetMetadata<JiraProjectSettings>().Types.First().Properties : null;

        public class JiraProjectSettings
        {
            [DisplayName("Work Item Package")]
            [Description("Include the work items from an included package")]
            [ReadOnly(false)]
            public DeploymentActionPackage WorkItemPackage { get; set; }
        }
    }
}