using System;
using FluentValidation.Validators;

namespace Octopus.Server.Extensibility.JiraIntegration.Deployments
{
    class OctopusJiraPayloadData
    {
        public string InstallationId { get; set; }
        public string BaseHostUrl { get; set; }
        public JiraPayloadData DeploymentsInfo { get; set; }
    }
}