using Octopus.Data.Storage.Configuration;
using Octopus.Diagnostics;
using Octopus.Server.Extensibility.Extensions.Infrastructure;

namespace Octopus.Server.Extensibility.JiraIntegration.Configuration
{
    class DatabaseInitializer : ExecuteWhenDatabaseInitializes
    {
        readonly ILog log;
        readonly IConfigurationStore configurationStore;

        public DatabaseInitializer(ILog log, IConfigurationStore configurationStore)
        {
            this.log = log;
            this.configurationStore = configurationStore;
        }

        public override void Execute()
        {
            var doc = configurationStore.Get<JiraConfiguration>(JiraConfigurationStore.SingletonId);
            if (doc != null)
                return;

            var oldConfiguration = configurationStore.Get<JiraConfiguration>("issuetracker-jira");
            if (oldConfiguration != null)
            {
                configurationStore.Delete(oldConfiguration);
                oldConfiguration.Id = "jira-integration";
                configurationStore.Create(oldConfiguration);
                return;
            }

            log.Info("Initializing Jira integration settings");
            doc = new JiraConfiguration();
            configurationStore.Create(doc);
        }
    }
}