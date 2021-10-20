using System.Linq;
using System.Text.RegularExpressions;
using Octopus.Server.MessageContracts.Features.BuildInformation;

namespace Octopus.Server.Extensibility.JiraIntegration.WorkItems
{
    internal class CommentParser
    {
        // Expression based on example found here https://confluence.atlassian.com/stashkb/integrating-with-custom-jira-issue-key-313460921.html?_ga=2.163394108.1696841245.1556699049-1954949426.1532303954 with modified negative lookbehind
        // with added '$' and '.' to exclude strings that look similar to Jira issues, e.g. `text that may cause confusion: $foo-1 or test.TST-01.com`
        private static readonly Regex Expression = new("(?<![A-Z0-9\\$\\.]-?)(?>[A-Z0-9]+-\\d+)(?![A-Z])",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public string[] ParseWorkItemIds(OctopusBuildInformation buildInformation)
        {
            return buildInformation.Commits.SelectMany(c => WorkItemIds(c.Comment))
                .Where(workItemId => !string.IsNullOrWhiteSpace(workItemId))
                .ToArray();
        }

        private string[] WorkItemIds(string comment)
        {
            return Expression.Matches(comment)
                .Select(m => m.Groups[0].Value)
                .ToArray();
        }
    }
}