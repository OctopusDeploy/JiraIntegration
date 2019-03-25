using System.Linq;
using Octopus.Server.Extensibility.Extensions.WorkItems;
using Octopus.Server.Extensibility.HostServices.Model.PackageMetadata;
using Octopus.Server.Extensibility.IssueTracker.Jira.Configuration;
using Octopus.Server.Extensibility.Resources.IssueTrackers;

namespace Octopus.Server.Extensibility.IssueTracker.Jira.WorkItems
{
    public class WorkItemLinkMapper : IWorkItemLinkMapper
    {
        private readonly IJiraConfigurationStore store;
        private readonly CommentParser commentParser;

        public WorkItemLinkMapper(IJiraConfigurationStore store,
            CommentParser commentParser)
        {
            this.store = store;
            this.commentParser = commentParser;
        }

        public string CommentParser => JiraConfigurationStore.CommentParser;
        public bool IsEnabled => store.GetIsEnabled();

        public WorkItemLink[] Map(OctopusPackageMetadata packageMetadata)
        {
            if (packageMetadata.CommentParser != CommentParser)
                return null;

            var baseUrl = store.GetBaseUrl();
            if (string.IsNullOrWhiteSpace(baseUrl))
                return null;

            var isEnabled = store.GetIsEnabled();

            var workItemIds = commentParser.ParseWorkItemIds(packageMetadata);

            return workItemIds.Select(workItemId => new WorkItemLink
                {
                    Id = workItemId,
                    Description = workItemId,
                    LinkUrl = isEnabled ? baseUrl + "/browse/" + workItemId : null
                })
                .ToArray();
        }
    }
}