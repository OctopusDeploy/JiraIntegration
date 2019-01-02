using System;
using System.Collections.Generic;
using System.Net;
using Octopus.Diagnostics;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Configuration;

namespace Octopus.Server.Extensibility.IssueTracker.Jira.Configuration
{
    public class JiraConfigureCommands : IContributeToConfigureCommand
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
            yield return new ConfigureCommandOption("jiraIsEnabled=", "Set whether Jira issue tracker integration is enabled.", v =>
            {
                var isEnabled = bool.Parse(v);
                jiraConfiguration.Value.SetIsEnabled(isEnabled);
                log.Info($"Jira Issue Tracker integration IsEnabled set to: {isEnabled}");
            });
            yield return new ConfigureCommandOption("jiraBaseUrl=", JiraConfigurationResource.JiraBaseUrlDescription, v =>
            {
                jiraConfiguration.Value.SetBaseUrl(v);
                log.Info($"Jira Issue Tracker integration base Url set to: {v}");
            });
        }
    }
}