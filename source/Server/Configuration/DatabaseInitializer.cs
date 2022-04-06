using System;
using Octopus.Data.Storage.Configuration;
using Octopus.Diagnostics;
using Octopus.Server.Extensibility.Extensions.Infrastructure;

namespace Octopus.Server.Extensibility.JiraIntegration.Configuration
{
    class DatabaseInitializer : ExecuteWhenDatabaseInitializes
    {
        readonly ISystemLog systemLog;
        readonly IConfigurationStore configurationStore;

        public DatabaseInitializer(ISystemLog systemLog, IConfigurationStore configurationStore)
        {
            this.systemLog = systemLog;
            this.configurationStore = configurationStore;
        }

        public override void Execute()
        {
            var doc = configurationStore.Get<JiraConfiguration>(JiraConfigurationStore.SingletonId);
            if (doc != null)
                return;

            var oldConfiguration = configurationStore.Get<JiraConfigurationWithSettableId>("issuetracker-jira");
            if (oldConfiguration != null)
            {
                configurationStore.Delete(oldConfiguration);
                oldConfiguration.Id = "jira-integration";
                configurationStore.Create(oldConfiguration);
                return;
            }

            systemLog.Info("Initializing Jira integration settings");
            doc = new JiraConfiguration();
            configurationStore.Create(doc);
        }

        class JiraConfigurationWithSettableId : JiraConfiguration
        {
            public new string Id { get; set; } = string.Empty;
        }
    }
}
