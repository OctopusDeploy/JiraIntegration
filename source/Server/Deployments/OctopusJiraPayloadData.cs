namespace Octopus.Server.Extensibility.JiraIntegration.Deployments
{
    internal class OctopusJiraPayloadData
    {
        public string InstallationId { get; set; } = string.Empty;
        public string BaseHostUrl { get; set; } = string.Empty;
        public JiraPayloadData DeploymentsInfo { get; set; } = new();
    }
}