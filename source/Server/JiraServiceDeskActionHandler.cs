using System.Collections.Generic;
using Octopus.Data.Storage.Configuration;
using Octopus.Diagnostics;
using Octopus.Server.Extensibility.HostServices.Model.Projects;
using Octopus.Server.Extensibility.JiraIntegration.Deployments;
using Sashimi.Server.Contracts.ActionHandlers;

namespace Octopus.Server.Extensibility.JiraIntegration
{
    class JiraServiceDeskActionHandler : IActionHandler
    {
        class JiraServiceDeskActionHandlerResult : IActionHandlerResult
        {
            public IReadOnlyDictionary<string, OutputVariable> OutputVariables { get; }
            public IReadOnlyList<ScriptOutputAction> OutputActions { get; }
            public IReadOnlyList<ServiceMessage> ServiceMessages { get; }
            public ExecutionOutcome Outcome { get; }
            public bool WasSuccessful { get; }
            public string ResultMessage { get; }
            public int ExitCode { get; }
        }
        
        public string Id => "Octopus.JiraIntegration.ServiceDeskAction";
        public string Name => "Log to Jira Service Desk";
        public string Description => "Create a log in Jira Service desk";
        public string? Keywords => null;
        public bool ShowInStepTemplatePickerUI => true;
        public bool WhenInAChildStepRunInTheContextOfTheTargetMachine => false;
        public bool CanRunOnDeploymentTarget => false;
        public ActionHandlerCategory[] Categories => new[] {ActionHandlerCategory.BuiltInStep};

        readonly JiraDeployment JiraDeployment;
        readonly ILog Log;
        readonly IConfigurationStore ConfigurationStore;
        
        public JiraServiceDeskActionHandler(ILog log, IConfigurationStore configurationStore, JiraDeployment jiraDeployment)
        {
            Log = log;
            ConfigurationStore = configurationStore;
            JiraDeployment = jiraDeployment;
        }
        
        public IActionHandlerResult Execute(IActionHanderContext context)
        {
            IDeployment deployment = 
                ConfigurationStore.Get<IDeployment>(context.Variables.Get("Octopus.Deployment.Id", ""));

            JiraDeployment.PublishToJira("in_progress", deployment);

            return new JiraServiceDeskActionHandlerResult
            {
                OutputActions = { },
                OutputVariables = { },
                ServiceMessages = { }
            };
        }
        
    }
}