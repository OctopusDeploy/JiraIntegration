using System.Linq;
using System.Text.RegularExpressions;
using Octopus.Server.Extensibility.HostServices.Model.PackageMetadata;

namespace Octopus.Server.Extensibility.IssueTracker.Jira.WorkItems
{
    public class CommentParser
    {
        // Expression based on example found here https://confluence.atlassian.com/stashkb/integrating-with-custom-jira-issue-key-313460921.html?_ga=2.163394108.1696841245.1556699049-1954949426.1532303954
        private static readonly Regex Expression = new Regex("((?<!([A-Z0-9.]{1,10})-?)[A-Z0-9]+-\\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        
        public string[] ParseWorkItemIds(OctopusPackageMetadata packageMetadata)
        {
            return packageMetadata.Commits.SelectMany(c => WorkItemIds(c.Comment))
                .Where(workItemId => !string.IsNullOrWhiteSpace(workItemId))
                .ToArray();
        }

        private string[] WorkItemIds(string comment)
        {
            return Expression.Matches(comment)
                .Cast<Match>()
                .Select(m => m.Groups[0].Value)
                .ToArray();
        }
    }
}