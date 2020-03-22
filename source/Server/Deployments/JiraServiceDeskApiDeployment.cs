using Octopus.Server.Extensibility.HostServices.Model.Projects;

namespace Octopus.Server.Extensibility.JiraIntegration.Deployments
{
    public class JiraServiceDeskApiDeployment : IJiraApiDeployment
    {
        public string DeploymentType()
        {
            return JiraAssociationConstants.JiraAssociationTypeServiceIdOrKeys;
        }

        public string[] DeploymentValues(IDeployment deployment)
        {
            return new[] {""};
        }
    }
}