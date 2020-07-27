using System.Linq;
using Octopus.Server.Extensibility.Domain.Deployments;
using Octopus.Server.Extensibility.Extensions.Domain;
using Octopus.Server.Extensibility.JiraIntegration.Configuration;

namespace Octopus.Server.Extensibility.JiraIntegration.Deployments
{
    class DeploymentObserver : IObserveDomainEvent<DeploymentEvent>
    {
        private JiraDeployment jiraDeployment;

        public DeploymentObserver(JiraDeployment jiraDeployment)
        {
            this.jiraDeployment = jiraDeployment;
        }

        public void Handle(DeploymentEvent domainEvent)
        {
            if (domainEvent.Deployment.Changes.All(drn =>
                drn.VersionBuildInformation.All(pm =>
                    pm.WorkItems.All(wi => wi.Source != JiraConfigurationStore.CommentParser))))
                return;

            jiraDeployment.PublishToJira(StateFromEventType(domainEvent.EventType), domainEvent.Deployment, new JiraIssueTrackerApiDeployment());
        }

        string StateFromEventType(DeploymentEventType eventType)
        {
            switch (eventType)
            {
                case DeploymentEventType.DeploymentStarted:
                    return "in_progress";
                case DeploymentEventType.DeploymentFailed:
                    return "failed";
                case DeploymentEventType.DeploymentSucceeded:
                    return "successful";
                default:
                    return "unknown";
            }
        }

    }
}