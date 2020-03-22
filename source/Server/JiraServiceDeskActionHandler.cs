using System.Collections.Generic;
using Octopus.Data.Storage.Configuration;
using Octopus.Diagnostics;
using Octopus.Server.Extensibility.HostServices.Domain.Projects;
using Octopus.Server.Extensibility.HostServices.Model.Projects;
using Octopus.Server.Extensibility.JiraIntegration.Deployments;
using Sashimi.Server.Contracts.ActionHandlers;

namespace Octopus.Server.Extensibility.JiraIntegration
{
    class JiraServiceDeskActionHandler : IActionHandler
    {
        public string Id => "Octopus.JiraIntegration.ServiceDeskAction";
        public string Name => "Log to Jira Service Desk";
        public string Description => "Create a log in Jira Service desk";
        public string? Keywords => null;
        public bool ShowInStepTemplatePickerUI => true;
        public bool WhenInAChildStepRunInTheContextOfTheTargetMachine => false;
        public bool CanRunOnDeploymentTarget => false;
        public ActionHandlerCategory[] Categories => new[] {ActionHandlerCategory.BuiltInStep};

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
        
        public IActionHandlerResult Execute(IActionHanderContext context)
        {
            string deploymentId = context.Variables.Get("Octopus.Deployment.Id", "");
            IDeployment deployment = deploymentStore.Get(deploymentId);

            jiraDeployment.PublishToJira("in_progress", deployment, new JiraServiceDeskApiDeployment());

            return context.RawShellCommand().Execute();
        }
        
    }
}