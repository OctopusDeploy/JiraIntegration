using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Octopus.Server.Extensibility.HostServices.Diagnostics;
using Octopus.Server.Extensibility.JiraIntegration.Configuration;
using Octopus.Server.Extensibility.Mediator;
using Octopus.Server.MessageContracts.Features.Projects.Releases.Deployments;

namespace Octopus.Server.Extensibility.JiraIntegration.Deployments
{
    class DeploymentObserver : IHandleEvent<DeploymentEvent>
    {
        private JiraDeployment jiraDeployment;
        private readonly IMediator mediator;
        private readonly ITaskLogFactory taskLogFactory;

        public DeploymentObserver(JiraDeployment jiraDeployment,
            IMediator mediator,
            ITaskLogFactory taskLogFactory)
        {
            this.jiraDeployment = jiraDeployment;
            this.mediator = mediator;
            this.taskLogFactory = taskLogFactory;
        }

        public async Task Handle(DeploymentEvent domainEvent, CancellationToken cancellationToken)
        {
            var deployment = (await mediator.Request(new GetDeploymentRequest(domainEvent.DeploymentId), cancellationToken)).Deployment;
            if (deployment.Changes.All(drn =>
                drn.BuildInformation.All(pm =>
                    pm.WorkItems.All(wi => wi.Source != JiraConfigurationStore.CommentParser))))
                return;

            var taskLog = taskLogFactory.Get(domainEvent.TaskLogCorrelationId);

            await jiraDeployment.PublishToJira(StateFromEventType(domainEvent.EventType), deployment, new JiraIssueTrackerApiDeployment(), taskLog, cancellationToken);
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