#nullable enable
using FluentValidation;
using Octopus.Diagnostics;
using Octopus.Server.Extensibility.HostServices.Domain.Projects;
using Octopus.Server.Extensibility.HostServices.Model.Projects;
using Octopus.Server.Extensibility.JiraIntegration.Deployments;
using Sashimi.Server.Contracts;
using Sashimi.Server.Contracts.ActionHandlers;
using Sashimi.Server.Contracts.ActionHandlers.Validation;

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
        public IValidator<DeploymentActionValidationContext>? Validator { get; } = null;

        readonly JiraDeployment jiraDeployment;
        readonly ILog log;
        private readonly IDeploymentStore deploymentStore;
        
        public JiraServiceDeskActionHandler(
            ILog log,
            IDeploymentStore deploymentStore,
            JiraDeployment jiraDeployment)
        {
            this.log = log;
            this.jiraDeployment = jiraDeployment;
            this.deploymentStore = deploymentStore;
        }
        
        public IActionHandlerResult Execute(IActionHandlerContext context)
        {
            string deploymentId = context.Variables.Get(KnownVariables.Deployment.Id, "");
            IDeployment deployment = deploymentStore.Get(deploymentId);
            
            string jiraServiceDeskChangeRequestId = context.Variables.Get("Octopus.Action.JiraIntegration.ServiceDesk.ServiceId");
            
            try
            {
                jiraDeployment.PublishToJira("in_progress", deployment, new JiraServiceDeskApiDeployment(jiraServiceDeskChangeRequestId));
            }
            catch (JiraDeploymentException exception)
            {
                throw new ControlledActionFailedException(exception.Message);
            }
            
            return ActionHandlerResult.FromSuccess();
        }
        
    }
}