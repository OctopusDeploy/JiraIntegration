using System.Linq;
using System.Text.RegularExpressions;
using Octopus.Server.Extensibility.HostServices.Model.PackageMetadata;

namespace Octopus.Server.Extensibility.IssueTracker.Jira.WorkItems
{
    public class CommentParser
    {
        // Expression based on example found here https://confluence.atlassian.com/stashkb/integrating-with-custom-jira-issue-key-313460921.html?_ga=2.163394108.1696841245.1556699049-1954949426.1532303954 with modified negative lookbehind
        // Explanation:
        // Negative Lookbehind (?<!([\041-\053\056-\176]{1,10})-?)
        // Assert that the Regex below does not match
        //  2nd Capturing Group ([\041-\053\056-\176]{1,10})
        //   Match a single character present in the list below [\041-\053\056-\176]{1,10}
        //   {1,10} Quantifier — Matches between 1 and 10 times, as many times as possible, giving back as needed (greedy)
        //   \041-\053 a single character in the range between ! (index 33) and + (index 43) (case insensitive)
        //   \056-\176 a single character in the range between . (index 46) and ~ (index 126) (case insensitive)
        //  -? matches the character - literally (case insensitive)
        //   ? Quantifier — Matches between zero and one times, as many times as possible, giving back as needed (greedy)
        private static readonly Regex Expression = new Regex("((?<!([\\041-\\053\\056-\\176]{1,10})-?)[A-Z0-9]+-\\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        
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