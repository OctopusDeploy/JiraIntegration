#nullable enable
using Octopus.Server.Extensibility.HostServices.Diagnostics;
using Octopus.Server.Extensibility.HostServices.Domain.Projects;
using Octopus.Server.Extensibility.JiraIntegration.Deployments;
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

        readonly JiraDeployment jiraDeployment;
        private readonly IDeploymentStore deploymentStore;

        public JiraServiceDeskActionHandler(
            IDeploymentStore deploymentStore,
            JiraDeployment jiraDeployment)
        {
            this.jiraDeployment = jiraDeployment;
            this.deploymentStore = deploymentStore;
        }

        public IActionHandlerResult Execute(IActionHandlerContext context, ITaskLog taskLog)
        {
            var deploymentId = context.Variables.Get(KnownVariables.Deployment.Id, "");
            var deployment = deploymentStore.Get(deploymentId);

            var jiraServiceDeskChangeRequestId = context.Variables.Get("Octopus.Action.JiraIntegration.ServiceDesk.ServiceId");
            if (string.IsNullOrWhiteSpace(jiraServiceDeskChangeRequestId))
                throw new ControlledActionFailedException("ServiceId is not set");

            try
            {
                jiraDeployment.PublishToJira("in_progress", deployment, new JiraServiceDeskApiDeployment(jiraServiceDeskChangeRequestId), taskLog);
            }
            catch (JiraDeploymentException exception)
            {
                throw new ControlledActionFailedException(exception.Message);
            }

            return ActionHandlerResult.FromSuccess();
        }

    }
}