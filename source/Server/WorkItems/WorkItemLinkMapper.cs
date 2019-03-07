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

        public WorkItemLinkMapper(IJiraConfigurationStore store)
        {
            this.store = store;
        }

        public string IssueTrackerId => JiraConfigurationStore.SingletonId;
        public bool IsEnabled => store.GetIsEnabled();

        public WorkItemLink[] Map(OctopusPackageMetadata packageMetadata)
        {
            if (packageMetadata.IssueTrackerId != IssueTrackerId)
                return null;

            var baseUrl = store.GetBaseUrl();
            if (string.IsNullOrWhiteSpace(baseUrl))
                return null;

            var isEnabled = store.GetIsEnabled();

            return packageMetadata.WorkItems.Select(wi => new WorkItemLink
                {
                    Id = wi.Id,
                    LinkText = wi.LinkText,
                    LinkUrl = isEnabled ? baseUrl + "/browse/" + wi.LinkData : null
                })
                .ToArray();
        }
    }
}