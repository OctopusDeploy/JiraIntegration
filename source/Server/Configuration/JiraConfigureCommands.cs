﻿using System;
using System.Collections.Generic;
using System.Threading;
using Octopus.Diagnostics;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Configuration;

namespace Octopus.Server.Extensibility.JiraIntegration.Configuration
{
    internal class JiraConfigureCommands : IContributeToConfigureCommand
    {
        private readonly Lazy<IJiraConfigurationStore> jiraConfiguration;
        private readonly ISystemLog systemLog;

        public JiraConfigureCommands(
            ISystemLog systemLog,
            Lazy<IJiraConfigurationStore> jiraConfiguration)
        {
            this.systemLog = systemLog;
            this.jiraConfiguration = jiraConfiguration;
        }

        public IEnumerable<ConfigureCommandOption> GetOptions()
        {
            yield return new ConfigureCommandOption("jiraIsEnabled=", "Set whether Jira Integration is enabled.", v =>
            {
                var isEnabled = bool.Parse(v);
                jiraConfiguration.Value.SetIsEnabled(isEnabled, CancellationToken.None);
                systemLog.Info($"Jira Integration IsEnabled set to: {isEnabled}");
            });
            yield return new ConfigureCommandOption("jiraBaseUrl=", JiraConfigurationResource.JiraBaseUrlDescription,
                v =>
                {
                    jiraConfiguration.Value.SetBaseUrl(v, CancellationToken.None);
                    systemLog.Info($"Jira Integration base Url set to: {v}");
                });
            yield return new ConfigureCommandOption("jiraConnectAppUrl=", "Set the URL for the Jira Connect App", v =>
            {
                jiraConfiguration.Value.SetConnectAppUrl(v, CancellationToken.None);
                systemLog.Info($"Jira Integration ConnectAppUrl set to: {v}");
            }, true);
        }
    }
}