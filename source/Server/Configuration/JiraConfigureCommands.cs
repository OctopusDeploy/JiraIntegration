using System;
using System.Collections.Generic;
using Octopus.Diagnostics;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Configuration;

namespace Octopus.Server.Extensibility.JiraIntegration.Configuration
{
    class JiraConfigureCommands : IContributeToConfigureCommand
    {
        readonly ILog log;
        readonly Lazy<IJiraConfigurationStore> jiraConfiguration;

        public JiraConfigureCommands(
            ILog log,
            Lazy<IJiraConfigurationStore> jiraConfiguration)
        {
            this.log = log;
            this.jiraConfiguration = jiraConfiguration;
        }

        public IEnumerable<ConfigureCommandOption> GetOptions()
        {
            yield return new ConfigureCommandOption("jiraIsEnabled=", "Set whether Jira Integration is enabled.", v =>
            {
                var isEnabled = bool.Parse(v);
                jiraConfiguration.Value.SetIsEnabled(isEnabled);
                log.Info($"Jira Integration IsEnabled set to: {isEnabled}");
            });
            yield return new ConfigureCommandOption("jiraBaseUrl=", JiraConfigurationResource.JiraBaseUrlDescription, v =>
            {
                jiraConfiguration.Value.SetBaseUrl(v);
                log.Info($"Jira Integration base Url set to: {v}");
            });
            yield return new ConfigureCommandOption("jiraConnectAppUrl=", "Set the URL for the Jira Connect App", v =>
            {
                jiraConfiguration.Value.SetConnectAppUrl(v);
                log.Info($"Jira Integration ConnectAppUrl set to: {v}");
            }, hide: true);
        }
    }
}