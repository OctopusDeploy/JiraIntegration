using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Octopus.Diagnostics;
using Octopus.Server.Extensibility.Domain.Deployments;
using Octopus.Server.Extensibility.Extensions.Domain;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Web.Api;
using Octopus.Server.Extensibility.HostServices.Configuration;
using Octopus.Server.Extensibility.HostServices.Domain.Environments;
using Octopus.Server.Extensibility.HostServices.Domain.Projects;
using Octopus.Server.Extensibility.HostServices.Domain.ServerTasks;
using Octopus.Server.Extensibility.HostServices.Licensing;
using Octopus.Server.Extensibility.HostServices.Model.Environments;
using Octopus.Server.Extensibility.JiraIntegration.Configuration;
using Octopus.Server.Extensibility.JiraIntegration.Integration;
using Octopus.Time;

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
            try
            {
                jiraDeployment.PublishToJira(StateFromEventType(domainEvent.EventType), domainEvent.Deployment, new JiraIssueTrackerApiDeployment());
            }
            catch (JiraDeploymentException e) { }
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