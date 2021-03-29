#nullable enable
using System.Threading;
using Octopus.Server.Extensibility.HostServices.Diagnostics;
using Octopus.Server.Extensibility.JiraIntegration.Deployments;
using Octopus.Server.Extensibility.Mediator;
using Octopus.Server.MessageContracts.Projects.Releases.Deployments;
using Sashimi.Server.Contracts;
using Sashimi.Server.Contracts.ActionHandlers;

namespace Octopus.Server.Extensibility.JiraIntegration.Actions
{
    class JiraServiceDeskActionHandler : IActionHandler
    {
        public string Id => "Octopus.JiraIntegration.ServiceDeskAction";
        public string Name => "Jira Service Desk Change Request";
        public string Description => "Initiate a Change Request in Jira Service Desk";
        public string? Keywords => null;
        public bool ShowInStepTemplatePickerUI => true;
        public bool WhenInAChildStepRunInTheContextOfTheTargetMachine => false;
        public bool CanRunOnDeploymentTarget => false;
        public ActionHandlerCategory[] Categories => new[] { ActionHandlerCategory.BuiltInStep, ActionHandlerCategory.Atlassian };

        private readonly IMediator mediator;
        readonly JiraDeployment jiraDeployment;

        public JiraServiceDeskActionHandler(
            IMediator mediator,
            JiraDeployment jiraDeployment)
        {
            this.mediator = mediator;
            this.jiraDeployment = jiraDeployment;
        }

        public IActionHandlerResult Execute(IActionHandlerContext context, ITaskLog taskLog)
        {
            var deploymentId = context.Variables.Get(KnownVariables.Deployment.Id, "");
            var deployment = mediator.Request(new GetDeploymentRequest(deploymentId.ToDeploymentId()), CancellationToken.None).GetAwaiter().GetResult().Deployment;

            var jiraServiceDeskChangeRequestId = context.Variables.Get("Octopus.Action.JiraIntegration.ServiceDesk.ServiceId");
            if (string.IsNullOrWhiteSpace(jiraServiceDeskChangeRequestId))
                throw new ControlledActionFailedException("ServiceId is not set");

            try
            {
                jiraDeployment.PublishToJira("in_progress", deployment, new JiraServiceDeskApiDeployment(jiraServiceDeskChangeRequestId), taskLog, CancellationToken.None).GetAwaiter().GetResult();
            }
            catch (JiraDeploymentException exception)
            {
                throw new ControlledActionFailedException(exception.Message);
            }

            return ActionHandlerResult.FromSuccess();
        }

    }
}