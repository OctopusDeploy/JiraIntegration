using Octopus.Server.MessageContracts.Projects.Releases.Deployments;

namespace Octopus.Server.Extensibility.JiraIntegration.Deployments
{
    interface IJiraApiDeployment
    {
        public string DeploymentType { get; }
        public string[] DeploymentValues(DeploymentResource deployment);
        public void HandleJiraIntegrationIsUnavailable();
    }
}