namespace Octopus.Server.Extensibility.IssueTracker.Jira.Deployments
{
    public class OctopusJiraPayloadData
    {
        public string InstallationId { get; set; }
        public string BaseHostUrl { get; set; }
        public JiraPayloadData DeploymentsInfo { get; set; }
    }
}