using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Octopus.Diagnostics;
using Octopus.Server.Extensibility.Extensions.WorkItems;
using Octopus.Server.Extensibility.JiraIntegration.Configuration;
using Octopus.Server.Extensibility.JiraIntegration.Integration;
using Octopus.Server.Extensibility.Results;
using Octopus.Server.MessageContracts.Features.BuildInformation;
using Octopus.Server.MessageContracts.Features.IssueTrackers;

namespace Octopus.Server.Extensibility.JiraIntegration.WorkItems
{
    class WorkItemLinkMapper : IWorkItemLinkMapper
    {
        private readonly IJiraConfigurationStore store;
        private readonly CommentParser commentParser;
        private readonly Lazy<IJiraRestClient> jira;
        private readonly ISystemLog systemLog;

        public WorkItemLinkMapper(IJiraConfigurationStore store,
            CommentParser commentParser,
            Lazy<IJiraRestClient> jira,
            ISystemLog systemLog)
        {
            this.store = store;
            this.commentParser = commentParser;
            this.jira = jira;
            this.systemLog = systemLog;
        }

        public string CommentParser => JiraConfigurationStore.CommentParser;
        public bool IsEnabled => store.GetIsEnabled();

        public IResultFromExtension<WorkItemLink[]> Map(OctopusBuildInformation buildInformation)
        {
            if (!IsEnabled)
                return ResultFromExtension<WorkItemLink[]>.ExtensionDisabled();
            if (string.IsNullOrEmpty(store.GetJiraUsername()) ||
                string.IsNullOrEmpty(store.GetJiraPassword()?.Value))
                return ResultFromExtension<WorkItemLink[]>.Failed("Username/password not configured");

            var baseUrl = store.GetBaseUrl();
            if (string.IsNullOrWhiteSpace(baseUrl))
                return ResultFromExtension<WorkItemLink[]>.Failed("No BaseUrl configured");

            var releaseNotePrefix = store.GetReleaseNotePrefix();
            var workItemIds = commentParser.ParseWorkItemIds(buildInformation).Distinct().ToArray();
            if (workItemIds.Length == 0)
                return ResultFromExtension<WorkItemLink[]>.Success(new WorkItemLink[0]);

            return ResultFromExtension<WorkItemLink[]>.Success(ConvertWorkItemLinks(workItemIds, releaseNotePrefix, baseUrl));
        }

        private WorkItemLink[] ConvertWorkItemLinks(string[] workItemIds, string? releaseNotePrefix, string baseUrl)
        {
            var issues = jira.Value.GetIssues(workItemIds.ToArray()).GetAwaiter().GetResult();
            if (issues == null || issues.Issues.Length == 0)
            {
                return new WorkItemLink[0];
            }

            var issueMap = issues.Issues.ToDictionary(x => x.Key);

            var workItemsNotFound = workItemIds.Where(x => !issueMap.ContainsKey(x)).ToArray();
            if (workItemsNotFound.Length > 0)
            {
                systemLog.Warn($"Parsed work item ids {string.Join(", ", workItemsNotFound)} from commit messages but could not locate them in Jira");
            }

            return workItemIds
                .Where(workItemId => issueMap.ContainsKey(workItemId))
                .Select(workItemId =>
                {
                    var issue = issueMap[workItemId];
                    return new WorkItemLink
                    {
                        Id = workItemId,
                        Description = GetReleaseNote(issueMap[workItemId], releaseNotePrefix),
                        LinkUrl = baseUrl + "/browse/" + workItemId,
                        Source = JiraConfigurationStore.CommentParser
                    };
                })
                .Where(i => i != null)
                // ReSharper disable once RedundantEnumerableCastCall
                .Cast<WorkItemLink>() // cast back from `WorkItemLink?` type to keep the compiler happy
                .ToArray();
        }

        public string GetReleaseNote(JiraIssue issue, string? releaseNotePrefix)
        {
            if (issue.Fields.Comments.Total == 0 || string.IsNullOrWhiteSpace(releaseNotePrefix))
                return issue.Fields.Summary;

            var releaseNoteRegex = new Regex($"^{releaseNotePrefix}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            var releaseNote = issue.Fields.Comments.Comments.SelectMany(x => x.Body?.Content.SelectMany(b => b.Content).Select(x => x.Text)).Where(x => !(x is null)).LastOrDefault(c => releaseNoteRegex.IsMatch(c));
            return !string.IsNullOrWhiteSpace(releaseNote)
                ? releaseNoteRegex.Replace(releaseNote, "")?.Trim() ?? string.Empty
                : issue.Fields.Summary ?? issue.Key;
        }
    }
}