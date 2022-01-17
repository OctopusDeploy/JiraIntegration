using System;
using System.Linq;
using Octopus.Server.Extensibility.JiraIntegration.Configuration;
using Octopus.Server.MessageContracts.Features.Projects.Releases.Deployments;

namespace Octopus.Server.Extensibility.JiraIntegration.Deployments
{
    class JiraIssueTrackerApiDeployment : IJiraApiDeployment
    {
        public string DeploymentType => JiraAssociationConstants.JiraAssociationTypeIssueKeys;

        public string[] DeploymentValues(DeploymentResource deployment)
        {
            return deployment.Changes.SelectMany(drn => drn.BuildInformation
                                                           .SelectMany(pm => pm.WorkItems)
                                                           .Where(wi => wi.Source == JiraConfigurationStore.CommentParser)
                                                           .Select(wi => wi.Id)
                                                           .Distinct())
                             .ToArray();
        }

        public void HandleJiraIntegrationIsUnavailable()
        {
        }
    }
}