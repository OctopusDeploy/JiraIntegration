using Octopus.Server.MessageContracts.Features.Projects.Releases.Deployments;

namespace Octopus.Server.Extensibility.JiraIntegration.Deployments
{
    internal interface IJiraApiDeployment
    {
        public string DeploymentType { get; }
        public string[] DeploymentValues(DeploymentResource deployment);
        public void HandleJiraIntegrationIsUnavailable();
    }
}