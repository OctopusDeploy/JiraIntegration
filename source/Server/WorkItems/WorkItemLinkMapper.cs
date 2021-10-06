using System;
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
                return ResultFromExtension<WorkItemLink[]>.Success(Array.Empty<WorkItemLink>());

            var (isSuccess, errorMessage) = TryConvertWorkItemLinks(workItemIds, releaseNotePrefix, baseUrl, out var links);
            return isSuccess ? ResultFromExtension<WorkItemLink[]>.Success(links) : ResultFromExtension<WorkItemLink[]>.Failed(errorMessage);
        }

        private (bool isSuccess, string? errorMessage) TryConvertWorkItemLinks(string[] workItemIds, string? releaseNotePrefix, string baseUrl, out WorkItemLink[] links)
        {
            var response = jira.Value.GetIssues(workItemIds.ToArray()).GetAwaiter().GetResult();

            if (!response.IsSuccess)
            {
                links = Array.Empty<WorkItemLink>();
                return (false, response.ErrorMessage);
            }

            if (response.Result.Issues.Length == 0)
            {
                links = Array.Empty<WorkItemLink>();
                return (true, null);
            }

            var issueMap = response.Result.Issues.ToDictionary(x => x.Key, StringComparer.OrdinalIgnoreCase);

            var workItemsNotFound = workItemIds.Where(x => !issueMap.ContainsKey(x)).ToArray();
            if (workItemsNotFound.Length > 0)
            {
                systemLog.Warn($"Parsed work item ids {string.Join(", ", workItemsNotFound)} from commit messages but could not locate them in Jira");
            }

            links = workItemIds
                .Where(workItemId => issueMap.ContainsKey(workItemId))
                .Select(workItemId =>
                {
                    var issue = issueMap[workItemId];
                    return new WorkItemLink
                    {
                        Id = issue.Key,
                        Description = GetReleaseNote(issueMap[workItemId], releaseNotePrefix),
                        LinkUrl = baseUrl + "/browse/" + workItemId,
                        Source = JiraConfigurationStore.CommentParser
                    };
                })
                .Where(i => i != null)
                // ReSharper disable once RedundantEnumerableCastCall
                .Cast<WorkItemLink>() // cast back from `WorkItemLink?` type to keep the compiler happy
                .ToArray();

            return (true, null);
        }

        public string GetReleaseNote(JiraIssue issue, string? releaseNotePrefix)
        {
            if (issue.Fields.Comments.Total == 0 || string.IsNullOrWhiteSpace(releaseNotePrefix))
                return issue.Fields.Summary;

            var releaseNoteRegex = new Regex($"^{releaseNotePrefix}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            var releaseNote = issue.Fields.Comments.Comments.Select(x => x.Body).Where(x => x is not null).LastOrDefault(c => releaseNoteRegex.IsMatch(c));
            return !string.IsNullOrWhiteSpace(releaseNote)
                ? releaseNoteRegex.Replace(releaseNote, "")?.Trim() ?? string.Empty
                : issue.Fields.Summary ?? issue.Key;
        }
    }
}