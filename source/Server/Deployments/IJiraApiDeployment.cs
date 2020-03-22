using Octopus.Server.Extensibility.HostServices.Model.Projects;

namespace Octopus.Server.Extensibility.JiraIntegration.Deployments
{
    public interface IJiraApiDeployment
    {
        public string DeploymentType();
        public string[] DeploymentValues(IDeployment deployment);
    }
}