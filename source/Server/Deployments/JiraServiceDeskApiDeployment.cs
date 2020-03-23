using Octopus.Server.Extensibility.HostServices.Model.Projects;

namespace Octopus.Server.Extensibility.JiraIntegration.Deployments
{
    public class JiraServiceDeskApiDeployment : IJiraApiDeployment
    {
        readonly string jiraServiceDeskChangeRequestId;

        public JiraServiceDeskApiDeployment(string jiraServiceDeskChangeRequestId)
        {
            this.jiraServiceDeskChangeRequestId = jiraServiceDeskChangeRequestId;
        }
        
        public string DeploymentType()
        {
            return JiraAssociationConstants.JiraAssociationTypeServiceIdOrKeys;
        }

        public string[] DeploymentValues(IDeployment deployment)
        {
            return new[] { jiraServiceDeskChangeRequestId };
        }
    }
}