using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Octopus.Data;
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

        public async Task<bool> IsEnabled(CancellationToken cancellationToken)
        {
            return await store.GetIsEnabled(cancellationToken);
        }

        public async Task<IResultFromExtension<WorkItemLink[]>> Map(OctopusBuildInformation buildInformation, CancellationToken cancellationToken)
        {
            if (!await IsEnabled(cancellationToken))
                return ResultFromExtension<WorkItemLink[]>.ExtensionDisabled();
            if (string.IsNullOrEmpty(await store.GetJiraUsername(cancellationToken)) ||
                string.IsNullOrEmpty((await store.GetJiraPassword(cancellationToken))?.Value))
                return ResultFromExtension<WorkItemLink[]>.Failed("Username/password not configured");

            var baseUrl = await store.GetBaseUrl(cancellationToken);
            if (string.IsNullOrWhiteSpace(baseUrl))
                return ResultFromExtension<WorkItemLink[]>.Failed("No BaseUrl configured");

            var releaseNotePrefix = await store.GetReleaseNotePrefix(cancellationToken);
            var workItemIds = commentParser.ParseWorkItemIds(buildInformation).Distinct().ToArray();
            if (workItemIds.Length == 0)
            {
                return ResultFromExtension<WorkItemLink[]>.Success(Array.Empty<WorkItemLink>());
            }

            return await TryConvertWorkItemLinks(workItemIds, releaseNotePrefix, baseUrl, cancellationToken);
        }

        private async Task<IResultFromExtension<WorkItemLink[]>> TryConvertWorkItemLinks(string[] workItemIds, string? releaseNotePrefix, string baseUrl, CancellationToken cancellationToken)
        {
            var response = await jira.Value.GetIssues(workItemIds.ToArray(), cancellationToken);

            if (response is IFailureResult failureResult)
            {
                return ResultFromExtension<WorkItemLink[]>.Failed(failureResult.Errors);
            }

            var issues = ((ISuccessResult<JiraIssue[]>)response).Value;
            if (issues.Length == 0)
            {
                return ResultFromExtension<WorkItemLink[]>.Success(Array.Empty<WorkItemLink>());
            }

            var issueMap = issues.ToDictionary(x => x.Key, StringComparer.OrdinalIgnoreCase);

            var workItemsNotFound = workItemIds.Where(x => !issueMap.ContainsKey(x)).ToArray();
            if (workItemsNotFound.Length > 0)
            {
                systemLog.Warn($"Parsed work item ids {string.Join(", ", workItemsNotFound)} from commit messages but could not locate them in Jira");
            }

            var releaseNoteRegex = new Regex($"^{releaseNotePrefix}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            return ResultFromExtension<WorkItemLink[]>.Success(workItemIds
                .Where(workItemId => issueMap.ContainsKey(workItemId))
                .Select(workItemId =>
                {
                    var issue = issueMap[workItemId];
                    return new WorkItemLink
                    {
                        Id = issue.Key,
                        Description = GetReleaseNote(issueMap[workItemId], releaseNotePrefix, releaseNoteRegex),
                        LinkUrl = baseUrl + "/browse/" + workItemId,
                        Source = JiraConfigurationStore.CommentParser
                    };
                })
                // ReSharper disable once RedundantEnumerableCastCall
                .Cast<WorkItemLink>() // cast back from `WorkItemLink?` type to keep the compiler happy
                .ToArray());
        }

        public string GetReleaseNote(JiraIssue issue, string? releaseNotePrefix, Regex? releaseNoteRegex)
        {
            if (issue.Fields.Comments.Total == 0 || string.IsNullOrWhiteSpace(releaseNotePrefix))
                return issue.Fields.Summary;

            var releaseNote = issue.Fields.Comments.Comments.Select(x => x.Body).Where(x => x is not null).LastOrDefault(releaseNoteRegex.IsMatch);
            return !string.IsNullOrWhiteSpace(releaseNote)
                ? releaseNoteRegex.Replace(releaseNote, String.Empty).Trim()
                : issue.Fields.Summary ?? issue.Key;
        }
    }
}