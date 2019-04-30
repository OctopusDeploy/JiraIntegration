using System.Linq;
using System.Text.RegularExpressions;
using Octopus.Server.Extensibility.HostServices.Model.PackageMetadata;

namespace Octopus.Server.Extensibility.IssueTracker.Jira.WorkItems
{
    public class CommentParser
    {
        private static readonly Regex Expression = new Regex("[A-Z0-9]+-\\d+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        
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