using Octopus.Server.Extensibility.HostServices.Model.Projects;

namespace Octopus.Server.Extensibility.JiraIntegration.Deployments
{
    public interface IJiraApiDeployment
    {
        public string DeploymentType { get; }
        public string[] DeploymentValues(IDeployment deployment);
        public void JiraIntegrationDisabled();
    }
}