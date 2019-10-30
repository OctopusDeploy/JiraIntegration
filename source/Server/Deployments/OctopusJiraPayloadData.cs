namespace Octopus.Server.Extensibility.IssueTracker.Jira.Deployments
{
    class OctopusJiraPayloadData
    {
        public string InstallationId { get; set; }
        public string BaseHostUrl { get; set; }
        public JiraPayloadData DeploymentsInfo { get; set; }
    }
}