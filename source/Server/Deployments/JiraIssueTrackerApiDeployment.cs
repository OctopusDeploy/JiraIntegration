using System.Linq;
using Octopus.Diagnostics;
using Octopus.Server.Extensibility.HostServices.Model.Projects;
using Octopus.Server.Extensibility.JiraIntegration.Configuration;

namespace Octopus.Server.Extensibility.JiraIntegration.Deployments
{
    class JiraIssueTrackerApiDeployment : IJiraApiDeployment
    {
        public string DeploymentType => JiraAssociationConstants.JiraAssociationTypeIssueKeys;

        public string[] DeploymentValues(IDeployment deployment)
        {
            return deployment.Changes.SelectMany(drn => drn.VersionBuildInformation
                .SelectMany(pm => pm.WorkItems)
                .Where(wi => wi.Source == JiraConfigurationStore.CommentParser)
                .Select(wi => wi.Id)
                .Distinct()).ToArray();
        }

        public void HandleJiraIntegrationIsUnavailable() { }
    }
}