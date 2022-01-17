using System;
using Octopus.Server.MessageContracts.Features.Projects.Releases.Deployments;

namespace Octopus.Server.Extensibility.JiraIntegration.Deployments
{
    class JiraServiceDeskApiDeployment : IJiraApiDeployment
    {
        readonly string jiraServiceId;

        public JiraServiceDeskApiDeployment(string jiraServiceId)
        {
            this.jiraServiceId = jiraServiceId;
        }

        public string DeploymentType => JiraAssociationConstants.JiraAssociationTypeServiceIdOrKeys;

        public string[] DeploymentValues(DeploymentResource deployment)
        {
            if (string.IsNullOrEmpty(jiraServiceId))
                throw new JiraDeploymentException("Service ID is empty. Please supply a Jira Service Desk Service ID and try again");
            return new[] { jiraServiceId };
        }

        public void HandleJiraIntegrationIsUnavailable()
        {
            throw new JiraDeploymentException("Trying to use Jira Service Desk Change Request step without having " + "Jira Integration enabled. Please enable Jira Integration or disable the Jira " + "Service Desk Change Request step");
        }
    }
}