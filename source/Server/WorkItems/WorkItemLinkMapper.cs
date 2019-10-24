using System;
using System.Linq;
using System.Text.RegularExpressions;
using Octopus.Diagnostics;
using Octopus.Server.Extensibility.Extensions;
using Octopus.Server.Extensibility.Extensions.WorkItems;
using Octopus.Server.Extensibility.HostServices.Model.BuildInformation;
using Octopus.Server.Extensibility.IssueTracker.Jira.Configuration;
using Octopus.Server.Extensibility.IssueTracker.Jira.Integration;
using Octopus.Server.Extensibility.Resources.IssueTrackers;
using Octopus.Versioning;

namespace Octopus.Server.Extensibility.IssueTracker.Jira.WorkItems
{
    public class WorkItemLinkMapper : IWorkItemLinkMapper
    {
        private readonly IJiraConfigurationStore store;
        private readonly CommentParser commentParser;
        private readonly Lazy<IJiraRestClient> jira;

        public WorkItemLinkMapper(IJiraConfigurationStore store,
            CommentParser commentParser,
            Lazy<IJiraRestClient> jira)
        {
            this.store = store;
            this.commentParser = commentParser;
            this.jira = jira;
        }

        public string CommentParser => JiraConfigurationStore.CommentParser;
        public bool IsEnabled => store.GetIsEnabled();

        public SuccessOrErrorResult<WorkItemLink[]> Map(string packageId, IVersion version, OctopusBuildInformation buildInformation, ILogWithContext log)
        {
            if (!IsEnabled || 
                string.IsNullOrEmpty(store.GetJiraUsername()) || 
                string.IsNullOrEmpty(store.GetJiraPassword()))
                return null;

            var baseUrl = store.GetBaseUrl();
            if (string.IsNullOrWhiteSpace(baseUrl))
                return null;

            var releaseNotePrefix = store.GetReleaseNotePrefix();
            var workItemIds = commentParser.ParseWorkItemIds(buildInformation).Distinct();

            return workItemIds.Select(workItemId =>
            {
                var issue = jira.Value.GetIssue(workItemId).GetAwaiter().GetResult();
                if (issue is null) return null;
                
                return new WorkItemLink
                {
                    Id = workItemId,
                    Description = GetReleaseNote(issue, releaseNotePrefix),
                    LinkUrl = baseUrl + "/browse/" + workItemId,
                    Source = JiraConfigurationStore.CommentParser
                };
            })
            .Where(i => i != null)
            .ToArray();
        }

        public string GetReleaseNote(JiraIssue issue, string releaseNotePrefix)
        {
            if (issue.Fields.Comments.Total == 0 || string.IsNullOrWhiteSpace(releaseNotePrefix))
                return issue.Fields.Summary;

            var releaseNoteRegex = new Regex($"^{releaseNotePrefix}", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var issueComments = jira.Value.GetIssueComments(issue.Key).GetAwaiter().GetResult();

            var releaseNote = issueComments?.Comments.LastOrDefault(c => releaseNoteRegex.IsMatch(c.Body))?.Body;
            return !string.IsNullOrWhiteSpace(releaseNote)
                ? releaseNoteRegex.Replace(releaseNote, "")?.Trim()
                : issue.Fields.Summary ?? issue.Key;
        }
    }
}