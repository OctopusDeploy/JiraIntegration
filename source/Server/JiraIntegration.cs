using System.Threading;
using System.Threading.Tasks;
using Octopus.Server.Extensibility.Extensions.WorkItems;
using Octopus.Server.Extensibility.JiraIntegration.Configuration;

namespace Octopus.Server.Extensibility.JiraIntegration
{
    internal class JiraIntegration : IIssueTracker
    {
        internal static string Name = "Jira";

        private readonly IJiraConfigurationStore configurationStore;

        public JiraIntegration(IJiraConfigurationStore configurationStore)
        {
            this.configurationStore = configurationStore;
        }

        public string CommentParser => JiraConfigurationStore.CommentParser;

        public string IssueTrackerName => Name;

        public async Task<bool> IsEnabled(CancellationToken cancellationToken)
        {
            return await configurationStore.GetIsEnabled(cancellationToken);
        }

        public async Task<string?> BaseUrl(CancellationToken cancellationToken)
        {
            return await configurationStore.GetIsEnabled(cancellationToken)
                ? await configurationStore.GetBaseUrl(cancellationToken)
                : null;
        }
    }
}