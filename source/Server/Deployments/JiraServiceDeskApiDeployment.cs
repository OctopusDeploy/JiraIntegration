using System;
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

        public string DeploymentType => JiraAssociationConstants.JiraAssociationTypeServiceIdOrKeys;

        public string[] DeploymentValues(IDeployment deployment)
        {
            if (String.IsNullOrEmpty(jiraServiceDeskChangeRequestId))
            {
                throw new JiraDeploymentException("Service ID for empty. Please supply a Jira Service Desk Service ID and try again.");
            }
            return new[] { jiraServiceDeskChangeRequestId };
        }

        public void JiraIntegrationDisabled()
        {
            throw new JiraDeploymentException($"Trying to use Jira Service Desk Change Request step without having " +
                                              $"Jira Integration enabled. Please enable Jira Integration or disable the Jira " +
                                              $"Service Desk Change Request step.");
        }
    }
}