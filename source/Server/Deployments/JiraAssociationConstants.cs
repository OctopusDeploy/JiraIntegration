namespace Octopus.Server.Extensibility.JiraIntegration.Deployments
{
    static class JiraAssociationConstants
    {
        public static string JiraAssociationTypeIssueIdOrKeys = "issueIdOrKeys";
        public static string JiraAssociationTypeServiceIdOrKeys = "serviceIdOrKeys";

        public static readonly string[] ValidJiraAssociationTypes = {
            JiraAssociationTypeIssueIdOrKeys,
            JiraAssociationTypeServiceIdOrKeys
        };
    }
}