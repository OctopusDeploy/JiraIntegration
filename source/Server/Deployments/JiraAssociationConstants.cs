namespace Octopus.Server.Extensibility.JiraIntegration.Deployments
{
    internal static class JiraAssociationConstants
    {
        public static string JiraAssociationTypeIssueKeys = "issueKeys";
        public static string JiraAssociationTypeServiceIdOrKeys = "serviceIdOrKeys";

        public static readonly string[] ValidJiraAssociationTypes =
        {
            JiraAssociationTypeIssueKeys,
            JiraAssociationTypeServiceIdOrKeys
        };
    }
}