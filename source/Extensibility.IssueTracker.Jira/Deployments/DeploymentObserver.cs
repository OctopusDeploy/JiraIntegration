using Octopus.Server.Extensibility.Extensions.Domain;
using Octopus.Server.Extensibility.IssueTracker.Jira.Configuration;

namespace Octopus.Server.Extensibility.IssueTracker.Jira.Deployments
{
    public class DeploymentObserver : IObserveDomainEvent<DeploymentEvent>
    {
        private readonly IJiraConfigurationStore _store;

        public DeploymentObserver(IJiraConfigurationStore store)
        {
            _store = store;
        }

        public void Handle(DeploymentEvent @event)
        {
            if (!_store.GetIsEnabled())
                return;

            // Push data to Jira
        }
    }
}