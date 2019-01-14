using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Octopus.Server.Extensibility.Extensions.Model.Projects;
using Octopus.Server.Extensibility.HostServices.Model.Projects;
using Octopus.Server.Extensibility.IssueTracker.Extensions;
using Octopus.Server.Extensibility.IssueTracker.Jira.Configuration;
using Octopus.Server.Extensibility.Metadata;
using Octopus.Server.Extensibility.Resources;

namespace Octopus.Server.Extensibility.IssueTracker.Jira.Model.Projects
{
    public class ProjectSettings : IContributeProjectSettingsMetadata, IContributeWorkItemsToReleases
    {
        private readonly IJiraConfigurationStore configurationStore;
        private readonly IProvideProjectSettingsValues projectSettings;

        public ProjectSettings(IJiraConfigurationStore configurationStore, IProvideProjectSettingsValues projectSettings)
        {
            this.configurationStore = configurationStore;
            this.projectSettings = projectSettings;
        }

        public string ExtensionId => JiraConfigurationStore.SingletonId;
        public string ExtensionName => JiraIssueTracker.Name;

        public List<PropertyMetadata> Properties => configurationStore.GetIsEnabled() ? new MetadataGenerator().GetMetadata<JiraProjectSettings>().Types.First().Properties : null;

        internal class JiraProjectSettings
        {
            [DisplayName("Work Item Package")]
            [Description("Include the work items from an included package")]
            [ReadOnly(false)]
            public DeploymentActionPackageResource WorkItemPackage { get; set; }
        }

        public DeploymentActionPackageResource GetDeploymentAction(string projectId)
        {
            if (!configurationStore.GetIsEnabled())
                return null;
            var settings = projectSettings.GetSettings<JiraProjectSettings>(ExtensionId, projectId);
            return settings?.WorkItemPackage;
        }
    }
}