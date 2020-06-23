using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Octopus.Server.Extensibility.Extensions.WorkItems;
using Octopus.Server.Extensibility.HostServices.Model.BuildInformation;
using Octopus.Server.Extensibility.JiraIntegration.Configuration;
using Octopus.Server.Extensibility.JiraIntegration.Integration;
using Octopus.Server.Extensibility.Resources.IssueTrackers;
using Octopus.Server.Extensibility.Results;

namespace Octopus.Server.Extensibility.JiraIntegration.WorkItems
{
    class WorkItemLinkMapper : IWorkItemLinkMapper
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

        public ResultFromExtension<WorkItemLink[]> Map(OctopusBuildInformation buildInformation)
        {
            if (!IsEnabled || 
                string.IsNullOrEmpty(store.GetJiraUsername()) || 
                string.IsNullOrEmpty(store.GetJiraPassword()?.Value))
                return ResultFromExtension<WorkItemLink[]>.ExtensionDisabled();

            var baseUrl = store.GetBaseUrl();
            if (string.IsNullOrWhiteSpace(baseUrl))
                return ResultFromExtension<WorkItemLink[]>.Failed("No BaseUrl configured");

            var releaseNotePrefix = store.GetReleaseNotePrefix();
            var workItemIds = commentParser.ParseWorkItemIds(buildInformation).Distinct();

            return ResultFromExtension<WorkItemLink[]>.Success(ConvertWorkItemLinks(workItemIds, releaseNotePrefix, baseUrl));
        }

        private WorkItemLink[] ConvertWorkItemLinks(IEnumerable<string> workItemIds, string? releaseNotePrefix, string baseUrl)
        {
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
                .Cast<WorkItemLink>() // cast back from `WorkItemLink?` type to keep the compiler happy
                .ToArray();
        }

        public string GetReleaseNote(JiraIssue issue, string? releaseNotePrefix)
        {
            if (issue.Fields.Comments.Total == 0 || string.IsNullOrWhiteSpace(releaseNotePrefix))
                return issue.Fields.Summary;

            var releaseNoteRegex = new Regex($"^{releaseNotePrefix}", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var issueComments = jira.Value.GetIssueComments(issue.Key).GetAwaiter().GetResult();

            var releaseNote = issueComments?.Comments.LastOrDefault(c => releaseNoteRegex.IsMatch(c.Body))?.Body;
            return !string.IsNullOrWhiteSpace(releaseNote)
                ? releaseNoteRegex.Replace(releaseNote, "")?.Trim() ?? string.Empty
                : issue.Fields.Summary ?? issue.Key;
        }
    }
}